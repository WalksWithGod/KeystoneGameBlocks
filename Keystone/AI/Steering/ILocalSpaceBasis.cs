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
    /// <summary>
    /// transformation as three orthonormal unit basis vectors and the
    /// origin of the local space.  These correspond to the "rows" of
    /// a 3x4 transformation matrix with [0 0 0 1] as the final column
    /// </summary>
    public interface ILocalSpaceBasis
    {
        /// <summary>
        /// side-pointing unit basis vector
        /// </summary>
        Vector3d Side { get; }

        /// <summary>
        /// upward-pointing unit basis vector
        /// </summary>
        Vector3d Up { get; }

        /// <summary>
        /// forward-pointing unit basis vector
        /// </summary>
        Vector3d Forward { get; }

        /// <summary>
        /// origin of local space
        /// </summary>
        Vector3d Position { get; }
    }
}
