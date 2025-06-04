// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Drawing;
using System.Collections;

namespace Open.Diagramming.Layouts
{
	internal struct TerrainPoint
	{
        public int X;
        public int Y;
        public int MovementCost2;

		public TerrainPoint(int x, int y)
		{
			X = x;
            Y = y;
            MovementCost2 = -1;
		}
		
        public bool IsEmpty
        {
            get
            {
                return (MovementCost2 == 0 && X == 0 && Y == 0);
            }
        }

	}
}
