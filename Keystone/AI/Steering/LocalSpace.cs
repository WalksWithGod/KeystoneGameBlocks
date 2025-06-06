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
using Steering.Helpers;

namespace Steering
{
    /// <summary>
    /// transformation as three orthonormal unit basis vectors and the
    /// origin of the local space.  These correspond to the "rows" of
    /// a 3x4 transformation matrix with [0 0 0 1] as the final column
    /// </summary>
    public class LocalSpaceBasis
        : ILocalSpaceBasis
    {
        protected Vector3d SideField;

        /// <summary>
        /// side-pointing unit basis vector
        /// </summary>
        public Vector3d Side
        {
            get { return SideField; }
            set { SideField = value; }
        }

        protected Vector3d UpField;

        /// <summary>
        /// upward-pointing unit basis vector
        /// </summary>
        public Vector3d Up
        {
            get { return UpField; }
            set { UpField = value; }
        }

        protected Vector3d ForwardField;

        /// <summary>
        /// forward-pointing unit basis vector
        /// </summary>
        public Vector3d Forward
        {
            get { return ForwardField; }
            set { ForwardField = value; }
        }

        protected Vector3d PositionField;

        /// <summary>
        /// origin of local space
        /// </summary>
        public Vector3d Position
        {
            get { return PositionField; }
            set { PositionField = value; }
        }
    }

    /// <summary>
	/// LocalSpaceMixin is a mixin layer, a class template with a paramterized base
	/// class.  Allows "LocalSpace-ness" to be layered on any class.
	/// </summary>
	public class LocalSpace : LocalSpaceBasis
    {
		public LocalSpace()
		{
			ResetLocalSpace();
		}

        public LocalSpace(Vector3d up, Vector3d forward, Vector3d position)
		{
			Up = up;
			Forward = forward;
            Position = position;
			SetUnitSideFromForwardAndUp();
		}

        public LocalSpace(Matrix transformation)
        {
            LocalSpaceBasisHelpers.FromMatrix(transformation, out ForwardField, out SideField, out UpField, out PositionField);
        }

		// ------------------------------------------------------------------------
		// reset transform: set local space to its identity state, equivalent to a
		// 4x4 homogeneous transform like this:
		//
		//     [ X 0 0 0 ]
		//     [ 0 1 0 0 ]
		//     [ 0 0 1 0 ]
		//     [ 0 0 0 1 ]
		//
		// where X is 1 for a left-handed system and -1 for a right-handed system.
		public void ResetLocalSpace()
		{
		    LocalSpaceBasisHelpers.ResetLocalSpace(out ForwardField, out SideField, out UpField, out PositionField);
		}

		// ------------------------------------------------------------------------
		// set "side" basis vector to normalized cross product of forward and up
		public void SetUnitSideFromForwardAndUp()
		{
		    LocalSpaceBasisHelpers.SetUnitSideFromForwardAndUp(ref ForwardField, out SideField, ref UpField);
		}

	    // ------------------------------------------------------------------------
		// regenerate the orthonormal basis vectors given a new forward
		//(which is expected to have unit length)
        public void RegenerateOrthonormalBasisUF(Vector3d newUnitForward)
        {
            LocalSpaceBasisHelpers.RegenerateOrthonormalBasisUF(newUnitForward, out ForwardField, out SideField, ref UpField);
        }

		// for when the new forward is NOT know to have unit length
        public void RegenerateOrthonormalBasis(Vector3d newForward)
        {
            LocalSpaceBasisHelpers.RegenerateOrthonormalBasis(newForward, out ForwardField, out SideField, ref UpField);
        }

		// for supplying both a new forward and and new up
        public void RegenerateOrthonormalBasis(Vector3d newForward, Vector3d newUp)
        {
            LocalSpaceBasisHelpers.RegenerateOrthonormalBasis(newForward, newUp, out ForwardField, out SideField, out UpField);
        }
	}
}
