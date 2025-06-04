using System;
using MTV3D65;
using Keystone.Types;
using System.Drawing;


namespace Keystone.Immediate_2D
{
    public class Renderable2DPolygon : IRenderable2DItem 
    {
        private Vector3d[] mPoints;
        private int mColor;
        private bool mFilled;
        
        public int Color { get { return mColor; } }

        // always return false, line primitives can never use alpha blending
        bool IRenderable2DItem.AlphaBlend { get { return false; } }


        public Renderable2DPolygon(Vector3d[] points, int color, bool filled = false)
        {
            mPoints = points;
            mColor = color;
            mFilled = filled;
        }

        public void Draw()
        {
            PointF[] points2d = new PointF[mPoints.Length];
            float x = 0, y = 0;
            for (int i = 0; i < mPoints.Length; i++)
            {
                // because these lines have to be projected to 2d, it depends on viewport and because these draw commands can
                // occur during things (mostly for debug) like AdvancedCollide, which is not specific to any viewport, then the current
                // projection used might be off.  Thus we will project now during the CommitDebugDraw where we're guaranteed to
                // have the proper current viewport set prior to doing the projection call
                CoreClient._CoreClient.Screen2D.Math_3DPointTo2D(Helpers.TVTypeConverter.ToTVVector(mPoints[i]), ref x, ref y);
                points2d[i] = new PointF(x, y);
            }


            if (points2d.Length == 3)
            {
                if (!mFilled)
                    CoreClient._CoreClient.Screen2D.Draw_Triangle(points2d[0].X, points2d[0].Y, points2d[1].X, points2d[1].Y, points2d[2].X, points2d[2].Y, mColor);
                else
                    CoreClient._CoreClient.Screen2D.Draw_FilledTriangle(points2d[0].X, points2d[0].Y, points2d[1].X, points2d[1].Y, points2d[2].X, points2d[2].Y, mColor);
            }
            else
            {
                TV_CUSTOM2DVERTEX[] verts = new TV_CUSTOM2DVERTEX[mPoints.Length + 1];

                for (int i = 0; i < mPoints.Length; i++)
                {
                    verts[i].color = (uint)mColor;
                    verts[i].x = points2d[i].X;
                    verts[i].y = points2d[i].Y;
                }
                
                // connect last point back to first point
                verts[mPoints.Length].color = (uint)mColor;
                verts[mPoints.Length].x = points2d[0].X;
                verts[mPoints.Length].y = points2d[0].Y;

                CoreClient._CoreClient.Screen2D.Draw_Custom(0, CONST_TV_PRIMITIVETYPE.TV_LINESTRIP, verts, verts.Length);
            }
        }
    }
}
