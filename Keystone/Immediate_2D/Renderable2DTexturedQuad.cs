using System;
using Keystone.Appearance;
using Keystone.IO;
using Keystone.Types;

namespace Keystone.Immediate_2D
{
    /// <summary>
    /// This class is used by RegionPVS to create a drawable bucket item.
    /// See Elements\TexturedQuad2D for an actual Scene Node.
    /// 
    /// 2D Quad always drawn on the near plane using screenspace coordinates.  THis is good for
    /// our Golf 98 style menus and for some dialogs.
    /// If we want a true 3D Quad that exists in world space use Billboard.cs (eg. Hud proxy icons)
    /// </summary>
    /// <remarks>
    /// See Elements\TexturedQuad2D for an actual Scene Node.
    /// </remarks>
    public class Renderable2DTexturedQuad : IRenderable2DItem 
    {
        private bool mUseAlpha; 
        private Texture mTexture;
        //private float mLeft;
        //private float mTop;
        private float mWidth;
        private float mHeight;

        private float mCenterX;
        private float mCenterY;
        private float mAngleRadians;

        private int mColorValue1;
        private int mColorValue2;
        private int mColorValue3;
        private int mColorValue4;

        public Renderable2DTexturedQuad(string resourceDescriptor, float centerX, float centerY, float width, float height, float angleRadians, bool alphaBlend) :
        	this ((Texture)Resource.Repository.Create(resourceDescriptor, "Texture"), centerX, centerY, width, height, angleRadians, alphaBlend)
        {
        	// TODO: this constructor will add a texture to cache but will potentially never release it.  We need to have a better way of
        	// caching these and maintaining track of the ones that a script will have ended up creating.  Maybe we can even force the
        	// scripts to precache them
        }
        
        public Renderable2DTexturedQuad(string resourceDescriptor, float centerX, float centerY, float width, float height, float angleRadians, bool alphaBlend, int color1) :
        	this ((Texture)Resource.Repository.Create(resourceDescriptor, "Texture"), centerX, centerY, width, height, angleRadians, alphaBlend, color1)
        {
        	// TODO: this constructor will add a texture to cache but will potentially never release it.  We need to have a better way of
        	// caching these and maintaining track of the ones that a script will have ended up creating.  Maybe we can even force the
        	// scripts to precache them
        }
        
        public Renderable2DTexturedQuad(string resourceDescriptor, float centerX, float centerY, float width, float height, float angleRadians, bool alphaBlend, int color1, int color2) :
        	this ((Texture)Resource.Repository.Create(resourceDescriptor, "Texture"), centerX, centerY, width, height, angleRadians, alphaBlend, color1, color2)
        {
        	// TODO: this constructor will add a texture to cache but will potentially never release it.  We need to have a better way of
        	// caching these and maintaining track of the ones that a script will have ended up creating.  Maybe we can even force the
        	// scripts to precache them
        }
        
        public Renderable2DTexturedQuad(Keystone.Appearance.Texture texture, float centerX, float centerY, float width, float height, float angleRadians, bool alphaBlend) : 
        	this (texture, centerX, centerY, width, height, angleRadians, alphaBlend, 
        	     CoreClient._CoreClient.Globals.RGBA(0.325f, 0.31f, 0.28f, 1f),
        	     CoreClient._CoreClient.Globals.RGBA(0.325f, 0.31f, 0.28f, 1f),
        	     CoreClient._CoreClient.Globals.RGBA(0.325f, 0.31f, 0.28f, 1f),
        	     CoreClient._CoreClient.Globals.RGBA(0.325f, 0.31f, 0.28f, 1f))
        {
        }
        
        public Renderable2DTexturedQuad(Keystone.Appearance.Texture texture, float centerX, float centerY, float width, float height, float angleRadians, bool alphaBlend, int color1) : 
        	this (texture, centerX, centerY, width, height, angleRadians, alphaBlend, 
        	     color1,
        	     color1,
        	     color1,
        	     color1)
        {
        }
        
    public Renderable2DTexturedQuad(Keystone.Appearance.Texture texture, float centerX, float centerY, float width, float height, float angleRadians, bool alphaBlend, int color1, int color2) : 
        	this (texture, centerX, centerY, width, height, angleRadians, alphaBlend, 
        	     color1,
        	     color2,
        	     color1,
        	     color1)
        {
        }
        
