using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Voxelizer
{
    public class VoxelizerComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public VoxelizerComponent()
          : base("Voxelizer", "Voxelizer",
              "voxelizes a mesh",
              "Mesh", "Voxel")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
   
            pManager.AddMeshParameter("Mesh", "M", "Mesh to be voxelized", GH_ParamAccess.item);
            pManager.AddNumberParameter("Size", "S", "Length of each voxel. If the value is less than 0, the size is automatically determined", GH_ParamAccess.item, -1);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddBoxParameter("Voxels", "V", "Voxels as a collection of boxes", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "P", "Center points of each voxel", GH_ParamAccess.list);
            
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Mesh M = new Mesh();
            double S = 0.0;

            // Load input values of a mesh and size
            if (!DA.GetData(0, ref M)) return;
            DA.GetData(1, ref S);

            if (!(M is Mesh))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "M must be Mesh.");
            };

            Voxels voxels = Voxelizer.Run(M, S);
            
            // assign the output values
            DA.SetDataList(0, voxels.voxels);
            DA.SetDataList(1, voxels.points);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                return Properties.Resources.icon_1;
                
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("90f01e86-00b6-42f9-87a7-9e59183d4c5c"); }
        }
        
    }
}
