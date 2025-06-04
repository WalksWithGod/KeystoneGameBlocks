using Lidgren.Network;
using KeyCommon.Messages;

namespace KeyCommon.Messages
{
    public class Simulation_Spawn : MessageBase
    {

        // TODO: this is fundamentally broken... it assumes the client has the .kgbentity save file which only occurs during loopback,but maybe for 1.0 this is just good enough 
        //       otherwise we need to send the file or serialize the Entity hierarchy 
        public string EntitySaveRelativePath; //  could this be a prefab also? what if they are always prefabs and only the .save gets stored in the \\saves\\ path.  Well this would mean we'd need ALOT of prefabs.  For instance, if we want the same prefab but with a different texture, we'd need to save a seperate prefab for it
        // TODO: there is no need to modify the core of Lidren to accomplish the transfer of files.  We simply need an exe app layer to handle the assembling of filetransfer messages as they come in.
        // TODO: and this work should be postponed until version 2.0. I could add a flag indicating whether we are spawning a save or a prefab
        //       We may only need to set the NetConfiguration.MaximumUserPayloadSize property. 
        // TODO: but not being able to just serialize the entire node hierarchy is a problem.  If i compressed the data first, it should be pretty small even for large hierarchies. Afterall
        //       this is very much like paging in parts of the Scene at a time.  The server sends you chunks to spawn and with all the details you need to attach that Hierarchy to the Scene.
        //       Hell, why not in addition just have a Simulation_SpawnPrefab and require that user have all prefabs in the given \\modname\\ folder.
        //       The client must already have all for a given modname\\  resources such as meshes and textures.
        //      While we're at it, we can have seperate Spawn_* messages for Saved entities vs Scene chunks.  Also the server can remove Interior actors and only have the user spawn enemy crew on demand.
        //       We could have a seperate message yet again for Spawn_Interior which will include the cell data.  
        //       This is also useful client side for only loading the Zones relevant to the user at any given time.
        //       Simulation_LoadZone message to load the worlds and moons of based on area of interest        public byte[] EntityFileData;
        public byte[] CellDBData; // only relevant if this is a Container/Vehicle
        public string ParentID;
        public string[] NodeIDsToUse;
        public Keystone.Types.Vector3d Translation; // todo: i may need a rotation, scale (lasers use scale), initial velocity too. We also need the server assigned IDs for the nodes. These can be an array of PropertySpec which i do use in Node_ChangeProperties.cs

        // TODO: we need a way to transfer the prefab for saving on the client. This spawn assumes the client already has the save file which it does only in loopback scenario
        // it's probably best to just serialize the entire entity.kgbentity as a file rather than as nested hierarchy of nodes with propertyspecs[]

        // for resumed games  i.e. we use the userName to query a db table to find their Player record which has Vehicle prefab specified.
        // otherwise VehiclePrefab is passed in and used for first time entry.

        public Simulation_Spawn() : base ((int)Enumerations.Simulation_Spawn)
        {
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);

            EntitySaveRelativePath = buffer.ReadString();
            ParentID = buffer.ReadString();
            Translation.x = buffer.ReadDouble();
            Translation.y = buffer.ReadDouble();
            Translation.z = buffer.ReadDouble();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);

            buffer.Write(EntitySaveRelativePath);
            buffer.Write(ParentID);
            buffer.Write(Translation.x);
            buffer.Write(Translation.y);
            buffer.Write(Translation.z);
        }
        #endregion
    }
}
