using System;
using Keystone.Types;
using System.Collections.Generic;

namespace Steering.Pathway
{
    /// <summary>
    /// A path consisting of a series of gates which must be passed through
    /// </summary>
    public class GatewayPathway
        : IPathway
    {
        public PolylinePathway Centerline
        {
            get
            {
                return _trianglePathway.Centerline;
            }
        }

        private readonly TrianglePathway _trianglePathway;
        public TrianglePathway TrianglePathway
        {
            get
            {
                return _trianglePathway;
            }
        }

        public GatewayPathway(IEnumerable<Gateway> gateways, bool cyclic = false)
        {
            List<TrianglePathway.Triangle> triangles = new List<TrianglePathway.Triangle>();

            bool first = true;
            Gateway previous = default(Gateway);
            Vector3d previousNormalized = Vector3d.Zero();
            foreach (var gateway in gateways)
            {
                var n = Vector3d.Normalize(gateway.B - gateway.A);

                if (!first)
                {
                    if (Vector3d.DotProduct(n, previousNormalized) < 0)
                    {
                        triangles.Add(new TrianglePathway.Triangle(previous.A, previous.B, gateway.A));
                        triangles.Add(new TrianglePathway.Triangle(previous.A, gateway.A, gateway.B));
                    }
                    else
                    {
                        triangles.Add(new TrianglePathway.Triangle(previous.A, previous.B, gateway.A));
                        triangles.Add(new TrianglePathway.Triangle(previous.B, gateway.A, gateway.B));
                    }
                }
                first = false;

                previousNormalized = n;
                previous = gateway;
            }

            _trianglePathway = new TrianglePathway(triangles, cyclic);

        }

        public struct Gateway
        {
            public readonly Vector3d A;
            public readonly Vector3d B;

            public Gateway(Vector3d a, Vector3d b)
                : this()
            {
                A = a;
                B = b;
            }
        }
        
        public bool HasArrivedAtEndPath (Vector3d point)
        {
        	throw new NotImplementedException();
        }

        public Vector3d MapPointToPath(Vector3d point, out Vector3d tangent, out double outside)
        {
            return _trianglePathway.MapPointToPath(point, out tangent, out outside);
        }

        public Vector3d MapPathDistanceToPoint(double pathDistance)
        {
            return _trianglePathway.MapPathDistanceToPoint(pathDistance);
        }

        public double MapPointToPathDistance(Vector3d point)
        {
            return _trianglePathway.MapPointToPathDistance(point);
        }
    }
}