        public Renderable2DTexturedQuad(Keystone.Appearance.Texture texture, float centerX, float centerY, float width, float height, float angleRadians, bool alphaBlend, 
                                        int color1, int color2, int color3, int color4)
        {
        	mTexture = texture;
        	
        	// TODO: this is an awful hack really... need a better way for our scripts to load a texture, cache it and recycle it
        	Keystone.IO.PagerBase.LoadTVResource (mTexture, false);
        	
        	
        	mCenterX = centerX;
            mCenterY = centerY;
            mHeight = height;
            mWidth = width;
            //mLeft = x - mWidth * .5f;
            //mTop = y - mHeight * .5f;

            mColorValue1 = color1;
            mColorValue2 = color2;
            mColorValue2 = color3;
            mColorValue2 = color4;

            mUseAlpha = alphaBlend;
            mAngleRadians = angleRadians;

            // float SecondaryColor;
            //                int SecondaryAlpha;
            //                float DistanceFactor =
            //                    Core._CoreClient.Maths.VLength(
            //                        Core._CoreClient.Maths.VSubtract(Core._CoreClient.Maths.VNormalize(_sun.Position), camera.LookAt));
            //                SecondaryColor = Math.Min(Math.Max(1 - DistanceFactor, 0)*1.25f, 1);
            //                SecondaryAlpha = Core._CoreClient.Globals.RGBA(SecondaryColor, SecondaryColor, SecondaryColor, 1);

            //                SecondaryColor = .4f; // Math.Max(1 - DistanceFactor, 0) / 1.5f;
            //                SecondaryAlpha = Core._CoreClient.Globals.RGBA(SecondaryColor, SecondaryColor, SecondaryColor, 1);
        }
        //public Renderable2DTexturedQuad (string resourceDescriptor, float x, float y, float width, float height) 
        //    : 
        //    this (resourceDescriptor, x, y, width ,height, false)
        //{
        //}


        
        public void Draw()
        {
            // TODO: following line moves to our DebugDrawer i guess that handles all our 2d
            //CoreClient._CoreClient.Screen2D.Draw_Texture(mTexture.TVIndex, mLeft, mTop, mLeft + mWidth, mTop + mHeight, mColorValue, mColorValue, mColorValue, mColorValue);

            //x  X coordinate of the center of texture on 2d screen 
            //Y  Y coordinate of the center of texture on 2d screen 
            //Width  Width of the texture on the screen 
            //Height  Height of the texture on the screen 
            //angle  Angle (in degrees or radians) of the rotation for the 2d texture. 
            //tu1 (Optional, value = 0)  Horizontal Texture coordinate of the left top corner (use 0 for full texture) 
            //tv1 (Optional, value = 0)  Vertical Texture coordinate of the left top corner (use 0 for full texture) 
            //tu2 (Optional, value = 1)  Horizontal Texture coordinate of the bottom right corner (use 1 for full texture) 
            //tv2 (Optional, value = 1)  Vertical Texture coordinate of the bottom right corner (use 1 for full texture) 
            //Color1 (Optional, value = &HFFFFFFFF)  Color of the left top corner that is multiplied by the texture color (usualy white) 
            //color2 (Optional, value = -2)  Color of the right top corner that is multiplied by the texture color (usualy white) 
            //color3 (Optional, value = -2)  Color of the bottom left corner that is multiplied by the texture color (usualy white) 
            //color4 (Optional, value = -2)  Color of the bottom right corner that is multiplied by the texture color (usualy white) 
           // mColorValue1 = mColorValue2 = mColorValue3 = mColorValue4 = Color.White.ToInt32();
            CoreClient._CoreClient.Screen2D.Draw_TextureRotated(mTexture.TVIndex, mCenterX, mCenterY, mWidth, mHeight, mAngleRadians, mColorValue1, mColorValue2, mColorValue3, mColorValue4);
            

        }

        public int Color1 { get { return mColorValue1; } set { mColorValue1 = value; } }
        public int Color2 { get { return mColorValue2; } set { mColorValue2 = value; } }
        public int Color3 { get { return mColorValue3; } set { mColorValue3 = value; } }
        public int Color4 { get { return mColorValue4; } set { mColorValue4 = value; } }

        #region IRenderable2DItem Members

        bool IRenderable2DItem.AlphaBlend { get { return mUseAlpha; } }
        
        int IRenderable2DItem.Color
        {
            get { return mColorValue1; } 
        }

        #endregion
    }
}
