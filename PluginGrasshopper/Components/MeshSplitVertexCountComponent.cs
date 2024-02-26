using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using NoahsArk.SharedRhino;

namespace NoahsArk.PluginGrasshopper
{
    public class MeshSplitVertexCountComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MeshSplitVertexCountComponent()
          : base("MeshSplitVertexCount", "MSVC",
            "Split a mesh into pieces whose vertex count stays below a given limit",
            "NoahsArk", "Mesh")
        {
            MeshSplitter = new MeshSplitter();
        }

        IMeshSplitter MeshSplitter;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to split", GH_ParamAccess.item);
            pManager.AddIntegerParameter("VertexCount", "C", "Maximum vertex count", GH_ParamAccess.item, 65535);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Meshes", "M", "Resulting meshes", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            if (!DA.GetData<Mesh>(0, ref mesh))
                return;
            int maxVertexCount = 65535;
            if (!DA.GetData<int>(1, ref maxVertexCount))
                return;

            MeshSplitter.SplitForMaxVertexCount(mesh, out Mesh[] meshes, maxVertexCount);

            DA.SetDataList(0, meshes);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => ResourceLoader.LoadBitmap("NoahsArkIcon_24.png");

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("D97C3727-2123-425D-A443-3620998C99E1");
    }
}