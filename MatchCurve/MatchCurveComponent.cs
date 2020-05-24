using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MatchCurve.Properties;

namespace MatchCurve
{
    public class MatchCurveComponent : GH_Component
    {
        
        public MatchCurveComponent()
            : base("Match Curve", "MC",
                "Matches a curve to another",
                "Curve", "Util")
        {
        }

     
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Reference Curve", "Ref", "Curve to match at. It does not change the location or direction.", GH_ParamAccess.item);
            pManager.AddCurveParameter("Curve", "C", "Curve to modify in order to match to reference curve.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Continuity", "con", "Continuity of transition 0=None,1=Pos,2=Tan,3=Cur.",GH_ParamAccess.item,3);
            pManager.AddTextParameter("___________", "__", "___________________", GH_ParamAccess.item, "");
            pManager.AddNumberParameter("Parameter", "t", "Parameter of reference to determine the point of matching.", GH_ParamAccess.item,1.0);
            pManager.AddBooleanParameter("isNormalized", "isN", "Is the parameter input normalized?", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("reverse Reference", "reR", "Reverses matching direction of the reference curve.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("reverse Curve ", "reC", "Reverses matching direction of the matching curve.", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Factor Tangency", "faT", "Second controlpoint location based on a factor, if continuity > 1.", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Factor Curvature", "faC", "Third controlpoint location based on a factor, if continuity > 2.", GH_ParamAccess.item, 1.0);
            pManager.AddTextParameter("___________", "__", "___________________", GH_ParamAccess.item, "");
            pManager.AddBooleanParameter("trim Reference", "trR", "Integrated trimming of reference (optional).", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("join ", "jo", "Join both curves into one  (optional)", GH_ParamAccess.item,false);
        }

     
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Matched Curve", GH_ParamAccess.item);
            pManager.AddCurveParameter("Reference", "R", "Reference Curve (trimmed if enabled)", GH_ParamAccess.item);
            pManager.AddCurveParameter("Joined", "J", "joined result (if enabled)", GH_ParamAccess.list);
        }
      
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Init all variables
            Curve reference = null;
            Curve curve = null;

            int continuity = 3;          
            
            double t = 1.0;
            double facTan = 1.0;
            double facCur = 1.0;

            bool isNormalised = true;
            bool reverseReference = false;
            bool reverseMatchingCurve = false;
            bool trim = false;
            bool join = false;

            // Try to fill them
            if (!DA.GetData(0,ref reference)) throw new Exception("Input <Reference> failed to collect data");
            if (!DA.GetData(1,ref curve)) throw new Exception("Input <Curve> failed to collect data");
            if (!DA.GetData(2,ref continuity)) throw new Exception("Input <Continuity> failed to collect data");
            
            if (!DA.GetData(4,ref t)) throw new Exception("Input <Parameter> failed to collect data");
            if (!DA.GetData(5,ref isNormalised)) throw new Exception("Input <IsNormalised> failed to collect data");
            if (!DA.GetData(6,ref reverseReference)) throw new Exception("Input <Reverse Reference> failed to collect data");
            if (!DA.GetData(7,ref reverseMatchingCurve)) throw new Exception("Input <Reverse Curve> failed to collect data");
            if (!DA.GetData(8,ref facTan)) throw new Exception("Input <Factor Tan> failed to collect data");
            if (!DA.GetData(9,ref facCur)) throw new Exception("Input <Factor Cur> failed to collect data");

            if (!DA.GetData(11,ref trim)) throw new Exception("Input <Trim> failed to collect data");
            if (!DA.GetData(12,ref join)) throw new Exception("Input <Join> failed to collect data");
            
            // Init the object doing the matching          
            MatchCurve matcher = new MatchCurve(reference, curve);           
            matcher.Continuity = (Enums.CONTINUITY) continuity;
            matcher.SetMatchingLocation(t, isNormalised);
            matcher.FactorTangency = facTan;
            matcher.FactorCurvature = facCur;
            matcher.ReverseReference = reverseReference;
            matcher.ReverseMatchingCurve = reverseMatchingCurve;
                       
            matcher.Match();

            // Set the output
            DA.SetData(0, matcher.MatchingCurve);
            DA.SetData(1, matcher.Reference);

            if (trim && !join)
              DA.SetData(2, matcher.Trim());
                     
            if (join)
              DA.SetDataList(2, matcher.TrimAndJoin());
        }

     
        protected override System.Drawing.Bitmap Icon
        {
            get
            {                
                return Resources.MatchCurveIcon24x24;
            }
        }

      
        public override Guid ComponentGuid
        {
            get { return new Guid("{2d0a0119-1fc9-45c0-a27a-f58a53ecb3dc}"); }
        }
    }
}
