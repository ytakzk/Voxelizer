using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Voxelizer
{
    class Voxelizer
    {

        public static Voxels Run(Mesh M, double S)
        {

            // Logic starts 
            var V = new List<Box>();
            var P = new List<Point3d>();

            M.Weld(Math.PI);

            // Get bounding of the mesh
            BoundingBox box = M.GetBoundingBox(Plane.WorldXY);

            double min_x = int.MaxValue;
            double max_x = int.MinValue;
            double min_y = int.MaxValue;
            double max_y = int.MinValue;
            double min_z = int.MaxValue;
            double max_z = int.MinValue;

            Point3d[] pts = box.GetCorners();

            foreach (Point3d p in pts)
            {

                if (p.X > max_x) { max_x = p.X; }

                if (p.X < min_x) { min_x = p.X; }

                if (p.Y > max_y) { max_y = p.Y; }

                if (p.Y < min_y) { min_y = p.Y; }

                if (p.Z > max_z) { max_z = p.Z; }

                if (p.Z < min_z) { min_z = p.Z; }

            }

            // If the size lacks or is an inappropriate value, set a reasonable value to it
            if (S <= 0)
            {

                double max_length = Math.Max(max_z - min_z, Math.Max(max_y - min_y, max_x - min_x));

                S = (int)(max_length / 10);

            }

            Interval unit_interval = new Interval(-S * 0.5, S * 0.5);

            // remap the bounding values taking the center position into account
            int num_x = (int)((max_x - min_x) / S);
            int num_y = (int)((max_y - min_y) / S);
            int num_z = (int)((max_z - min_z) / S);

            min_x = (max_x + min_x) * 0.5 - (num_x * S * 0.5);
            min_y = (max_y + min_y) * 0.5 - (num_y * S * 0.5);
            min_z = (max_z + min_z) * 0.5 - (num_z * S * 0.5);

            var horizontal_curves_by_plane = new Dictionary<Plane, Curve[]>();
            var vertical_curves_by_planes  = new Dictionary<Plane, Curve[]>();
           

            // vertical contours of the mesh
            for (var i = 0; i < num_z + 1; i++)
            {

                double z = min_z + i * S;

                Point3d p = new Point3d(0, 0, z);
                Plane plane = new Plane(p, Plane.WorldXY.Normal);

                Curve[] curves = Mesh.CreateContourCurves(M, plane);

                vertical_curves_by_planes[plane] = curves;

            }

            // horizontal contours of the mesh
            for (var i = 0; i < num_x + 1; i++)
            {

                double x = min_x + i * S;

                Point3d p = new Point3d(x, 0, 0);
                Plane plane = new Plane(p, Plane.WorldXY.XAxis);

                Curve[] curves = Mesh.CreateContourCurves(M, plane);

                horizontal_curves_by_plane[plane] = curves;
           
            }

            Point3d reference_min_pt = new Point3d(0, 0, min_z - int.MaxValue);
            Point3d reference_max_pt = new Point3d(0, 0, max_z + int.MaxValue);

            foreach (KeyValuePair<Plane, Curve[]> item in horizontal_curves_by_plane)
            {

                double x = item.Key.OriginX;
                Curve[] curves = item.Value;

                foreach (Curve curve in curves)
                {

                    PolylineCurve pl = curve.ToPolyline(0.01, 0.001, 0.0001, int.MaxValue);

                    Polyline polyline = pl.ToPolyline();

                    if (!polyline.IsClosed)
                    {

                        polyline.Add(polyline[0]);
                    }

                    Point3d min_pt = polyline.ClosestPoint(reference_min_pt);
                    Point3d max_pt = polyline.ClosestPoint(reference_max_pt);

                    foreach (Plane plane in vertical_curves_by_planes.Keys)
                    {

                        if (plane.OriginZ < min_pt.Z || max_pt.Z < plane.OriginZ)
                        {

                            continue;
                        }

                        NurbsCurve nurbs_curve = polyline.ToNurbsCurve();

                        var intersections = Rhino.Geometry.Intersect.Intersection.CurvePlane(nurbs_curve, plane, S * 0.0001);

                        if (intersections.Count < 2)
                        {

                            continue;

                        }

                        Point3d min_y_pt = new Point3d(0, int.MaxValue, 0);
                        Point3d max_y_pt = new Point3d(0, int.MinValue, 0);


                        foreach (Rhino.Geometry.Intersect.IntersectionEvent intersect in intersections)
                        {


                            if (min_y_pt.Y > intersect.PointA.Y)
                            {

                                min_y_pt = intersect.PointA;

                            }

                            if (max_y_pt.Y < intersect.PointA.Y)
                            {

                                max_y_pt = intersect.PointA;

                            }

                            int start_index = (int)((min_y_pt.Y - min_y) / S) + 1;
                            int end_index = (int)((max_y_pt.Y - min_y) / S) + 1;


                            for (var y_index = start_index; y_index < end_index; y_index++)
                            {

                                double y = min_y + S * y_index;

                                Point3d grid_pt = new Point3d(x, y, plane.OriginZ);

                                Plane grid_plane = new Plane(grid_pt, Plane.WorldXY.ZAxis);

                                Box voxel = new Box(grid_plane, unit_interval, unit_interval, unit_interval);

                                V.Add(voxel);
                                P.Add(grid_pt);

                            }


                        }

                    }

                }
            }

            return new Voxels(V, P, max_x, min_x, max_y, min_y, max_z, min_z);

        }

    }
}
