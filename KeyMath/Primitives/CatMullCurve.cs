using System;
using System.Collections.Generic;
using Keystone.Types;

namespace keymath.Primitives
{
    /// <summary>
    /// CatMullCurves have the desired property for pathing where the interpolated points between control points have a slope that 
    /// is equal to the previous and next control points.  This means that if you were plotting camera control points or actor waypoints that avoided
    /// going through walls or obstacles, the interpolated points would also be guaranteed to avoid going through those obstacles.  The same
    /// cannot be said for BezierCurves generally.
    /// 
    /// Based on Overhauser (Catmull-Rom) Splines for Camera Animation
    /// By Radu Gruian
    /// http://www.codeproject.com/KB/recipes/Overhauser.aspx?msg=2828974
    /// 
    /// Converted to c# Oct.15.2009 - Hypnotron
    /// </summary>
    public class CatMullCurve
    {
        List<Vector3d> _points;
        float _step;  //  1.0 / _points.Count yields the time step between control points

        public CatMullCurve() 
        {
            _points = new List<Vector3d>();
            _step = 0;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="curve"></param>
        public CatMullCurve(CatMullCurve curve) : this()
        {
            for (int i = 0; i < curve._points.Count; i++)
                _points.Add(curve._points[i]);

            _step = curve._step;
        }

         public int NumPoints
        {
	        get
	        {
	            if (_points == null) return 0;
	            return _points.Count;
            } 
        }

        public void AddSplinePoint(Vector3d v)
        {
            _points.Add(v);
            _step = (float)1 / _points.Count;
        }

        public  Vector3d[] Points {get { return _points.ToArray();}}

        /// <summary>
        /// Find's the nearest control point index
        /// </summary>
        /// <param name="time">Relative step between 0 and 1.0f (beginning and end of curve)</param>
        /// <returns></returns>
        public Vector3d GetInterpolatedSplinePoint(float time)
        {
            if (_points == null) throw new Exception("No control points set.");
            // Find out in which interval we are on the spline
            int p = (int)(time / _step);
            
            // Compute local control point indices
            int p0 = Clamp(p - 1); 
            int p1 = Clamp(p);
            int p2 = Clamp(p + 1); 
            int p3 = Clamp(p + 2);

            float lt = (time - _step * p) / _step;

            return Catmull(lt, _points[p0], _points[p1], _points[p2], _points[p3]);
        }

        /// <summary>
        /// Returns a user specified number of points, that are evenly distributed along the length of the spline
        /// TODO: should first point _always_ be included?  That would make numberOfPoints
        /// incluive of first and last.  And note that the last value in the returned array
        /// depending on what resolution evaluates to, may not "exactly" match the last.  I could add a flag option
        /// to forceably clamp the final value to the end point _points[NumPoints-1]
        /// </summary>
        /// <param name="numberOfPoints"></param>
        /// <returns></returns>
        public Vector3d[] GetInterpolatedSplinePoint(uint numberOfPoints)
        {
            if (numberOfPoints == 0) throw new ArgumentException("Argument must be greater than 0.");
            Vector3d[] results = new Vector3d[numberOfPoints];
            float resolution = 1/numberOfPoints;
            float t = resolution;
            for (uint i =0 ; i < numberOfPoints ; i++ )
            {
                results[i] = GetInterpolatedSplinePoint(t);
                t += resolution;
            }
            return results;
        }


        // Static method for computing the Catmull-Rom parametric equation
        // given a time (t) and a vector quadruple (p1,p2,p3,p4).
        // Solve the Catmull-Rom parametric equation for a given time(t) and vector quadruple (p1,p2,p3,p4)
        public static Vector3d Catmull(float t, Vector3d p1, Vector3d p2, Vector3d p3, Vector3d p4)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            double b1 = .5 * (-t3 + 2 * t2 - t);
            double b2 = .5 * (3 * t3 - 5 * t2 + 2);
            double b3 = .5 * (-3 * t3 + 4 * t2 + t);
            double b4 = .5 * (t3 - t2);

            return (p1 * b1 + p2 * b2 + p3 * b3 + p4 * b4);
        }

        /// <summary>
        /// Make sure the returned index is within the bounds of the _points list
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private int Clamp(int value)
        {
            if (value < 0) return 0;
            if (value >= _points.Count - 1) return _points.Count - 1;
            return value;
        }
    }
}
