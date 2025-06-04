using System;


namespace Keystone.States
{
    //TODO: obsolete? i think this was originally was supposed to be for
    // network state before i started using lidgren and IRemotable?
    public struct NodeState
    {

        int mStateID;  // some states can actually be a collection of states such as Position | Rotation | Velocity
        byte[] mValue;

        public NodeState(int id)
        {
            mStateID = id;
            mValue = null;
        }

        public NodeState(int id, byte[] value)
        {
            mStateID = id;
            mValue = value;
        }

        public int ID { get { return mStateID; } }
        public byte[] Value { get { return mValue; } set { mValue = value; } }
    }
}
