using System;

namespace Keystone.Immediate_2D
{

    public interface IRenderable2DItem
    {
        bool AlphaBlend { get; }
        int Color { get;}
        void Draw();
    }
}
