using System;
using Keystone.Resource;
using MTV3D65;

namespace Keystone.RenderSurfaces
{


    public class RenderSurfacePool<T> : ObjectPool<T> where T : RenderSurface
    {
        public RenderSurfacePool(int expirationTime)
            : base(expirationTime)
        {
        }

        public int LockedItemsTotalMemory
        {
            get
            {
                int consumed = 0;
                foreach (Object obj in _unavailable)
                {
                    RenderSurface rs = (RenderSurface) obj;
                    int multiplier = (rs.IsCubeMap) ? 6 : 1;
                    consumed += rs.Width * rs.Height * CoreClient._CoreClient.Graphics.BPP * multiplier;
                }

                return consumed;
            }
        }

        public int UnLockedItemsTotalMemory
        {
            get
            {
                int consumed = 0;
                foreach (Object obj in _available)
                {
                    RenderSurface rs = (RenderSurface) obj;
                    int multiplier = (rs.IsCubeMap) ? 6 : 1;
                    consumed += rs.Width * rs.Height * CoreClient._CoreClient.Graphics.BPP * multiplier;
                }

                return consumed;
            }
        }

        protected override bool Validate(T rs, PoolContext context)
        {
            RSFormatInfo info = (RSFormatInfo) context;
            //validate that the item we wish to checkout, matches the specification of the RenderSurface requested
            if (info.Resolution != rs._resolution) return false;
            if (info.ColorFormat != rs._colorFormat) return false;
            if (info.SurfaceType != rs._surfaceType) return false;
            if (info.UseDepthBuffer != rs._useDepthBuffer) return false;
            if (info.UseMainBuffer != rs._useMainBuffer) return false;
            if (info.MainBufferScale != rs._mainBufferScale) return false;

            return true;
        }

        protected override T Create()
        {
            return null;
        }
    }
}