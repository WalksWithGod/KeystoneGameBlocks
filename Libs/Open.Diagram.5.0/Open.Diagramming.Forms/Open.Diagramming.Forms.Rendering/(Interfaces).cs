// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Open.Diagramming;

namespace Open.Diagramming.Forms.Rendering
{
    //Interface for any class that requires rendering to implement
    public interface IFormsRenderer
    {
        void RenderAction(IRenderable element, Graphics graphics, ControlRender render);
        void RenderElement(IRenderable element, Graphics graphics, Render render);
        void RenderHighlight(IRenderable element, Graphics graphics,ControlRender render);
        void RenderSelection(IRenderable element, Graphics graphics, ControlRender render);
        void RenderShadow(IRenderable element, Graphics graphics, Render render);
    }
}

