using System;
using Lidgren.Network;

namespace KeyCommon.Messages
{
    // Exterior TileMap.Structure
    public class TileMapStructure_PaintCell : MessageBase
    {

        public string ParentStructureID;
        public string LayerName;
        public int FloorLevel;

        public uint[] Indices;

        public object PaintValue;
        public Messages.PaintCellOperation.PAINT_OPERATION Operation;


        // results and cancel arrays are for CommandCompleted and so do not need to be serialized
        public bool[] Cancel; // if null, all elements are assumed NOT canceled
        
        // command.WorkerProduct is used, not this redundant member
        // public Keystone.Portals.MinimeshMap[] WorkerResults;

        public TileMapStructure_PaintCell()
            : base((int)Enumerations.TileMapStructure_PaintCell)
        { }

        
        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            ParentStructureID = buffer.ReadString();

            // TODO: i should have a "BrushStyle" id and where we know what type of brush
            //       to instantiate and read/write is based on that StyleID

            // TODO: here there is no way to know the type before
            // we start to read in the value!  argh...
            // i would need some bool like "IsCustomBrushStyle" to differentiate
            // between more intrinsic var types, or i'd have to switch to all values
            // set here to use CustomBrushStyle...perhaps actually allowing some to use
            // the generic BrushStyle that implements IBrushStyle perhaps... for now
            // we'll just use a bool
            bool isCustomBrushStyle = buffer.ReadBoolean();
            if (isCustomBrushStyle)
            {
                Keystone.Portals.EdgeStyle style = new Keystone.Portals.EdgeStyle();
                style.Read(buffer);
                PaintValue = style;
            }
            else
            {
                string typeName;
                PaintValue = KeyCommon.Helpers.ExtensionMethods.ReadType(buffer, out typeName);
            }

            FloorLevel = buffer.ReadInt32();
            LayerName = buffer.ReadString();

            Operation = (KeyCommon.Messages.PaintCellOperation.PAINT_OPERATION)buffer.ReadByte();

            // tile indices
            uint count = buffer.ReadUInt32();
            Indices = new uint[count];

            for (int i = 0; i < count; i++)
            {
                Indices[i] = buffer.ReadUInt32();
            }

        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(ParentStructureID);

            bool isCustomBrushStyle = PaintValue is Keystone.Portals.EdgeStyle;
            buffer.Write(isCustomBrushStyle);
            if (isCustomBrushStyle)
            {
                ((Keystone.Portals.EdgeStyle)PaintValue).Write(buffer);
            }
            else
                Helpers.ExtensionMethods.WriteType(buffer, PaintValue);


            buffer.Write(FloorLevel);
            buffer.Write(LayerName);

            buffer.Write((byte)Operation);

            // tile indices
            uint count = 0;
            if (Indices != null)
                count = (uint)Indices.Length;

            buffer.Write(count);
            for (int i = 0; i < count; i++)
            {
                buffer.Write(Indices[i]);
            }

        }
        #endregion
    }

    // Interior Structure
    public class PaintCellOperation : MessageBase
    {

    	public enum PAINT_OPERATION : byte
    	{
    		REMOVE = 0,
    		ADD
    	}
    	
        public string ParentCelledRegionID;
        public string LayerName;
		public int FloorLevel;
		
        public uint[] Indices;
        
        public object PaintValue;
	
        public KeyCommon.Messages.PaintCellOperation.PAINT_OPERATION Operation;

        
        // results and cancel arrays are for CommandCompleted and so do not need to be serialized
        public bool[] Cancel; // if null, all elements are assumed NOT canceled

        // command.WorkerProduct is used, not this redundant member
        //public Portals.EdgeStyle[] WorkerResults;

        public PaintCellOperation()
            : base((int)Enumerations.CelledRegion_PaintCell)
        { }



        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            ParentCelledRegionID = buffer.ReadString();

            // TODO: i should have a "BrushStyle" id and where we know what type of brush
            //       to instantiate and read/write is based on that StyleID

            // TODO: here there is no way to know the type before
            // we start to read in the value!  argh...
            // i would need some bool like "IsCustomBrushStyle" to differentiate
            // between more intrinsic var types, or i'd have to switch to all values
            // set here to use CustomBrushStyle...perhaps actually allowing some to use
            // the generic BrushStyle that implements IBrushStyle perhaps... for now
            // we'll just use a bool
            bool isCustomBrushStyle = buffer.ReadBoolean();
            if (isCustomBrushStyle)
            {
                Keystone.Portals.EdgeStyle style = new Keystone.Portals.EdgeStyle();
                style.Read(buffer);
                PaintValue = style;
            }
            else
            {
                string typeName;
                PaintValue = Helpers.ExtensionMethods.ReadType(buffer, out typeName);
            }

            FloorLevel = buffer.ReadInt32();
            LayerName = buffer.ReadString();

            Operation = (PAINT_OPERATION)buffer.ReadByte();
            
            // tile indices
            uint count = buffer.ReadUInt32();
            Indices = new uint[count];

            for (int i = 0; i < count; i++)
            {
                Indices[i] = buffer.ReadUInt32();
            }

        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(ParentCelledRegionID);

            bool isCustomBrushStyle = PaintValue is Keystone.Portals.EdgeStyle; 
            buffer.Write(isCustomBrushStyle);
            if (isCustomBrushStyle)
            {
                ((Keystone.Portals.EdgeStyle)PaintValue).Write(buffer);
            }
            else
                Helpers.ExtensionMethods.WriteType(buffer, PaintValue);
    
            
            buffer.Write (FloorLevel);
            buffer.Write(LayerName);
            
            buffer.Write ((byte)Operation);
            
            // tile indices
            uint count = 0;
            if (Indices != null)
                count = (uint)Indices.Length;

            buffer.Write(count);
            for (int i = 0; i < count; i++)
            {
                buffer.Write(Indices[i]);
            }

        }
        #endregion
    }
}
