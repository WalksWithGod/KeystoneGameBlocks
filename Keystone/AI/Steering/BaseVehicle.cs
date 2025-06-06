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

namespace Steering
{
	public abstract class BaseVehicle : LocalSpace, IVehicle
	{
		public abstract double Mass { get; set; }
		public abstract double Radius { get; set; }
        public abstract Vector3d Velocity { get; }
		public abstract Vector3d Acceleration { get; }
		public abstract double Speed { get; set; }

        public abstract Vector3d PredictFuturePosition(double predictionTime);

		public abstract double MaxForce { get; }
		public abstract double MaxSpeed { get; }
	}
}
