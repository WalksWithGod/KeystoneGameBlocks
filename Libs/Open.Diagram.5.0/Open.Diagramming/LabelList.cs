// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Text;

using Open.Diagramming.Collections;

namespace Open.Diagramming
{
    public class LabelList : List<Label>
    {
        //Creates a new empty label list
        public LabelList(): base()
        {
        }

        //Creates a label list from a shapes collection
        public LabelList(Shapes shapes): base()
        {
            foreach (Shape shape in shapes.Values)
            {
                if (shape.Label != null) Add(shape.Label);
            }
        }
    }
}
