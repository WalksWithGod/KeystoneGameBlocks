// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using Open.Diagramming.Collections;

namespace Open.Diagramming
{
    public class Lines: Elements<Line>
    {
        public Lines(Model model): base(model)
        {

        }

        protected internal override void ElementInsert(Line line)
        {
            base.ElementInsert(line);

            //If a connector and is not auto routed and is not being deserialized then calculate points
            if (line is Connector)
            {
                Connector connector = line as Connector;
                if (connector.Points == null) connector.CalculateRoute();
            }

            line.DrawPath();
        }

    }
}
