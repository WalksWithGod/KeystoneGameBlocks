//using System;
//using Lidgren.Network;
//using KeyCommon.Messages;
//using Keystone.Types;
//using Keystone.Portals;


//namespace KeyCommon.Messages
//{
////  OBSOLETE - User now must send Node_Create_Request message along with property list to assign
//    public class Scene_LoadLight : MessageBase
//    {

//        public enum LightType : byte
//        {
//            Directional,
//            Pointlight,
//            Spotlight
//        }

//        public string ParentID;
//        public Vector3d Position;

//        public LightType Type;
//        public Vector3d Direction;
//        public Color Color;
//        public float Range;
//        public float Phi;
//        public float Theta;
//        public float Attenuation0;
//        public float Attenuation1;
//        public float Attenuation2;


//        // TODO: if we were adding the light to the placer/thrower tool we wouldn't need a parent, but we also have the potential
//        // to dyanmically add lights to a scene such as when a server tells us to create a light in the level after one got installed
//        // by a multiplayer opponent on their ship during a boarding party raid you're involved in.
//        // So for those we should have some kind of differentiating command like... first you create the light then you 
//        // assign it or something somehow...  
//        // Actually, hrm.... i think most of those nodes that we'd spawn we could just send the xml for with a typical
//        // parent id specified and then use our typical xml prefab load code?  
//        // And i think here we shoudl do the same thing... CreateLight can just load an xml 
//        public Scene_LoadLight() : base ((int)Enumerations.AddLight) { }

//        public Scene_LoadLight(string parent)
//            : this()
//        {
//            ParentID = parent;
//            Color = new Color(1f, 1f, 1f, 1f);
//        }

//        public Scene_LoadLight(string parent, Vector3d direction)
//            : this(parent)
//        {
//            Type = LightType.Directional;
//            Direction = direction;
//        }

//        public Scene_LoadLight(string parent, Vector3d localPosition, Color color, float range, float attenuation0, float attenuation1, float attenuation2)
//            : this (parent)
//        {
//            Position = localPosition;
//            Color = color;
//            Type = LightType.Pointlight;
            
//            Range = range;
//            Attenuation0 = attenuation0;
//            Attenuation1 = attenuation1;
//            Attenuation2 = attenuation2;
//        }

//        public Scene_LoadLight(string parent, Vector3d localPosition, Vector3d direction, float range, float fallOff, float phi, float theta)
//            : this(parent, direction) 
//        {
//            Position = localPosition;

//            Type = LightType.Spotlight;
//            Direction = direction;
//            Range = range;
//            Phi = phi;
//            Theta = theta;
//        }


//        #region IRemotableType Members
//        public override void Read(NetBuffer buffer)
//        {
//            base.Read(buffer);
//            ParentID = buffer.ReadString();
//            double x, y, z;
//            x = buffer.ReadDouble();
//            y = buffer.ReadDouble();
//            z = buffer.ReadDouble();
//            Position = new Vector3d(x,y,z);
//            x = buffer.ReadDouble();
//            y = buffer.ReadDouble();
//            z = buffer.ReadDouble();
//            Direction = new Vector3d(x,y,z);
//            Range = buffer.ReadFloat();

//            Type = (LightType)buffer.ReadByte();
//            float r =  buffer.ReadFloat();
//            float g = buffer.ReadFloat();
//            float b = buffer.ReadFloat();
//            float a = buffer.ReadFloat();
//            Color = new Color(r, g, b, a);
//            Phi = buffer.ReadFloat();
//            Theta = buffer.ReadFloat();
//            Attenuation0 = buffer.ReadFloat();
//            Attenuation1 = buffer.ReadFloat();
//            Attenuation2 = buffer.ReadFloat();
//        }

//        public override void Write(NetBuffer buffer)
//        {
//            base.Write(buffer);
//            buffer.Write(ParentID);
//            buffer.Write(Position.x);
//            buffer.Write(Position.y);
//            buffer.Write(Position.z);
//            buffer.Write(Direction.x);
//            buffer.Write(Direction.y);
//            buffer.Write(Direction.z);
//            buffer.Write(Range);

//            buffer.Write((byte)Type);
//            buffer.Write(Color.r);
//            buffer.Write(Color.g);
//            buffer.Write(Color.b);
//            buffer.Write(Color.a);
//            buffer.Write(Phi);
//            buffer.Write(Theta);
//            buffer.Write(Attenuation0);
//            buffer.Write(Attenuation1);
//            buffer.Write(Attenuation2);

//        }
//        #endregion

//    }
//}
