using System;

namespace KeyCommon.DatabaseEntities
{
    public struct HostRules
    {
        public uint TimeLimit;
        public uint ShipBuildPoints; // used to limit the number of build points each player can have so that fleets are competitive
        // so you can build several tiny ships, or one larger ship but the points will be the same
    }

    /// <summary>
    /// 
    /// </summary>
    public class GameServerInfo : Lidgren.Network.IRemotableType
    {
        private Lidgren.Network.NetChannel _channel;

        //The host information, HostData, object sent during host registrations or queries contains the following information:
        //public Authentication.AuthenticatedLogin Login;
        //public GameType Type; //	The game type of the host.
        //public HostRules Rules;
        public string Name; //	The game name of the host.
        public string IP; //	The internal IP address of the host. On a server with a public address the external and internal addresses are the same. This is an array as when connecting internally, all the IP addresses associated with all the active interfaces of the machine need to be checked.
        public int Port; //	The port of the host.
        public bool UsesNat; //	Indicates if the host uses NAT punchthrough.
        public uint Uptime;
        public string CurrentMap;
        public uint Players; //	The amount of currently connected players/clients.
        public uint PlayerLimit;//	The maximum amount of allowed concurrent players/clients.
        public uint Spectators;
        public uint SpectatorsLimit;
        public bool PasswordProtected; //for private servers. Indicates if you need to supply a password to be able to connect to this host.
        public string Message; // TODO: message field needs to be limited to x characters.

        #region IRemotableType Members
        public int Type
        {
            get
            {
                return (int)KeyCommon.Messages.Enumerations.GameServerInfo;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Lidgren.Network.NetChannel Channel
        {
            get
            {
                return _channel;
            }
            set
            {
                _channel = value;
            }
        }

        public void Read(Lidgren.Network.NetBuffer buffer)
        {
            try
            {
                // TODO: this is wrong, i dont believe this command_id should be written here since it moust obviously be read
                // prior to knowing which struct to call .FromBuffer on!
                // I should have a IRemoteable interface for these structs that implement the FromBuffer and ToBuffer

                Port = buffer.ReadInt32();
                Uptime = buffer.ReadUInt32();
                Players = buffer.ReadUInt32();
                PlayerLimit = buffer.ReadUInt32();
                Spectators = buffer.ReadUInt32();
                SpectatorsLimit = buffer.ReadUInt32();
                UsesNat = buffer.ReadBoolean();
                PasswordProtected = buffer.ReadBoolean();

                Name = buffer.ReadString();
                IP = buffer.ReadString();
                CurrentMap = buffer.ReadString();
                Message = buffer.ReadString();
            }
            catch
            {
                // malformed packet could not be read.
            }
        }

        public void Write(Lidgren.Network.NetBuffer buffer)
        {
            if (buffer == null) return;

            try
            {
                buffer.Write(Port);
                buffer.Write(Uptime);
                buffer.Write(Players);
                buffer.Write(PlayerLimit);
                buffer.Write(Spectators);
                buffer.Write(SpectatorsLimit);
                buffer.Write(UsesNat);
                buffer.Write(PasswordProtected);

                buffer.Write(Name);
                buffer.Write(IP);
                buffer.Write(CurrentMap);
                buffer.Write(Message);

            }
            catch
            {
                // buffer write error
                return;
            }
            return;
        }

        #endregion
    }
}
