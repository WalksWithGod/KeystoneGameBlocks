using System;

namespace Keystone.Immediate_2D
{
    /// <summary>
    /// This class is used by RegionPVS to create a drawable bucket item.
    /// See Elements\TexturedQuad2D for an actual Scene Node.
    /// 
    /// 2D Quad always drawn on the near plane using screenspace coordinates.  This is good for
    /// our Golf 98 style menus and for some dialogs.
    /// If we want a true 3D Quad that exists in world space use Billboard.cs (eg. Hud proxy icons)
    /// </summary>
    /// <remarks>
    /// See Elements\TexturedQuad2D for an actual Scene Node.
    /// </remarks>
    public class Renderable2DQuad : IRenderable2DItem 
    {
    	private bool mFilled = true;
        private float mLeft;
        private float mTop;
        private float mRight;
        private float mBottom;

        private int mColorValue1;
        private int mColorValue2;
        private int mColorValue3;
        private int mColorValue4;

        
        public Renderable2DQuad(float left, float top, float right, float bottom) : 
        	this (left, top, right, bottom,  
        	     CoreClient._CoreClient.Globals.RGBA(0.325f, 0.31f, 0.28f, 1f),
        	     CoreClient._CoreClient.Globals.RGBA(0.325f, 0.31f, 0.28f, 1f),
        	     CoreClient._CoreClient.Globals.RGBA(0.325f, 0.31f, 0.28f, 1f),
        	     CoreClient._CoreClient.Globals.RGBA(0.325f, 0.31f, 0.28f, 1f))
        {
        }
        
        public Renderable2DQuad(float left, float top, float right, float bottom, int color1) : 
        	this (left, top, right, bottom,  
        	     color1,
        	     color1,
        	     color1,
        	     color1)
        {
        }
        
    public Renderable2DQuad(float left, float top, float right, float bottom, int color1, int color2) : 
        	this (left, top, right, bottom, 
	       	      color1,
        	     color2,
        	     color1,
        	     color2)
        {
        }
        
        public Renderable2DQuad(float left, float top, float right, float bottom,  
                                        int color1, int color2, int color3, int color4)
        {        	
        	mLeft = left;
            mTop = top;
            mBottom = bottom;
            mRight = right;

            mColorValue1 = color1;
            mColorValue2 = color2;
            mColorValue2 = color3;
            mColorValue2 = color4;            
        }
        
        //public Renderable2DQuad (float x, float y, float width, float height) 
        //    : 
        //    this ( x, y, width ,height, false)
        //{
        //}

        public void Draw()
        {

            //x  X coordinate of the center of texture on 2d screen 
            //Y  Y coordinate of the center of texture on 2d screen 
            //Width  Width of the texture on the screen 
            //Height  Height of the texture on the screen 
            //angle  Angle (in degrees or radians) of the rotation for the 2d texture. 

            //Color1 (Optional, value = &HFFFFFFFF)  Color of the left top corner that is multiplied by the texture color (usualy white) 
            //color2 (Optional, value = -2)  Color of the right top corner that is multiplied by the texture color (usualy white) 
            //color3 (Optional, value = -2)  Color of the bottom left corner that is multiplied by the texture color (usualy white) 
            //color4 (Optional, value = -2)  Color of the bottom right corner that is multiplied by the texture color (usualy white) 

            if (mFilled)
	            CoreClient._CoreClient.Screen2D.Draw_FilledBox(mLeft, mTop, mRight, mBottom, mColorValue1, mColorValue2, mColorValue3, mColorValue4);
    		else         		
				CoreClient._CoreClient.Screen2D.Draw_Box (mLeft, mTop, mRight, mBottom, mColorValue1);


        }

        public int Color1 { get { return mColorValue1; } set { mColorValue1 = value; } }
        public int Color2 { get { return mColorValue2; } set { mColorValue2 = value; } }
        public int Color3 { get { return mColorValue3; } set { mColorValue3 = value; } }
        public int Color4 { get { return mColorValue4; } set { mColorValue4 = value; } }

        #region IRenderable2DItem Members

        bool IRenderable2DItem.AlphaBlend { get { return false; } }
        
        int IRenderable2DItem.Color
        {
            get { return mColorValue1; } 
        }

        #endregion
    }
}
