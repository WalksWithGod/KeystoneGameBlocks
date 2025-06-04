// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace Open.Diagramming
{
    public delegate void ElementEventHandler(object sender, ElementEventArgs e);
    public delegate void ElementRenderEventHandler(object sender, ElementRenderEventArgs e);
    public delegate void LayoutChangedEventHandler(object sender, LayoutChangedEventArgs e);
    public delegate void SegmentsEventHandler(object sender, SegmentsEventArgs e);
    public delegate void DrawShapeEventHandler(object sender, DrawShapeEventArgs e);
    public delegate void ExpandedChangedEventHandler(object sender, bool expanded);
    public delegate void TableItemsEventHandler(object sender, TableItemsEventArgs e);
}
