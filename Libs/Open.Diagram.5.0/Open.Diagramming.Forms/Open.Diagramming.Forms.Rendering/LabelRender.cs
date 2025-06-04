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
    public class LabelRender: IFormsRenderer
    {
        public virtual void RenderElement(IRenderable element, Graphics graphics, Render render)
        {
            Label label = element as Label;

            //Translate by offset
            if (label.Visible)
            {
                graphics.TranslateTransform(label.Offset.X, label.Offset.Y);

                SolidBrush brush = new SolidBrush(render.AdjustColor(label.Color, 0, label.Opacity));
                graphics.DrawString(label.Text, label.Font, brush, 0, 0);

                graphics.TranslateTransform(-label.Offset.X, -label.Offset.Y);
            }
        }

        public virtual void RenderElement(IRenderable element, Graphics graphics, Render render, RectangleF layout)
        {
            Label label = element as Label;

            if (label.Visible)
            {
                SolidBrush brush = new SolidBrush(render.AdjustColor(label.Color, 0, label.Opacity));
                graphics.DrawString(label.Text, label.Font, brush, layout, label.GetStringFormat());
            }

            //Render the layout rectangle for testing
            //Pen pen = new Pen(Color.Red);
            //graphics.DrawRectangle(pen,layout.X,layout.Y,layout.Width,layout.Height);  
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
            RenderElement(element, graphics, render);
        }

        public virtual void RenderAction(IRenderable element, Graphics graphics, Render render, RectangleF layout)
        {
            RenderElement(element, graphics, render, layout);
        }

        //Implement a base rendering of an element selection
        public virtual void RenderHighlight(IRenderable element, Graphics graphics, ControlRender render)
        {
            
        }
       
    }
}
