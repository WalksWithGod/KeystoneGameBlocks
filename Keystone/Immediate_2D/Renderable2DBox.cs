using System;


namespace Keystone.Immediate_2D
{
    public class Renderable2DBox : IRenderable2DItem 
    {
        private int mColor;

        public int Color { get { return mColor; } }

        // always return false, line primitives can never use alpha blending
        bool IRenderable2DItem.AlphaBlend { get { return false; } }

        public void Draw()
        {
 
        }

    }
}
