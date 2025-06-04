using System;
using Keystone.Types;

namespace Keystone.Immediate_2D
{
    public class Renderable3DLines : IRenderable2DItem
    {
        private Keystone.Types.Line3d[] mLines;
        private Keystone.Types.Color[] mColor;
        private Keystone.Types.Vector3d mOffset;

        public int Color { get { return mColor[0].ToInt32(); } }

        // always return false, line primitives can never use alpha blending
        bool IRenderable2DItem.AlphaBlend { get { return false; } }


        public Renderable3DLines(Line3d[] lines, int color) : 
            this (lines, color, Vector3d.Zero())
        {
        }


        public Renderable3DLines(Line3d[] lines, int color, Vector3d offset) : 
            this (lines, new Color(color), offset)
        {
        }

        public Renderable3DLines(Line3d[] lines, Color color) :
            this (lines, color, Vector3d.Zero())
        {
        }

        public Renderable3DLines(Line3d[] lines, Color color, Vector3d offset)
        {
            mLines = lines;
            mColor = new Color[] { color };
            mOffset = offset;
        }

        public Renderable3DLines(Line3d[] lines, Color[] colors) :
            this (lines, colors, Vector3d.Zero())
        {
        }

        public Renderable3DLines(Line3d[] lines, Color[] colors, Vector3d offset)
        {
            if (lines.Length != colors.Length) throw new ArgumentOutOfRangeException();
            mLines = lines;
            mColor = colors;
            mOffset = offset;
        }

        public Renderable3DLines(Keystone.Types.Line3d[] lines, int[] colors) : 
            this (lines, colors, Vector3d.Zero())
        {
        }

        public Renderable3DLines(Keystone.Types.Line3d[] lines, int[] colors, Vector3d offset)
        {
            if (lines.Length != colors.Length) throw new ArgumentOutOfRangeException();
            Color[] c = new Color[colors.Length];

            for (int i = 0; i < colors.Length; i++)
                c[i] = new Color(colors[i]);

            mLines = lines;
            mColor = c;
            mOffset = offset;
        }

        public void Draw ()
        {
            Keystone.Types.Line3d line;
            Keystone.Types.Vector3d start, end;

            if (mColor.Length == 1)
                for (int i = 0; i < mLines.Length; i++)
                {
                    line = mLines[i];
                    start = line.Point[0] + mOffset;
                    end = line.Point[1] + mOffset;
                    int color = (int)mColor[0].ToInt32();
                    CoreClient._CoreClient.Screen2D.Draw_Line3D(
                                                (float)start.x,
                                                (float)start.y,
                                                (float)start.z,
                                                (float)end.x,
                                                (float)end.y,
                                                (float)end.z,
                                                color);
                }
            else
                for (int i = 0; i < mLines.Length; i++)
                {
                    line = mLines[i];
                    start = line.Point[0] + mOffset;
                    end = line.Point[1] + mOffset;
                    CoreClient._CoreClient.Screen2D.Draw_Line3D(
                                                (float)start.x,
                                                (float)start.y,
                                                (float)start.z,
                                                (float)end.x,
                                                (float)end.y,
                                                (float)end.z,
                                                (int)mColor[i].ToInt32());
                }
        }
    }
}
