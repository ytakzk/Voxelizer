using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Voxelizer
{
    public class OctreeVoxelizerComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public OctreeVoxelizerComponent()
          : base("OctreeVoxelizer", "OctreeVoxelizer",
              "voxelizes a mesh with Octree",
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
            pManager.AddIntegerParameter("Group", "G", "Permitted content per leaf", GH_ParamAccess.item, 2);
            
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
            int G = 2;

            // Load input values of a mesh and size
            if (!DA.GetData(0, ref M)) return;
            DA.GetData(1, ref S);
            DA.GetData(2, ref G);

            if (!(M is Mesh))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "M must be Mesh.");
            };

            Voxels voxels = Voxelizer.Run(M, S);

            // ocree
            Octree tree = new Octree(voxels.min_x, voxels.max_x, voxels.min_y, voxels.max_y, voxels.min_z, voxels.max_z, G);

            foreach (Point3d p in voxels.points)
            {

                tree.Add(p);

            }

            voxels.voxels.Clear();
            voxels.points.Clear();

            foreach (OctreeNode node in tree.getChildren())
            {

                Point3d center = node.getCenter();

                double size = node.getWidth();

                Plane grid_plane = new Plane(center, Plane.WorldXY.ZAxis);

                Interval interval = new Interval(-size, size);

                Box voxel = new Box(grid_plane, interval, interval, interval);

                voxels.voxels.Add(voxel);
                voxels.points.Add(center);

            }

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
                return Properties.Resources.icon_2;

            }
        }
 

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("9e57fd46-b15e-41b6-a651-e89b65b53b29"); }
        }
    }
}