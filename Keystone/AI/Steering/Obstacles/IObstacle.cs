// Copyright (c) 2002-2003, Sony Computer Entertainment America
// Copyright (c) 2002-2003, Craig Reynolds <craig_reynolds@playstation.sony.com>
// Copyright (C) 2007 Bjoern Graf <bjoern.graf@gmx.net>
// Copyright (C) 2007 Michael Coles <michael@digini.com>
// All rights reserved.
//
// This software is licensed as described in the file license.txt, which
// you should have received as part of this distribution. The terms
// are also available at http://www.codeplex.com/SharpSteer/Project/License.aspx.

using Keystone.Types;

namespace Steering.Obstacles
{
	/// <summary>
	/// Obstacle: a pure virtual base class for an abstract shape in space, to be
	/// used with obstacle avoidance.
	/// </summary>
	public interface IObstacle
	{
        Vector3d SteerToAvoid(IVehicle v, double minTimeToCollision);

        double? NextIntersection(IVehicle vehicle);
	}
}
