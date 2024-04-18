using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using NoahsArk.SharedRhino;

namespace NoahsArk.PluginGrasshopper
{
    public class MeshSplitAlongPlaneComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MeshSplitAlongPlaneComponent()
          : base("MeshSplitAlongPlane", "MSAP",
            "Split a mesh into pieces along an array of planes",
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
            pManager.AddPlaneParameter("Planes", "P", "Planes along which to split the mesh", GH_ParamAccess.list);
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
            List<Plane> planes = new List<Plane>();
            if (!DA.GetDataList<Plane>(1, planes))
                return;

            MeshSplitter.SplitAlongPlanes(mesh, planes.ToArray(), out Mesh[] meshes);

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
        public override Guid ComponentGuid => new Guid("6BEDB879-A730-4550-AFE3-850E08E1D4F7");
    }
}