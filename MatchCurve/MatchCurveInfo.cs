using System;
using System.Drawing;
using Grasshopper.Kernel;
using MatchCurve.Properties;

namespace MatchCurve
{
    public class MatchCurveInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "MatchCurve";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Resources.MatchCurveIcon24x24;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Match Curve Functionality";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("17596d23-b9ad-4c5d-b3a5-94f3fdf5a4e6");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Tom Jankowski";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "Search in Grasshopper3d Forum";
            }
        }
    }
}
