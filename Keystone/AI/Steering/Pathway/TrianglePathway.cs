using Keystone.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using Steering.Helpers;

namespace Steering.Pathway
{
    /// <summary>
    /// A pathway made out of triangular segments
    /// </summary>
    public class TrianglePathway
        : IPathway
    {
        private readonly Triangle[] _path;

        public IEnumerable<Triangle> Triangles
        {
            get { return _path; }
        }

        private readonly PolylinePathway _centerline;
        public PolylinePathway Centerline
        {
            get
            {
                return _centerline;
            }
        }

        public TrianglePathway(IEnumerable<Triangle> path, bool cyclic = false)
        {
            _path = path.ToArray();

            //Calculate center points
            for (int i = 0; i < _path.Length; i++)
                _path[i].PointOnPath = (2 * _path[i].A + _path[i].Edge0) / 2;

            //Calculate tangents along path
            for (int i = 0; i < _path.Length; i++)
            {
                var bIndex = cyclic ? ((i + 1) % _path.Length) : Math.Min(i + 1, _path.Length - 1);

                var vectorToNextTriangle = _path[bIndex].PointOnPath - _path[i].PointOnPath;
                var l = vectorToNextTriangle.Length;

                _path[i].Tangent = vectorToNextTriangle / l;

                if (Math.Abs(l) < float.Epsilon)
                    _path[i].Tangent = Vector3d.Zero();
            }

            _centerline = new PolylinePathway(_path.Select(a => a.PointOnPath).ToArray(), 0.1f, cyclic);
        }

      	public bool HasArrivedAtEndPath (Vector3d point)
        {
        	throw new NotImplementedException();
        }

        
        public Vector3d MapPointToPath(Vector3d point, out Vector3d tangent, out double outside)
        {
            int index;
            return MapPointToPath(point, out tangent, out outside, out index);
        }

        private Vector3d MapPointToPath(Vector3d point, out Vector3d tangent, out double outside, out int segmentIndex)
        {
            double distanceSqr = double.PositiveInfinity;
            Vector3d closestPoint = Vector3d.Zero();
            bool inside = false;
            segmentIndex = -1;

            for (int i = 0; i < _path.Length; i++)
            {
                bool isInside;
                var p = ClosestPointOnTriangle(ref _path[i], point, out isInside);

                var normal = (point - p);
                var dSqr = normal.LengthSquared();

                if (dSqr < distanceSqr)
                {
                    distanceSqr = dSqr;
                    closestPoint = p;
                    inside = isInside;
                    segmentIndex = i;
                }

                if (isInside)
                    break;
            }

            if (segmentIndex == -1)
                throw new InvalidOperationException("Closest Path Segment Not Found (Zero Length Path?");

            tangent = _path[segmentIndex].Tangent;
            outside = Math.Sqrt(distanceSqr) * (inside ? -1 : 1);
            return closestPoint;
        }

        public Vector3d MapPathDistanceToPoint(double pathDistance)
        {
            return _centerline.MapPathDistanceToPoint(pathDistance);

            //// clip or wrap given path distance according to cyclic flag
            //if (_cyclic)
            //    pathDistance = pathDistance % _totalPathLength;
            //else
            //{
            //    if (pathDistance < 0)
            //        return _path[0].PointOnPath;
            //    if (pathDistance >= _totalPathLength)
            //        return _path[_path.Length - 1].PointOnPath;
            //}

            //// step through segments, subtracting off segment lengths until
            //// locating the segment that contains the original pathDistance.
            //// Interpolate along that segment to find 3d point value to return.
            //for (int i = 1; i < _path.Length; i++)
            //{
            //    if (_path[i].Length < pathDistance)
            //    {
            //        pathDistance -= _path[i].Length;
            //    }
            //    else
            //    {
            //        float ratio = pathDistance / _path[i].Length;

            //        var l = Vector3.Lerp(_path[i].PointOnPath, _path[i].PointOnPath + _path[i].Tangent * _path[i].Length, ratio);
            //        return l;
            //    }
            //}

            //return Vector3.Zero;
        }

        public double MapPointToPathDistance(Vector3d point)
        {
            return _centerline.MapPointToPathDistance(point);
        }

        public struct Triangle
        {
            public readonly Vector3d A;
            public readonly Vector3d Edge0;
            public readonly Vector3d Edge1;

            internal Vector3d Tangent;
            internal Vector3d PointOnPath;

            internal readonly double Determinant;

            public Triangle(Vector3d a, Vector3d b, Vector3d c)
            {
                A = a;
                Edge0 = b - a;
                Edge1 = c - a;

                PointOnPath = Vector3d.Zero();
                Tangent = Vector3d.Zero();

                // ReSharper disable once ImpureMethodCallOnReadonlyValueField
                var edge0LengthSquared = Edge0.LengthSquared();

                var edge0DotEdge1 = Vector3d.DotProduct(Edge0, Edge1);
                var edge1LengthSquared = Vector3d.DotProduct(Edge1, Edge1);

                Determinant = edge0LengthSquared * edge1LengthSquared - edge0DotEdge1 * edge0DotEdge1;
            }
        }

        private static Vector3d ClosestPointOnTriangle(ref Triangle triangle, Vector3d sourcePosition, out bool inside)
        {
            double a, b;
            return ClosestPointOnTriangle(ref triangle, sourcePosition, out a, out b, out inside);
        }

        internal static Vector3d ClosestPointOnTriangle(ref Triangle triangle, Vector3d sourcePosition, out double edge0Distance, out double edge1Distance, out bool inside)
        {
            Vector3d v0 = triangle.A - sourcePosition;

            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            double a = triangle.Edge0.LengthSquared();
            double b = Vector3d.DotProduct(triangle.Edge0, triangle.Edge1);
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            double c = triangle.Edge1.LengthSquared();
            double d = Vector3d.DotProduct(triangle.Edge0, v0);
            double e = Vector3d.DotProduct(triangle.Edge1, v0);

            double det = triangle.Determinant;
            double s = b * e - c * d;
            double t = b * d - a * e;

            inside = false;
            if (s + t < det)
            {
                if (s < 0)
                {
                    if (t < 0)
                    {
                        if (d < 0)
                        {
                            s = Utilities.Clamp(-d / a, 0, 1);
                            t = 0;
                        }
                        else
                        {
                            s = 0;
                            t = Utilities.Clamp(-e / c, 0, 1);
                        }
                    }
                    else
                    {
                        s = 0;
                        t = Utilities.Clamp(-e / c, 0, 1);
                    }
                }
                else if (t < 0)
                {
                    s = Utilities.Clamp(-d / a, 0, 1);
                    t = 0;
                }
                else
                {
                    double invDet = 1 / det;
                    s *= invDet;
                    t *= invDet;
                    inside = true;
                }
            }
            else
            {
                if (s < 0)
                {
                    double tmp0 = b + d;
                    double tmp1 = c + e;
                    if (tmp1 > tmp0)
                    {
                        double numer = tmp1 - tmp0;
                        double denom = a - 2 * b + c;
                        s = Utilities.Clamp(numer / denom, 0, 1);
                        t = 1 - s;
                    }
                    else
                    {
                        t = Utilities.Clamp(-e / c, 0, 1);
                        s = 0;
                    }
                }
                else if (t < 0)
                {
                    if (a + d > b + e)
                    {
                        double numer = c + e - b - d;
                        double denom = a - 2 * b + c;
                        s = Utilities.Clamp(numer / denom, 0, 1);
                        t = 1 - s;
                    }
                    else
                    {
                        s = Utilities.Clamp(-e / c, 0, 1);
                        t = 0;
                    }
                }
                else
                {
                    double numer = c + e - b - d;
                    double denom = a - 2 * b + c;
                    s = Utilities.Clamp(numer / denom, 0, 1);
                    t = 1 - s;
                }
            }

            edge0Distance = s;
            edge1Distance = t;
            return triangle.A + s * triangle.Edge0 + t * triangle.Edge1;
        }
    }
}
