﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoahsArk.Shared.LinearCutting
{
    /// <summary>
    /// See https://en.wikipedia.org/wiki/Cutting_stock_problem
    /// Source code copied and adapted from https://github.com/AlexanderMorozovDesign/GH_Linear_Cutting
    /// Credits to Alexander Morozov.
    /// </summary>
    public class LinearCutting
    {
        /// <summary>
        /// Lengths of required workpieces
        /// </summary>
        List<int> desiredLengths;
        /// <summary>
        /// Number of workpieces for each length
        /// </summary>
        List<int> amount;
        /// <summary>
        /// Blank length
        /// </summary>
        int whipLength;               
        /// <summary>
        /// End cut
        /// </summary>
        int endSawCut;               
        /// <summary>
        /// Tool width
        /// </summary>
        int toolWidth;                
        /// <summary>
        /// Unconditional withdrawal
        /// TODO: to be clarified
        /// </summary>
        int headlessRetreat;

        /// <summary>
        /// Lengths of blanks for each whip
        /// </summary>
        List<List<int>> cuts;
        /// <summary>
        /// Number of repetitions of each whip
        /// </summary>
        List<int> repeats;
        /// <summary>
        /// Residue on every whip
        /// </summary>
        List<int> retreats;
        /// <summary>
        /// Used length of each whip
        /// </summary>
        List<int> usingLength;        

        //
        double[] schet = { 0, 0, 0, 0 };
        double[] schetDl = { 0, 0, 0, 0 };

        int cut, bez, torc, line, maxNumb, maxNumbMem;
        int x, j, i, y, zvr, hl, pop, verif;
        int maxRes, sum, sdv, pov;

        int[] l;
        int[] k;
        int[] z;
        int[] zost;
        int[,] lovr;
        int[,] kvr;
        int[] maxNumbVr;
        int[] sumLine;

        int[,] lvr;
        int[] q;
        int[] w;

        int[,] lo;
        int[] kol;
        int[] p;
        int[,] res;

        /// <summary>
        /// Lengths of blanks for each whip
        /// </summary>
        /// <returns></returns>
        public List<List<int>> GetCuts() => cuts;

        /// <summary>
        /// Number of repetitions per whip
        /// </summary>
        /// <returns></returns>
        public List<int> GetRepeats() => repeats;

        /// <summary>
        /// Residue per whip
        /// </summary>
        /// <returns></returns>
        public List<int> GetRetreats() => retreats;

        /// <summary>
        /// Used length of each whip
        /// </summary>
        /// <returns></returns>
        public List<int> GetUsingLength() => usingLength;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="desiredLengths_">Lengths of required workpieces</param>
        /// <param name="amount_">Number of blanks for each length</param>
        /// <param name="whipLength_">Whip length</param>
        /// <param name="endSawCut_">End cut</param>
        /// <param name="toolWidth_">Tool width</param>
        /// <param name="headlessRetreat_">
        /// Uncoditional withdrawal
        /// TODO: to be clarified
        /// </param>
        public LinearCutting(List<int> desiredLengths_, List<int> amount_, int whipLength_, int endSawCut_, int toolWidth_, int headlessRetreat_)
        {
            desiredLengths = new List<int>(desiredLengths_);
            amount = new List<int>(amount_);
            whipLength = whipLength_;
            endSawCut = endSawCut_;
            toolWidth = toolWidth_;
            headlessRetreat = headlessRetreat_;

            if (desiredLengths.Count != amount.Count)
                throw new ArgumentException("Lengths and amounts must have the same length");

            Calculate();
        }

        /// <summary>
        /// Trimming zero positions
        /// </summary>
        void CutMass()
        {
            for (i = w[y]; i <= maxNumbVr[x] - 1; i++)
            {
                kvr[i, x] = kvr[i + 1, x];
                lvr[i, x] = lvr[i + 1, x];
            }
            kvr[maxNumbVr[x], x] = 0;
            lvr[maxNumbVr[x], x] = 0;
        }

        /// <summary>
        /// Base
        /// </summary>
        void BaseLine()
        {
            zvr = line / l[1];
            if (zvr > k[1])
                zvr = k[1];

            for (j = 1; j <= zvr; j++)
            {
                for (i = 1; i <= j; i++)
                    lovr[i, j] = l[1];
                kvr[1, j] = k[1] - j;
                maxNumbVr[j] = maxNumb;
                sumLine[j] = j * l[1];
                sumLine[j + 1] = 0;
            }
            for (j = 1; j <= zvr; j++)
            {
                lvr[1, j] = l[1];
                for (i = 2; i <= maxNumb; i++)
                {
                    kvr[i, j] = k[i];
                    lvr[i, j] = l[i];
                }
            }
            if (kvr[1, zvr] == 0)
            {
                x = zvr;
                CutMass();
            }
        }

        /// <summary>
        /// Algorithm for preparing an array for calculations
        /// </summary>
        void Optimize()
        {
            int len = line - sumLine[x];
            int L;
            int dif;
            for (i = 1; i <= len; i++)
            {
                q[i] = 0;
                w[i] = 0;
                for (j = 1; j <= maxNumbVr[x]; j++)
                {
                    L = lvr[j, x];
                    dif = i - L;
                    if (dif == 0)
                    {
                        q[i] = L;
                        w[i] = j;
                    }
                    else
                        if (dif > 0 && q[i] < (q[dif] + L))
                    {
                        q[i] = q[dif] + L;
                        w[i] = j;
                    }
                }
            }
        }

        /// <summary>
        /// Formation of nesting for the remainder
        /// </summary>
        void OstLine()
        {
            int kv;
            int ind = w[y];
            while (w[y] > 0)
            {
                kv = kvr[ind, x];
                if (kv > 0)
                {
                    kvr[ind, x] = kv - 1;
                    zost[x] = zost[x] + 1;
                    lovr[x, zost[x] + x] = lvr[ind, x];
                    sumLine[x] = sumLine[x] + lvr[ind, x];
                    y = y - lvr[ind, x];
                    ind = w[y];
                    continue;
                }
                else
                    if (maxNumbVr[x] > 1)
                {
                    CutMass();
                    Optimize();
                    ind = w[y];
                    continue;
                }
                if (kv == 0 && maxNumbVr[x] == 1)
                {
                    y = 1;
                    return;
                }
            }
        }

        void Result()
        {
            cuts = new List<List<int>>();
            repeats = new List<int>();
            retreats = new List<int>();
            usingLength = new List<int>();

            List<int> workpieces;
            for (i = 1; i <= hl - 1; i++)
            {
                x = 0;
                sum = 0;
                kol[i] = 1;
                res[i + 1, 1] = i;
                res[i + 1, 4] = kol[i];

                repeats.Add(kol[i]); //Number of repetitions of each whip
                workpieces = new List<int>();
                for (j = 1; j <= z[i]; j++)
                {
                    if (z[i] > maxRes)
                        maxRes = z[i];
                    res[i + 1, j + 4] = lo[i, j] - cut;
                    sum = sum + lo[i, j];
                    schet[3] = schet[3] + 1;
                    schetDl[3] = schetDl[3] + lo[i, j] - cut;
                    workpieces.Add(res[i + 1, j + 4]); //The size of the workpiece in the whip
                    if (i > 1 && res[i + 1, j + 4] == res[i, j + 4])
                        x = x + 1;
                    else
                    {
                        x = 0;
                        pov = 0;
                    }
                }
                cuts.Add(workpieces);
                res[i + 1, 2] = sum + torc;
                res[i + 1, 3] = line + torc - res[i + 1, 2];
                usingLength.Add(res[i + 1, 2]);  //used
                retreats.Add(res[i + 1, 3]);     //Remainder

                if (x == z[i] && z[i] == z[i - 1])
                {
                    kol[i - 1 - pov] = kol[i - 1 - pov] + 1;            //Increase the number of repetitions of the previous whip
                    repeats[repeats.Count - 2] = kol[i - 1 - pov];
                    pov = pov + 1;
                    usingLength.Remove(usingLength.Last());
                    retreats.Remove(retreats.Last());
                    repeats.Remove(repeats.Last());
                    cuts.Remove(cuts.Last());
                    sdv = sdv + 1;
                }
            }

            if (schet[1] == schet[2] && schet[1] == schet[3] && schetDl[1] == schetDl[2] && schetDl[1] == schetDl[3])
                return;
            else
            {
                throw new Exception("Error! Possibility of incorrect cutting");
                //return false;
            }
        }

        void ResizeArray(ref int[,] original, int rows, int cols)
        {
            var newArray = new int[rows, cols];
            int minRows = Math.Min(rows, original.GetLength(0));
            int minCols = Math.Min(cols, original.GetLength(1));
            for (int i = 0; i < minRows; i++)
                for (int j = 0; j < minCols; j++)
                    newArray[i, j] = original[i, j];
            original = newArray;
        }

        void ResizeArrays(int n)
        {
            ResizeArray(ref kvr, maxNumb + 1, n);
            ResizeArray(ref lvr, maxNumb + 1, n);
            ResizeArray(ref kvr, maxNumb + 1, n);
            ResizeArray(ref lovr, n, n);
            ResizeArray(ref lo, n, n);
            ResizeArray(ref res, n, n);

            Array.Resize(ref z, n);
            Array.Resize(ref zost, n);
            Array.Resize(ref maxNumbVr, n);
            Array.Resize(ref sumLine, n);
            Array.Resize(ref kol, n);
            Array.Resize(ref p, n);
        }

        void Calculate()
        {
            cut = toolWidth;
            bez = headlessRetreat;
            torc = endSawCut;
            line = whipLength - torc;
            maxNumb = amount.Count;
            maxNumbMem = maxNumb;

            int max_it = 200; //maxNumb

            l = new int[maxNumb + 2];
            k = new int[maxNumb + 2];
            kvr = new int[maxNumb + 1, max_it + 1];
            lvr = new int[maxNumb + 1, max_it + 1];
            res = new int[max_it + 1, max_it + 1];
            q = new int[line + 1];
            w = new int[line + 1];

            z = new int[max_it + 1];
            zost = new int[max_it + 1];
            lovr = new int[max_it + 1, max_it + 1];
            maxNumbVr = new int[max_it + 1];
            sumLine = new int[max_it + 1];

            lo = new int[max_it + 1, max_it + 1];
            kol = new int[max_it + 1];
            p = new int[max_it + 1];

            //Reading into an array
            j = 1;
            foreach (int len in desiredLengths)
            {
                l[j] = len + cut;
                if (l[j] > line)
                    throw new Exception($"Desired length {len} + tool width {cut} + end cut width {torc} is longer than blank length {whipLength}.");
                k[j] = amount[j - 1];
                schet[1] += k[j];
                schetDl[1] += (l[j] - cut) * k[j];
                j++;
            }

            //Descending sort
            int vl;
            int vk;
            for (i = 1; i <= maxNumb - 1; i++)
                for (j = i + 1; j <= maxNumb; j++)
                    if (l[j] > l[i])
                    {
                        vl = l[j];
                        vk = k[j];
                        l[j] = l[i];
                        k[j] = k[i];
                        l[i] = vl;
                        k[i] = vk;
                    }

            hl = 1;
            z[1] = 0;
            y = line;

            //Control algorithm
            while (k[1] > 0)
            {
                for (i = 1; i <= max_it; i++)
                    zost[i] = 0;

                BaseLine();

                for (x = 1; x <= zvr; x++)
                {
                    y = line - sumLine[x];
                    Optimize();
                    OstLine();
                }
                p[1] = zost[1] + 1;
                pop = 1;
                for (x = 2; x <= zvr; x++)
                {
                    verif = line - sumLine[x] - bez;
                    if (verif < 0)
                    {
                        p[x] = x + zost[x];
                        if (p[x] < p[x - 1])
                            pop = x;
                    }
                    else
                        if (sumLine[x] > sumLine[x - 1])
                        pop = x;
                }
                for (i = 1; i <= pop; i++)
                {
                    lo[hl, i] = l[1];
                    k[1] = k[1] - 1;
                    schet[2] = schet[2] + 1;
                    schetDl[2] = schetDl[2] + lo[hl, i] - cut;
                }
                for (j = 1; j <= zost[pop]; j++)
                {
                    lo[hl, j + pop] = lovr[pop, j + pop];
                    schet[2] = schet[2] + 1;
                    schetDl[2] = schetDl[2] + lo[hl, j + pop] - cut;
                }
                for (i = 1; i <= maxNumb; i++)
                {
                    k[i] = kvr[i, pop];
                    l[i] = lvr[i, pop];
                }
                z[hl] = pop + zost[pop];
                hl = hl + 1;
                if (hl == max_it)
                {
                    max_it *= 2;
                    ResizeArrays(max_it + 1);
                    //Result();
                    //return;
                }

                if (maxNumb > 1)
                    for (j = 1; j <= 3; j++)
                    {
                        if (k[1] == 0)
                        {
                            for (i = 1; i <= maxNumb; i++)
                            {
                                k[i] = k[i + 1];
                                l[i] = l[i + 1];
                            }
                            k[maxNumb] = 0;
                            l[maxNumb] = 0;
                        }
                        else
                            break;
                    }
            }//while (k[1]>0)   
            Result();
        }

    }
}
