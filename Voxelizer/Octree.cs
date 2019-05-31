using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace Voxelizer
{

    public class Octree
    {
        public OctreeNode root;

        public Octree(double x_min, double x_max, double y_min, double y_max, double z_min, double z_max, int capacity)
        {
            this.root = new OctreeNode(x_min, x_max, y_min, y_max, z_min, z_max, capacity);
        }

        public bool Add(Point3d point)
        {
            return this.root.AddLeaf(point);
        }

        public int getDepth()
        {
            return this.root.getDepth();
        }
        
        public List<OctreeNode> getChildren()
        {
            return this.root.getChildren();
        }
        
    }

    public class OctreeNode
    {
        private OctreeBounds _bounds;
        private OctreeNode[] _children;
        /*
         * Cube:
         *   4 5
         * 2 3 6
         * 0 1
         * Not visible 7
         */
        private List<Point3d> _leafs;
        private int _capacity;
        private bool _innerNode; // True if this is not a leaf node

        public OctreeNode(double x_min, double x_max, double y_min, double y_max, double z_min, double z_max, int capacity)
        {
            this._bounds = new OctreeBounds(x_min, x_max, y_min, y_max, z_min, z_max);

            this._leafs  = new List<Point3d>();

            this._innerNode = false;

            this._capacity  = capacity;
        }

        public List<OctreeNode> getChildren()
        {

            if (this._children == null)
            {

                if (this._leafs.Count == 0)
                {

                    return new List<OctreeNode>();

                }
                else
                {

                    return new List<OctreeNode>() { this };

                }
            }

            var children = new List<OctreeNode>();

            foreach (OctreeNode child in this._children)
            {

                var grand_children = child.getChildren();

                foreach (OctreeNode grand_child in grand_children)
                {

                    children.Add(grand_child);

                }
                

            }

            return children;

        }

        private bool _divide()
        {
            double _size_x = (this._bounds.getXMax() - this._bounds.getXMin()) / 2;
            double _size_y = (this._bounds.getYMax() - this._bounds.getYMin()) / 2;
            double _size_z = (this._bounds.getZMax() - this._bounds.getZMin()) / 2;

            this._children = new OctreeNode[8];

            this._children[0] = new OctreeNode(
            this._bounds.getXMin(), this._bounds.getXMax() - _size_x,
            this._bounds.getYMin(), this._bounds.getYMax() - _size_y,
            this._bounds.getZMin(), this._bounds.getZMax() - _size_z,
            this._capacity);

            this._children[1] = new OctreeNode(
            this._bounds.getXMin() + _size_x, this._bounds.getXMax(),
            this._bounds.getYMin(), this._bounds.getYMax() - _size_y,
            this._bounds.getZMin(), this._bounds.getZMax() - _size_z,
            this._capacity);

            this._children[2] = new OctreeNode(
            this._bounds.getXMin(), this._bounds.getXMax() - _size_x,
            this._bounds.getYMin() + _size_y, this._bounds.getYMax(),
            this._bounds.getZMin(), this._bounds.getZMax() - _size_z,
            this._capacity);

            this._children[3] = new OctreeNode(
            this._bounds.getXMin() + _size_x, this._bounds.getXMax(),
            this._bounds.getYMin() + _size_y, this._bounds.getYMax(),
            this._bounds.getZMin(), this._bounds.getZMax() - _size_z,
            this._capacity);

            this._children[4] = new OctreeNode(
            this._bounds.getXMin(), this._bounds.getXMax() - _size_x,
            this._bounds.getYMin() + _size_y, this._bounds.getYMax(),
            this._bounds.getZMin() + _size_z, this._bounds.getZMax(),
            this._capacity);

            this._children[5] = new OctreeNode(
            this._bounds.getXMin() + _size_x, this._bounds.getXMax(),
            this._bounds.getYMin() + _size_y, this._bounds.getYMax(),
            this._bounds.getZMin() + _size_z, this._bounds.getZMax(),
            this._capacity);

            this._children[6] = new OctreeNode(
            this._bounds.getXMin() + _size_x, this._bounds.getXMax(),
            this._bounds.getYMin(), this._bounds.getYMax() - _size_y,
            this._bounds.getZMin() + _size_z, this._bounds.getZMax(),
            this._capacity);

            this._children[7] = new OctreeNode(
            this._bounds.getXMin(), this._bounds.getXMax() - _size_x,
            this._bounds.getYMin(), this._bounds.getYMax() - _size_y,
            this._bounds.getZMin() + _size_z, this._bounds.getZMax(),
            this._capacity);

            // Distribute points to children
            for (int i = 0; i < this._leafs.Count; i++)
            {
                for (int j = 0; j < 8; j++)
                {   

                    if (this._children[j].AddLeaf(this._leafs[i]))
                    {
                        break;
                    }
                }
            }

            this._innerNode = true;
            this._leafs = null;

            return true;
        }

        public bool AddLeaf(Point3d pt)
        {

            // outside of this node
            if (!this._bounds.InBound(pt.X, pt.Y, pt.Z))
            {

                return false;
            }

            // too many points
            if (!this._innerNode && this._leafs.Count + 1 > this._capacity)
            {

                this._divide();
            }

            // can accomodate the new point
            if (this._innerNode)
            {

                // go deeper
                for (int i = 0; i < 8; i++)
                {
                    if (this._children[i].AddLeaf(pt))
                    {
                        break;
                    }
                }
            }
            else
            {

                // Add the new point to this node
                this._leafs.Add(pt);
            }

            return true;
        }
        
        public int getDepth()
        {
            if (this._innerNode)
            {
                int max = 0;

                for (int i = 0; i < 8; i++)
                {
                    int depth = this._children[i].getDepth() + 1;
                    if (depth > max)
                    {
                        max = depth;
                    }
                }

                return max;
            }
            else
            {
                return 1;
            }
        }
      
        public Point3d getCenter()
        {

            return new Point3d(this._bounds.getXCenter(), this._bounds.getYCenter(), this._bounds.getZCenter());
            
        }

        public double getWidth()
        {

            return (this._bounds.getXMax() - this._bounds.getXMin()) * 0.5;

        }
    }

    public class OctreeBounds
    {

        private double _x_min, _x_max;
        private double _y_min, _y_max;
        private double _z_min, _z_max;

        public OctreeBounds(double x_min, double x_max, double y_min, double y_max, double z_min, double z_max)
        {
            this._x_min = x_min;
            this._x_max = x_max;
            this._y_min = y_min;
            this._y_max = y_max;
            this._z_min = z_min;
            this._z_max = z_max;
        }

        public bool InBound(double x, double y, double z)
        {
            if (this._x_min < x
                && this._x_max > x
                && this._y_min < y
                && this._y_max > y
                && this._z_min < z
                && this._z_max > z)
            {
                return true;
            }

            return false;
        }

        public double getDistanceToCenter(Point3d oL)
        {
            double sum = 0.0d;

            sum += Math.Pow(oL.X - this.getXCenter(), 2);
            sum += Math.Pow(oL.Y - this.getYCenter(), 2);
            sum += Math.Pow(oL.Z - this.getZCenter(), 2);

            return Math.Sqrt(sum);
        }

        public double getXCenter()
        {
            return this._x_min + (this._x_max - this._x_min) / 2;
        }

        public double getYCenter()
        {
            return this._y_min + (this._y_max - this._y_min) / 2;
        }

        public double getZCenter()
        {
            return this._z_min + (this._z_max - this._z_min) / 2;
        }

        public double getXMin()
        {
            return this._x_min;
        }

        public double getYMin()
        {
            return this._y_min;
        }

        public double getZMin()
        {
            return this._z_min;
        }

        public double getXMax()
        {
            return this._x_max;
        }

        public double getYMax()
        {
            return this._y_max;
        }

        public double getZMax()
        {
            return this._z_max;
        }
    }
  

}
