using MatchCurve.Enums;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace MatchCurve
{
    public class CurveMatcher
    {
        private double _t = 1.0;

        /// <summary>
        /// Returns the tangent at which the curve is matched against. Please use SetMatchingLocation, to set this value
        /// </summary>
        public double T => _t;
        
        /// <summary>
        /// The factor, which determines the relative location of p1 at the tangent
        /// </summary>
        public double FactorTangency { get; set; } = 1.0;

        /// <summary>
        /// The factor, which determines the relative location of p2 at the tangent, offset by the curvature vector
        /// </summary>
        public double FactorCurvature { get; set; } = 1.0;
       
        /// <summary>
        /// Flag to determine the direction of matching at the reference
        /// </summary>
        public bool ReverseReference { get; set; } = false;

        /// <summary>
        /// Flag to determine the direction of matching at the curve to match
        /// </summary>
        public bool ReverseMatchingCurve { get; set; } = false;

        /// <summary>
        /// The curve to match against. Does not change it position. It can get trimmed and joined, if desired
        /// </summary>
        public NurbsCurve Reference { get; set; } = null;

        /// <summary>
        /// The curve which is getting match onto the reference curve
        /// </summary>
        public NurbsCurve MatchingCurve { get; set; } = null;
        
        /// <summary>
        /// The continuity of which both curves need to match
        /// </summary>
        public CONTINUITY Continuity = CONTINUITY.Curvature;

        /// <summary>
        /// Creates a new instance to match a curve to another referencing curve
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="matchingCurve"></param>
        public CurveMatcher(Curve reference, Curve matchingCurve)
        {
            if (reference == null) throw new ArgumentNullException("Reference curve is null.");
            if (matchingCurve == null) throw new ArgumentNullException("Matching curve is null.");
            // Could be supported if its converted back to non periodic:
            if (matchingCurve.IsPeriodic) throw new Exception("Matching Curve must not be periodic!"); 
           
            try
            {
                Reference = reference.ToNurbsCurve();
                MatchingCurve = matchingCurve.ToNurbsCurve();
            }
            catch (Exception)
            {
                throw new InvalidCastException("Could not cast Curves to NurbsCurves, Check input");
            }
        }

        /// <summary>
        /// Sets the parameter to match at
        /// </summary>
        /// <param name="t"></param>
        /// <param name="isNormalised"></param>
        public void SetMatchingLocation(double t, bool isNormalised)
        {
            if (isNormalised)
                _t = Reference.Domain.ParameterAt(t);
            else
                _t = t;
        }        
       
        /// <summary>
        /// Matches the curve to the reference curve
        /// </summary>
        public void Match()
        {
            if (this.Continuity == CONTINUITY.None)
                return; 

            if (this.Continuity == CONTINUITY.Flow || this.Continuity == CONTINUITY.Flow2)
                throw new NotSupportedException("Flow is not supported yet");
                       
            // Make sure Curve has sufficent cps
            MatchingCurve.IncreaseDegree((int)this.Continuity);

            MatchingCurve.Domain = new Interval(0, 1);
            if (this.ReverseMatchingCurve) MatchingCurve.Reverse();

            // POSITIONAL MATCHING
            ControlPoint p0 = MatchingCurve.Points[0];
            Point3d pointAtParameter = Reference.PointAt(_t);
            p0.Location = pointAtParameter;
            SetPointOfMatchingCurve(0, p0);

            if (this.Continuity == CONTINUITY.Position) return;

            // TANGENCY MATCHING
            int c = (int)this.Continuity + 1;
            double lf = MatchingCurve.GetLength() / c;

            ControlPoint p1 = MatchingCurve.Points[1];
            Vector3d tangentAtParameter = Reference.TangentAt(_t);
            if (this.ReverseReference) tangentAtParameter *= -1;
            tangentAtParameter.Unitize();
            p1.Location = pointAtParameter + (tangentAtParameter * this.FactorTangency * lf);

            SetPointOfMatchingCurve(1, p1);

            if (this.Continuity == CONTINUITY.Tangency) return;

            // CURVATURE MATCHING
            // move p2 tangent to p1, the location of p2 in this direction does not affect the curvature continuity
            ControlPoint p2 = MatchingCurve.Points[2];
            double curvatureA = Reference.CurvatureAt(_t).Length;            
            p2.Location = p1.Location + (tangentAtParameter * this.FactorCurvature);
            
            // Update the curve
            SetPointOfMatchingCurve(2, p2);

            // Abort if the curvature is zero and keep it tangent instead
            if (Math.Abs(curvatureA) < Rhino.RhinoMath.ZeroTolerance) return;

            // Move orthogonal in curvature direction and measure the curvature
            double tM = MatchingCurve.Domain.Min;
            Vector3d normalAtParameter = Reference.CurvatureAt(_t);
            normalAtParameter.Unitize();
            ControlPoint p2Copy = p2; // Copy the cp to remove from it later
            double curvatureB0 = MatchingCurve.CurvatureAt(tM).Length; // measure curvature
            p2.Location = p2.Location + normalAtParameter; // move by the normalized curvature vector
            SetPointOfMatchingCurve(2, p2); // update the curve

            double curvatureB1 = MatchingCurve.CurvatureAt(tM).Length; // measure curvature again                      
            double deltaCurvature = curvatureB1 - curvatureB0; // compute the delta of both measurements
            
            // compute the required length of the curvature vector to move p2 so that both curvatures match
            double factorN  = 0.0; 
            if (Math.Abs(deltaCurvature) >= Rhino.RhinoMath.ZeroTolerance )  
             factorN = curvatureA / deltaCurvature;

            p2.Location = p2Copy.Location + (normalAtParameter * factorN);
            SetPointOfMatchingCurve(2, p2); // final update

            // Todo: Continue for Flow and Flow2
            // Idea: Also retreive the curvature at t + 0.0001 from the reference. 
            //       Remap the length of both curvature vectors to an arbitry length
            //       Move points there, create a vector, which equals the curvature graphs tangent at t 
            //       Do the same for the matching curve. 
            //       Move the fourth point (p3), first in reference tangent direction and then in normal direction
            //       until both tangents are equal. Should be linear?! So do the same ratio-calculation as in curvature
            //       matching operation.
            //if (this.Continuity == CONTINUITY.Curvature) return;
        }

       
        /// <summary>
        /// Sets the controlpoint of the curve at a given index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="point"></param>
        private void SetPointOfMatchingCurve(int index, ControlPoint point)
        {
            MatchingCurve.Points.SetPoint(index, point.Location.X, point.Location.Y, point.Location.Z, point.Weight);
        }

        /// <summary>
        /// After the surface has been matched, this call can be made to trim at the matched parameter
        /// </summary>
        /// <returns></returns>
        public Curve Trim()
        {
            return Reference.Trim( this.ReverseReference ? new Interval(_t,Reference.Domain.Max) : new Interval(Reference.Domain.Min, _t));                    
        }

        /// <summary>
        /// After the surface has been matched, this call can be made to trim and join at the matched parameter
        /// </summary>
        /// <returns></returns>
        public Curve[] TrimAndJoin()
        {
            Curve trim = Trim();
            return Curve.JoinCurves(new Curve[] { trim,MatchingCurve }); 
        }
    }
}
