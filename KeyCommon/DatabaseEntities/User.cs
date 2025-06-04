using System;

namespace KeyCommon.DatabaseEntities
{
    // TODO: I Don't know if we want KeyCommon to hold our "DatabaseEntities."
    //       maybe just Game01.dll should host them?  I dont think keystone.dll needs access to them via KeyCommon.dll
    //       NOTE: I Think KeyCommon.dll is meant to be _common_ to Client and Server
    // TODO: And i need a factory method to instantiate the different type of DatabaseEntities and GameObjects right? keystone.dll does reference KeyCommon.dll, but i dont think it reall should right?
    public class User : GameObject 
    {
        public string Name      ;        // user name in authentication db.  When something like a lobby validates, it checks the authenticator that the usernames match
        public string Password;
        public long CustomerID  ; // foreign key to the Customer's table.  For security purposes, Customer's table should be in a seperate database on a seperate machine since that database will include payment and address information of the users
        public bool IsAdmin ;
        public bool IsSuspended ;
        public DateTime JoinedDate ;
        public object Tag; // not serialized in IRemotableType or stored \ retreived from DB

        public User(long id) : base(id)
        {

        }

        #region GameObject members
        public override void Read( Lidgren.Network.NetBuffer buffer)
        {
            mID = buffer.ReadInt64();
        }

        public override void Write(Lidgren.Network.NetBuffer buffer)
        {
            buffer.Write(mID);
        }
#endregion
    }
}
