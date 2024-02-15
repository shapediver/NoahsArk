using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NoahsArk.PluginGrasshopper
{
    public class PluginLoader : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.ComponentServer.AddCategoryIcon("NoahsArk", ResourceLoader.LoadBitmap("NoahsArkIcon_16.png"));
            Instances.ComponentServer.AddCategorySymbolName("NoahsArk", 'N');
            return GH_LoadingInstruction.Proceed;
        }
    }
}