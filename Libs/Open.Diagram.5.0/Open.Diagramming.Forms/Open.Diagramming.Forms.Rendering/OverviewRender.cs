// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Open.Diagramming;

namespace Open.Diagramming.Forms.Rendering
{
	public class OverviewRender: Render
	{
        public OverviewRender(): base()
        {
            Summary = true;
        }

        public override void RenderLayer(Graphics graphics, Layer layer, ElementList elementRenderList, RectangleF renderRectangle)
        {
            if (elementRenderList == null) throw new ArgumentNullException("elementRenderList");

            //Draw each element by checking if it is renderable and calling the render method
            foreach (Element element in elementRenderList)
            {
                if (element.Layer == layer && element.Visible && element.Bounds.IntersectsWith(renderRectangle))
                {
                    //Draw shapes
                    GraphicsState graphicsState = graphics.Save();
                    Matrix matrix = graphics.Transform;

                    matrix.Translate(element.Bounds.X, element.Bounds.Y);

                    //Set up rotation and other transforms
                    if (element is ITransformable)
                    {
                        ITransformable rotatable = (ITransformable)element;
                        PointF center = new PointF(element.Bounds.Width / 2, element.Bounds.Height / 2);
                        matrix.RotateAt(rotatable.Rotation, center);
                    }

                    //Apply transform, mode, and render element
                    graphics.Transform = matrix;
                    graphics.SmoothingMode = element.SmoothingMode;

                    //Get the renderer for this type and render
                    IFormsRenderer renderer = GetRenderer(element.GetType());
                    renderer.RenderElement(element, graphics, this);

                    graphics.Restore(graphicsState);
                }
            }
        }

        public virtual void RenderElement(Element element, Graphics graphics)
        {
            
        }


	}
}
