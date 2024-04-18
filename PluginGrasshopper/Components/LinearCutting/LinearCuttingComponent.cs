using System;
using System.Collections.Generic;
using System.Linq;

using NoahsArk.Shared.LinearCutting;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace NoahsArk.PluginGrasshopper
{
    /// <summary>
    /// See https://en.wikipedia.org/wiki/Cutting_stock_problem
    /// Source code copied and adapted from https://github.com/AlexanderMorozovDesign/GH_Linear_Cutting
    /// Credits to Alexander Morozov.
    /// </summary>
    public class LinearCuttingComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public LinearCuttingComponent() : base("1D Linear Cutting", "1DLinCut", "1D Linear Cutting program", "NoahsArk", "Fabrication")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Desired lengths", "L", "Lengths of desired workpieces", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Amount", "A", "Amount of workpieces for each length", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Blank length", "B", "Length of the blanks", GH_ParamAccess.item, 6000);
            pManager.AddIntegerParameter("End cut width", "E", "Cut width at one end of the blanks", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("Tool width", "W", "Tool width", GH_ParamAccess.item, 5);
            pManager.AddIntegerParameter("Headless retreat", "H", "TODO: To be clarified", GH_ParamAccess.item, 100);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Groups", "G", "Lengths of workpieces for each group", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Amount", "A", "Amount for each group", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Waste", "W", "Waste for each group", GH_ParamAccess.list);
            pManager.AddIntegerParameter("UsingLen", "U", "Used length for each group", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<int> desiredLengths = new List<int>();  // cutting length 
            List<int> amount = new List<int>();  // quantity for each position for cutting
            int whipLength = 6000; // cutting length
            int endSawCut = 10; // end cut
            int toolWidth = 5;  // tool saw width
            int headlessRetreat = 100;  // unconditional withdrawal

            // бdata entry block in the API interface of my program
            if (!DA.GetDataList(0, desiredLengths)) return;
            if (!DA.GetDataList(1, amount)) return;
            if (!DA.GetData(2, ref whipLength)) return;
            if (!DA.GetData(3, ref endSawCut)) return;
            if (!DA.GetData(4, ref toolWidth)) return;
            if (!DA.GetData(5, ref headlessRetreat)) return;

            // sanity check
            if (desiredLengths.Count != amount.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Desired lengths and amount must have the same length.");
                return;
            }

            // class for nesting calculation
            var linerCutting = new LinearCutting(desiredLengths, amount, whipLength, endSawCut, toolWidth, headlessRetreat);
            List<List<int>> cuts = linerCutting.GetCuts();

            GH_Structure<GH_Integer> Tree = new GH_Structure<GH_Integer>(); // list of lists with cutting for each whip

            for (int i = 0; i < cuts.Count; i++)
            {
                Tree.AppendRange(cuts[i].Select(j => new GH_Integer(j)), new GH_Path(new int[] { 0, i }));
            }

            List<int> repeats = linerCutting.GetRepeats();   //list of the number of repetitions of the cutting mask per whip
            List<int> retreat = linerCutting.GetRetreats();  // waste list for each whip
            List<int> usingLength = linerCutting.GetUsingLength();  // list of used lengths for cutting for each workpiece

            // the output block of the received data in the API interface of my program
            DA.SetDataTree(0, Tree);  // list of lists with cutting for each whip
            DA.SetDataList(1, repeats);  //list of the number of repetitions of the cutting mask on the whip
            DA.SetDataList(2, retreat);  // waste list for each whip
            DA.SetDataList(3, usingLength); // list of used lengths for each piece
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => ResourceLoader.LoadBitmap("NoahsArkIcon_24.png");


        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C9901E91-5A52-4CC4-8B20-7A59C93B8D3C"); }
        }
    }
}
