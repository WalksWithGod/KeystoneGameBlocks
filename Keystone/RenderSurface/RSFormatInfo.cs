using System;
using MTV3D65;
using Keystone.Resource;

namespace Keystone.RenderSurfaces
{
    public class RSFormatInfo : PoolContext
    {
        public RSResolution Resolution;
        public CONST_TV_RENDERSURFACEFORMAT ColorFormat;
        public CONST_TV_TEXTURETYPE SurfaceType;
        public bool UseDepthBuffer;
        public bool UseMainBuffer;
        public double MainBufferScale;
    }
}
