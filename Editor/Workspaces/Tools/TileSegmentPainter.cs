using System;
using Keystone.Extensions;
using Keystone.CSG;
using Keystone.Portals;

namespace KeyEdit.Workspaces.Tools
{
    public class TileSegmentPainter : InteriorSegmentPainter 
    {
        public TileSegmentPainter(Keystone.Network.NetworkClientBase netClient)
            : base(netClient)
        {
        }






        public void SetValue(string layerName, EdgeStyle style)
        {
            LayerName = layerName;
            mValue = style;
        }

        public EdgeStyle GetValue()
        {
            if (mValue == null) return null;
            return (EdgeStyle)mValue;
        }

    }
}
