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
    public class ComplexShapeRender: ShapeRender
    {
        public override void RenderElement(IRenderable element, Graphics graphics, Render render)
        {
            ComplexShape complex = element as ComplexShape;

            //Render this shape
            base.RenderElement(element, graphics, render);

            Region current = null;

            //Set up clipping if required
            if (complex.Clip)
            {
                Region region = new Region(complex.GetPath());
                current = graphics.Clip;
                graphics.SetClip(region, CombineMode.Intersect);
            }

            //Render the children
            if (complex.Children != null)
            {
                foreach (Solid solid in complex.RenderList)
                {
                    graphics.TranslateTransform(solid.Bounds.X, solid.Bounds.Y);

                    IFormsRenderer renderer = render.GetRenderer(solid.GetType());
                    renderer.RenderElement(solid, graphics, render);

                    graphics.TranslateTransform(-solid.Bounds.X, -solid.Bounds.Y);
                }
            }

            if (complex.Clip) graphics.Clip = current;
        }

        public override void RenderShadow(IRenderable element, Graphics graphics, Render render)
        {
            ComplexShape complex = element as ComplexShape;
            base.RenderShadow(element, graphics, render);

            Region current = null;

            //Set up clipping if required
            if (complex.Clip)
            {
                Region region = new Region(complex.GetPath());
                current = graphics.Clip;
                graphics.SetClip(region, CombineMode.Intersect);
            }

            //Render the children
            if (complex.Children != null)
            {
                foreach (Solid solid in complex.RenderList)
                {
                    graphics.TranslateTransform(solid.Bounds.X, solid.Bounds.Y);

                    IFormsRenderer renderer = render.GetRenderer(solid.GetType());
                    renderer.RenderShadow(solid, graphics, render);

                    graphics.TranslateTransform(-solid.Bounds.X, -solid.Bounds.Y);
                }
            }

            if (complex.Clip) graphics.Clip = current;
        }

        public override void RenderAction(IRenderable element, Graphics graphics, ControlRender render)
        {
            ComplexShape complex = element as ComplexShape;
            base.RenderAction(element, graphics, render);

            Region current = null;

            //Set up clipping if required
            if (complex.Clip)
            {
                Region region = new Region(complex.GetPath());
                current = graphics.Clip;
                graphics.SetClip(region, CombineMode.Intersect);
            }

            //Render the children
            if (complex.Children != null)
            {
                foreach (Solid solid in complex.RenderList)
                {
                    graphics.TranslateTransform(solid.Bounds.X, solid.Bounds.Y);

                    IFormsRenderer renderer = render.GetRenderer(solid.GetType());
                    renderer.RenderAction(solid, graphics, render);
                    graphics.TranslateTransform(-solid.Bounds.X, -solid.Bounds.Y);
                }
            }

            if (complex.Clip) graphics.Clip = current;
        }

    }
}
