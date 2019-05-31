using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Voxelizer
{
    public class VoxelizerInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Voxelizer";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("f4accd82-1139-4a78-a376-1cd7a3f301dd");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
