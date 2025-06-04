using System;
using KeyCommon;
using Lidgren.Network;

namespace KeyCommon.Messages
{
    // OBSOLETE-See Simulation_Spawn.css instead
    /// <summary>
    /// Spawn's an entity in whole or in part (eg. clients wont necessarily 
    /// receive the interior components of npc or other player's ships, just the exterior)
    /// into the scene.  
    /// Spawning is not the same as loading a prefab.  Spawning specifically refers to
    /// serializing via IRemotableType using propertySpecs
    /// To serialize an entity, write the typename first, then 
    /// properties[]  = node.GetProperties();
    /// Write the property count
    /// Iterate through them and write them according to their type.
    /// Since each entity knows what properties it has, it will be able to read/write
    /// in binary without knowing the property names in advance.... i think?
    /// (Although maybe that's not a good idea, if we change the properties around
    /// it breaks?  Or maybe doesn't break since read/write will always be updated together)
    /// however it does seem to make it less robust.  Be much better if we could at least
    /// use property ID constants, but our XML doesnt use ID Constants. XML is supposed to be
    /// human readable.  Well, i think we should use the strings for now because it allows us to
    /// not have each Entity read/write itself.  We could very simply and generically 
    /// instance the entity, then node.SetProperties(specs[]) after we've deserialized them.
    /// It's quite elegant that way in fact.
    /// So in this way, perhaps we can send a command to server Node_Create (typename, parent)
    /// and have the server send us the Node_Create back with the node's properties all filled in?
    /// I think this is what we'll try...
    /// 
    /// Next, is there any harm in allowing users to create guid's for some node types
    /// that are not really critical to anything?  I think that would be begging for trouble.
    /// 
    /// </summary>
    ////public class Spawn  : KeyCommon.Messages.MessageBase 
    ////{

    ////    public string ID;  // ID of the entity
    ////    public string ParentID; // ID of the parent to attach this entity to
    ////    public Types.Vector3d Position;  // what about starting velocity vector?
    ////    // TODO: what about physics details? is that stored in the Entity prefab?
    ////    public KeyCommon.IO.ResourceDescriptor Resource;

       

    ////    public Spawn()
    ////        : base(KeyCommon.Messages.Enumerations.EntitySpawn)
    ////    { }



    ////    #region IRemotableType Members
    ////    public override void Read(NetBuffer buffer)
    ////    {
    ////        throw new NotImplementedException();
    ////        base.Read(buffer);
    ////        //ID = buffer.ReadString();
    ////        //ParentID = buffer.ReadString();
    ////        //RegionDiameterX = buffer.ReadDouble();
    ////        //RegionDiameterY = buffer.ReadFloat();
    ////        //RegionDiameterZ = buffer.ReadFloat();
    ////    }

    ////    public override void Write(NetBuffer buffer)
    ////    {
    ////        throw new NotImplementedException();
    ////        base.Write(buffer);
    ////        //buffer.Write(SceneName);
    ////        //buffer.Write(FileName);
    ////        //Position.Write(buffer);


    ////    }
    ////    #endregion


    ////}
}
