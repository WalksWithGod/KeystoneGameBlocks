// Copyright (c) 2002-2003, Sony Computer Entertainment America
// Copyright (c) 2002-2003, Craig Reynolds <craig_reynolds@playstation.sony.com>
// Copyright (C) 2007 Bjoern Graf <bjoern.graf@gmx.net>
// Copyright (C) 2007 Michael Coles <michael@digini.com>
// All rights reserved.
//
// This software is licensed as described in the file license.txt, which
// you should have received as part of this distribution. The terms
// are also available at http://www.codeplex.com/SharpSteer/Project/License.aspx.

using System;
using Keystone.Types;
using Steering.Helpers;

namespace Steering.Obstacles
{
	/// <summary>
	/// SphericalObstacle a simple concrete type of obstacle.
	/// </summary>
	public class SphericalObstacle : IObstacle
	{
	    public double Radius;
	    public Vector3d Center;

	    public SphericalObstacle()
	    	: this(1, Vector3d.Zero())
		{
		}

        public SphericalObstacle(double r, Vector3d c)
		{
			Radius = r;
			Center = c;
		}

        // xxx couldn't this be made more compact using localizePosition?
        /// <summary>
        /// Checks for intersection of the given spherical obstacle with a
        /// volume of "likely future vehicle positions": a cylinder along the
        /// current path, extending minTimeToCollision seconds along the
        /// forward axis from current position.
        ///
        /// If they intersect, a collision is imminent and this function returns
        /// a steering force pointing laterally away from the obstacle's center.
        ///
        /// Returns a zero vector if the obstacle is outside the cylinder
        /// </summary>
        /// <param name="v"></param>
        /// <param name="minTimeToCollision"></param>
        /// <returns></returns>
        public Vector3d SteerToAvoid(IVehicle v, double minTimeToCollision)
        {
            // Capsule x Sphere collision detection
            //http://www.altdev.co/2011/04/26/more-collision-detection-for-dummies/

            var capStart = v.Position;
            var capEnd = v.PredictFuturePosition(minTimeToCollision);

            var alongCap = capEnd - capStart;
            var capLength = alongCap.Length;

            //If the vehicle is going very slowly then simply test vehicle sphere against obstacle sphere
            if (capLength <= 0.05)
            {
                var distance = Vector3d.GetDistance3d (Center, v.Position);
                if (distance < Radius + v.Radius)
                    return (v.Position - Center);
                return Vector3d.Zero();
            }

            var capAxis = alongCap / capLength;

            //Project vector onto capsule axis
            var b = Utilities.Clamp(Vector3d.DotProduct(Center - capStart, capAxis), 0, capLength);

            //Get point on axis (closest point to sphere center)
            var r = capStart + capAxis * b;

            //Get distance from circle center to closest point
            var dist = Vector3d.GetDistance3d(Center, r);

            //Basic sphere sphere collision about the closest point
            var inCircle = dist < Radius + v.Radius;
            if (!inCircle)
            	return Vector3d.Zero();

            //avoidance vector calculation
            Vector3d avoidance = Vector3Helpers.PerpendicularComponent(v.Position - Center, v.Forward);
            //if no avoidance was calculated this is because the vehicle is moving exactly forward towards the sphere, add in some random sideways deflection
            if (avoidance == Vector3d.Zero())
                avoidance = -v.Forward + v.Side * 0.01f * RandomHelpers.Random();

            avoidance = Vector3d.Normalize(avoidance);
            avoidance *= v.MaxForce;
            avoidance += v.Forward * v.MaxForce * 0.75f;
            return avoidance;
		}

        // xxx experiment cwr 9-6-02
        public double? NextIntersection(IVehicle vehicle)
        {
            // This routine is based on the Paul Bourke's derivation in:
            //   Intersection of a Line and a Sphere (or circle)
            //   http://www.swin.edu.au/astronomy/pbourke/geometry/sphereline/

            // find "local center" (lc) of sphere in boid's coordinate space
            Vector3d lc = vehicle.LocalizePosition(Center);

            // computer line-sphere intersection parameters
            double b = -2 * lc.z;
            var totalRadius = Radius + vehicle.Radius;
            double c = (lc.x * lc.x) + (lc.y * lc.y) + (lc.z * lc.z) - (totalRadius * totalRadius);
            double d = (b * b) - (4 * c);

            // when the path does not intersect the sphere
            if (d < 0)
                return null;

            // otherwise, the path intersects the sphere in two points with
            // parametric coordinates of "p" and "q".
            // (If "d" is zero the two points are coincident, the path is tangent)
            double s = Math.Sqrt(d);
            double p = (-b + s) / 2;
            double q = (-b - s) / 2;

            // both intersections are behind us, so no potential collisions
            if ((p < 0) && (q < 0))
                return null;

            // at least one intersection is in front of us
            return
                ((p > 0) && (q > 0)) ?
                // both intersections are in front of us, find nearest one
                ((p < q) ? p : q) :
                // otherwise only one intersections is in front, select it
                ((p > 0) ? p : q);
        }
	}
}
