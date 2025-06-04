using System;
using System.Collections.Generic;
using Lidgren.Network;


namespace KeyCommon.DatabaseEntities
{
    /// <summary>
    /// A list of summary information about a hosted game that were requested with a given set of filters applied.
    /// NOTE: Since a particular server can host multiple games, we distinguish between getting just the server versus
    /// the games on a particular server(s).
    /// </summary>
    public class GameSummary : GameObject
    {
        public string Name; // friendly server name
        public string ServerName;  // the authentication username 
        public string Map;
        public bool PasswordProtected;
        public string ListenTable;

        public GameSummary() : base((int)Messages.Enumerations.GameSummary)
        { }

        public int ID
        {
            get { return (int)Messages.Enumerations.GameSummary; }
        }

        public NetChannel Channel
        {
            get { return NetChannel.ReliableUnordered; }
        }

        public override void Read(NetBuffer buffer)
        {
            Name = buffer.ReadString();
            ServerName = buffer.ReadString();
            Map = buffer.ReadString();
            PasswordProtected = buffer.ReadBoolean();
            ListenTable = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            buffer.Write(Name);
            buffer.Write(ServerName);
            buffer.Write(Map);
            buffer.Write(PasswordProtected);
            buffer.Write(ListenTable);
        }
    }
}
