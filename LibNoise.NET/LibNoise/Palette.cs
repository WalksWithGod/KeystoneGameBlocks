using System;
using System.Collections.Generic;

namespace LibNoise
{
    /// <summary>
    /// 
    /// </summary>
    public class Palette
    {
        protected struct ColorPt
        {
            public double pos;
            public byte r;
            public byte g;
            public byte b;
            public byte a;
            public ColorPt(double pos, byte r, byte g, byte b, byte a)
            {
                this.pos = pos; // position - from 0.0 to 1.0
                this.r = r; // color components - from 0.0 to 255.0
                this.b = b;
                this.g = g;
                this.a = a;
            }
        }

        private class ColorPtSorter : System.Collections.Generic.IComparer <ColorPt>
        {
            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            public int Compare(ColorPt x, ColorPt y)
            {
                return (int)((x.pos * 10000000) - (y.pos * 10000000));
            }
        }

        System.Collections.Generic.IComparer<ColorPt> compareFunc = new ColorPtSorter();
        private List<ColorPt> colors = new List<ColorPt> ();

        /// <summary>
        /// A 1d bitmap where each pixel along x is used as a gradient value.  If the bitmap is a 2d bitmap, only the first row will be used
        /// </summary>
        /// <param name="bitmap"></param>
        public Palette(System.Drawing.Bitmap bitmap)
        {
            int width = bitmap.Width ;
            for (int i = 0; i < width; i++) // the width is how many different colors there are
            {
                System.Drawing.Color color = bitmap.GetPixel(i, 2);
                //lookupTable[i] = Core._Core.Globals.RGBA256(color.R, color.G, color.B, color.A);
                float position = i / (width - 1f);
                AddColor( position  , color.R, color.G, color.B);
            }
        }

        public Palette(System.Drawing.Color start, System.Drawing.Color end)
        {
            AddColor(0, start.R, start.G, start.B, start.A);
            AddColor(1, end.R, end.G, end.B, end.A);
        }

        public Palette()
        {
            AddColor(0, 0, 0, 70);
            AddColor(.6, 100, 128, 200);
            AddColor(.65, 168, 80, 0);
            AddColor(.7, 50, 200, 50);
            AddColor(.9, 128, 128, 128);
            AddColor(1, 255, 255, 255);
        }

        public void Clear()
        {
            colors.Clear();
        }

        public void AddColor(double pos, byte r, byte g, byte b)
        {
            // a = 255 = completely opaque by default
            byte a = 255;
            AddColor(pos, r, g, b, a);
        }

        // add a keyframe color at a position along the gradient line
        public void AddColor(double pos, byte r, byte g, byte b, byte a)
        {
            ColorPt c = new ColorPt(pos, r, g, b, a);
            colors.Add(c);
            colors.Sort(compareFunc);
        }

        public System.Drawing.Color GetColor (double n)
        {
            int r, g, b, a;
            GetColor(n, out r, out g, out b, out a);
            return System.Drawing.Color.FromArgb(a,r, g, b);
        }

        public void GetColor(double n, out int red, out int green, out int blue, out int alpha)
        {
            int r, g, b, a;
            //if(n < 0 || n > 1) return System.Drawing.Color.Purple;
            if (n <= 0.0)
            {
                r = (int)colors[0].r;
                g = (int)colors[0].g;
                b = (int)colors[0].b;
                a = (int)colors[0].a;
                red = r;
                green = g;
                blue = b;
                alpha = a;
                return;
            }
            if (n >= 1.0)
            {
                int index = colors.Count - 1;
                r = (int)colors[index].r;
                g = (int)colors[index].g;
                b = (int)colors[index].b;
                a = (int)colors[index].a;
                red = r;
                green = g;
                blue = b;
                alpha = a;
                return;
            }

            ColorPt left = default(ColorPt );
            ColorPt right = default(ColorPt);
            for (int i = 0; i < colors.Count; i++)
            {
                if (n < colors[i].pos)
                {
                    left = colors[i - 1];
                    right = colors[i];  	// 'i' is gauranteed to be a right hand side at this point.
                    break;
                }
            }

                    
            double dist = right.pos - left.pos;
            double frac = (n - left.pos) / dist;

            // interpolate the rgb values based on the "keyframe" colors
            r = (int)LibNoise.Math.LinearInterpolate(left.r, right.r, frac); // ((left.r * (1.0 - frac)) + (right.r * frac));
            g = (int)LibNoise.Math.LinearInterpolate(left.g, right.g, frac); //((left.g * (1.0 - frac)) + (right.g * frac));
            b = (int)LibNoise.Math.LinearInterpolate(left.b, right.b, frac); //((left.b * (1.0 - frac)) + (right.b * frac));
            a = (int)LibNoise.Math.LinearInterpolate(left.a, right.a, frac); //((left.a * (1.0 - frac)) + (right.a * frac));
            red = r;
            green = g;
            blue = b;
            alpha = a;
        }
    }
}
