using System;


namespace Keystone.Immediate_2D
{

    // http://www.truevision3d.com/forums/tv3d_sdk_63/3d_text_label-t15.0.html
    public class Renderable2DText : IRenderable2DItem
    {
        protected int mTextureFontID;
        protected string mText;
        protected float mLeft, mTop;
        protected int mColor;

        public int Color { get { return mColor; } }
        bool IRenderable2DItem.AlphaBlend { get { return false; } }

        public Renderable2DText (string text, int left, int top, int color, int fontID)
        {
            mTextureFontID = fontID;
            mText = text;
            mLeft = left;
            mTop = top;
            mColor = color;
        }

      
        //    Mesh3d mesh;
        //    mesh._mesh.Create3DText 
        public virtual void Draw()
        {
            CoreClient._CoreClient.Text.TextureFont_DrawText(mText, mLeft, mTop, (int)mColor, mTextureFontID);                     
        }
    }

    public class Renderable3DText : Renderable2DText
    {
        private Keystone.Types.Vector3d mPosition;
        private float mScaleX = 1;
        private float mScaleY = 1;
        public Renderable3DText(string text, double x, double y, double z, int color, int fontID, float scaleX, float scaleY)
            : base(text, 0, 0, color, fontID)
        {
            mPosition.x = x;
            mPosition.y = y;
            mPosition.z = z;
            mScaleX = scaleX;
            mScaleY = scaleY;
        }

        public Renderable3DText(string text, double x, double y, double z, int color, int fontID)
            : this (text, x, y, z, color, fontID, 1, 1)
        {

        }

        // TODO: different colors for ships, moon, stars, planets, etc
        public override void Draw()
        {
            mColor = (int)MTV3D65.CONST_TV_COLORKEY.TV_COLORKEY_WHITE;
            CoreClient._CoreClient.Text.TextureFont_DrawBillboardText(mText, (float)mPosition.x, (float)mPosition.y, (float)mPosition.z , (int)mColor, mTextureFontID, mScaleX, mScaleY);                     
        }
    }

}
