//using System;
//using System.Collections.Generic;
//using KeyCommon.Messages;
//using Lidgren.Network;


//namespace KeyCommon.Messages
//{
//    public class CelledRegion_PaintLink : MessageBase
//    {

//        public string ParentCelledRegionID;
//        public string LayerName;

//        public uint[] TileIndices;
//        public object PaintValue;

//        // results and cancel arrays are for CommandCompleted and so do not need to be serialized
//        public bool[] Cancel; // if null, all elements are assumed NOT canceled
//        public Keystone.Portals.MinimeshMap[][] WorkerResults;

//        public CelledRegion_PaintLink()
//            : base ((int)Enumerations.CelledRegion_PaintLink)
//        { }



//        #region IRemotableType Members
//        public override void Read(NetBuffer buffer)
//        {
//            base.Read(buffer);
//            ParentCelledRegionID = buffer.ReadString();

//            // TODO: i should have a "BrushStyle" id and where we know what type of brush
//            //       to instantiate and read/write is based on that StyleID

//            // TODO: here there is no way to know the type before
//            // we start to read in the value!  argh...
//            // i would need some bool like "IsCustomBrushStyle" to differentiate
//            // between more intrinsic var types, or i'd have to switch to all values
//            // set here to use CustomBrushStyle...perhaps actually allowing some to use
//            // the generic BrushStyle that implements IBrushStyle perhaps... for now
//            // we'll just use a bool
//            bool isCustomBrushStyle = buffer.ReadBoolean();
//            if (isCustomBrushStyle)
//            {
//                Keystone.Portals.SegmentStyle style = new Keystone.Portals.SegmentStyle();
//                style.Read(buffer);
//                PaintValue = style;
//            }
//            else
//            {
//                string typeName;
//                PaintValue = Keystone.Helpers.ExtensionMethods.ReadType(buffer, out typeName);
//            }

//            LayerName = buffer.ReadString();

//            // cell indices
//            uint count = buffer.ReadUInt32();
//            TileIndices = new uint[count];

//            for (int i = 0; i < count; i++)
//            {
//                TileIndices[i] = buffer.ReadUInt32();
//            }

//        }

//        public override void Write(NetBuffer buffer)
//        {
//            base.Write(buffer);
//            buffer.Write(ParentCelledRegionID);

//            bool isCustomBrushStyle = PaintValue is Keystone.Portals.SegmentStyle;
//            buffer.Write(isCustomBrushStyle);
//            if (isCustomBrushStyle)
//            {
//                ((Keystone.Portals.SegmentStyle)PaintValue).Write(buffer);
//            }
//            else
//                Keystone.Helpers.ExtensionMethods.WriteType(buffer, PaintValue);


//            // layer names
//            buffer.Write(LayerName);

//            // cell indices
//            uint count = 0;
//            if (TileIndices != null)
//                count = (uint)TileIndices.Length;

//            buffer.Write(count);
//            for (int i = 0; i < count; i++)
//            {
//                buffer.Write(TileIndices[i]);
//            }

//        }
//        #endregion
//    }
//}
