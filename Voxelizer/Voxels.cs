using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Voxelizer
{
    class Voxels
    {

        public List<Box> voxels;
        public List<Point3d> points;
        public double max_x;
        public double min_x;
        public double max_y;
        public double min_y;
        public double max_z;
        public double min_z;

        public Voxels(List<Box> voxels, List<Point3d> points, double max_x, double min_x, double max_y, double min_y, double max_z, double min_z)
        {
            this.voxels = voxels;
            this.points = points;

            this.max_x = max_x;
            this.min_x = min_x;
            this.max_y = max_y;
            this.min_y = min_y;
            this.max_z = max_z;
            this.min_z = min_z;

        }
    }
}
