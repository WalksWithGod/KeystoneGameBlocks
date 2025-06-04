// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Text;

namespace Open.Diagramming.Forms.Rendering
{
    public class TableItemRender : IFormsRenderer
    {
        public virtual void RenderElement(IRenderable element, Graphics graphics, Render render)
        {
        }

        //Implement a base rendering of an element
        public virtual void RenderShadow(IRenderable element, Graphics graphics, Render render)
        {
        }

        //Implement a base rendering of an element selection
        public virtual void RenderSelection(IRenderable element, Graphics graphics, ControlRender render)
        {
        }

        //Implement a base rendering of an element selection
        public virtual void RenderAction(IRenderable element, Graphics graphics, ControlRender render)
        {
        }

        //Implement a base rendering of an element selection
        public virtual void RenderHighlight(IRenderable element, Graphics graphics, ControlRender render)
        {
        }
    }
}
