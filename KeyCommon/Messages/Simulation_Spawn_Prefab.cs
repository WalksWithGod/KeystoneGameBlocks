using Lidgren.Network;
using KeyCommon.Messages;

namespace KeyCommon.Messages
{
    // todo: we already have a command Prefab_Load.  What does this do that is different?
    //       And Prefab_Load has same issue with ID of the child nodes not being set by the server.
    // In the short term just to test lasers i can probably skip the assignment of proper node IDs. I think 
    // the easiest way is to store the IDs in a stack<string> and then as we resurse through the nodes we pop the current ID from the stack.
    // Is there ever a case where the client can create its own IDs that don't need to match the server?  I think that would be bad practice.
    // client and server should be sychrnoized.
    public class Simulation_Spawn_Prefab : MessageBase
    {
        public string UserName;
        public string PrefabRelativePath; 
        
        public string ParentID;
        public string ID; // TODO: should we have an array of IDs for every node in the prefab? This is an existential flaw because the client and server do not have sychronized IDs.  Only the top entity ID is sychronized
        public Keystone.Types.Vector3d Translation; // todo: i may need a rotation, scale (lasers use scale), initial velocity too. We also need the server assigned IDs for the nodes.


        public Simulation_Spawn_Prefab() : base ((int)Enumerations.Simulation_Spawn)
        {
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);

            UserName = buffer.ReadString();
            PrefabRelativePath = buffer.ReadString();
            ParentID = buffer.ReadString();
            Translation.x = buffer.ReadDouble();
            Translation.y = buffer.ReadDouble();
            Translation.z = buffer.ReadDouble();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);

            buffer.Write(UserName);
            buffer.Write(PrefabRelativePath);
            buffer.Write(ParentID);
            buffer.Write(Translation.x);
            buffer.Write(Translation.y);
            buffer.Write(Translation.z);
        }
        #endregion
    }
}
