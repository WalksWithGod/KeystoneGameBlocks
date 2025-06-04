using System;
using Lidgren.Network;

namespace KeyCommon.Commands
{
    //public class EntityList : NetCommandBase
    //{
    //    public Entities.DatabaseEntity[] Entities;
    //    public int[] EntityTypes;

    //    public EntityList()
    //    {
    //        mCommand = (int)Enumerations.Types.EntityList;
    //    }

    //    #region IRemotableType Members
       
    //    // public bool Read (NetBuffer buffer, out bytesRead)
    //    public override void Read(NetBuffer buffer)
    //    {
    //        //int startPosition = buffer.Position;
    //        //int bitsRead; // used in our ref calls
            
    //        //// what if instead we had a system where
    //        //// at the start of the Read, we store the maxBytes that can be read in teh buffer
    //        //// and then we determine if for the reads we need to do, if there's enough.
    //        //// then after doing the read, 
    //        //if (buffer.LengthUnread  < 16) return false;
    //        //ushort count = buffer.ReadUInt16();
            
    //        //if (count > 0)
    //        //{
    //        //    Entities = new Entities.Entity[count];
    //        //    EntityTypes = new int[count];

    //        //    // if there's not enough space to read all of the entity type id's, return false
    //        //    if (buffer.LengthUnread < 16 * count) return false;
    //        //    for (ushort i = 0; i < count; i++)
    //        //        EntityTypes[i] = buffer.ReadUInt16();

    //        //    for (ushort i = 0; i < count; i++)
    //        //    {
    //        //        // TODO: do i just use a try/catch over all this crap? maybe
    //        //        // Read should return a bool?  But im not thrilled at the prospect
    //        //        // of having to test every read to see if it was successful.  
    //        //        // 
    //        //        Entities.Entity newEnt = KeyCommon.Entities.Factory.Create(EntityTypes[i]);
    //        //        //bool result = newEnt.Read(buffer);
    //        //        //if (!result) return false;

    //        //        Entities[i] = newEnt;
    //        //    }
    //        //}
    //    }

    //    public override void Write(NetBuffer buffer)
    //    {
    //        throw new NotImplementedException();
    //    }
    //    #endregion
    //}
}
