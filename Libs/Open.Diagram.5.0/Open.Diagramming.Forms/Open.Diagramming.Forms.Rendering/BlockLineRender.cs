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
    public class BlockLineRender: LinkRender
    {
        public override void RenderElement(IRenderable element, Graphics graphics, Render render)
        {
            BlockLine line = element as BlockLine;
            if (line.Points == null) return;
            if (line.Points.Count < 2) return;

            GraphicsPath path = line.GetBlockPath();
            Pen pen = null;

            if (line.CustomPen == null)
            {
                pen = new Pen(line.BorderColor, line.BorderWidth);
                pen.DashStyle = line.BorderStyle;

                //Check if winforms renderer and adjust color as required
                pen.Color = render.AdjustColor(line.BorderColor, line.BorderWidth, line.Opacity);
            }
            else
            {
                pen = line.CustomPen;
            }

            //Can throw an out of memory exception in System.Drawing
            try
            {
                graphics.SmoothingMode = line.SmoothingMode;
                graphics.DrawPath(pen, path);
            }
            catch
            {

            }

        }
    }
}
