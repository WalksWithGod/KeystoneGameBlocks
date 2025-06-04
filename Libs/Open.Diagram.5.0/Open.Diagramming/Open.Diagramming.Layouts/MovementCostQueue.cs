// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.Text;

using Open.Diagramming.Collections;

namespace Open.Diagramming.Layouts
{
    internal class MovementCostQueue: PriorityQueue<RouteNode>
    {
        public int TotalCost
        {
            get
            {
                return Peek().TotalCost;
            }
        }

        //Needs to be in reverse order
        protected override int Compare(int i, int j)
        {
            return InnerList[j].MovementCost.CompareTo(InnerList[i].MovementCost);
        }
    }
}
