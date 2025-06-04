// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using Open.Diagramming.Collections;

namespace Open.Diagramming
{
    public class Shapes: Elements<Shape>
    {
        public Shapes(Model model): base(model)
        {
        }
    
        protected internal override void ElementInsert(Shape shape)
        {
            base.ElementInsert(shape);

            //Set container and layers for children of complex shape
            if (shape is ComplexShape)
            {
                ComplexShape complex = shape as ComplexShape;

                foreach (Element child in complex.Children.Values)
                {
                    child.SetModel(Model);
                    child.SetLayer(Model.Layers.CurrentLayer);
                }
            }

            //Set the height of the table
            if (shape is Table)
            {
                Table table = shape as Table;
                table.SetHeight();
            }
        }
    }
}
