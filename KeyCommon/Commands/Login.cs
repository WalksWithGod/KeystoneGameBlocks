using System;
using System.Collections.Generic;
using Lidgren.Network;

namespace KeyCommon.Commands
{
    //public class Login : NetCommandBase 
    //{
    //    public string Name;
    //    public bool Direct;
    //    public string HashedPassword;  // only filled in if Direct = true;
    //    public byte[] AuthLogin;
    //    public string Tag;  // if a game server logging into lobby, it also supplies the listening port +"|" + address and subnets the game server will use to which the lobby must forward to players who register to play on that game server  
    //    // e.g.  Tag = "7779|127.0.0.1:255.255.255.255,192.168.1.64:255.255.255.0
    //    // The address is necessary because the Lobby can't assume that the IP the game server is connecting 
    //    //thru will be the same the clients must use
   


    //    public Login()
    //    {
    //        mCommand = (int)(int)Enumerations.Types.Authenticate;
    //    }

    //    public void SetGameServerTag(int port, string csvIPsAndMasks )
    //    {
    //        Tag = port.ToString() + "|" + csvIPsAndMasks;
    //    }

    //    public static string ParseInterfaces(string loginTag )
    //    {
    //        string[] s = loginTag.Split( new char[]{'|'});
    //        if (s == null || s.Length < 2 ) 
    //            return "";

    //            return s[1];
    //    }

    //    public static int ParsePort(string  loginTag )
    //    {
    //        string[] s = loginTag.Split(new char[]{'|'});

    //        if (s == null || s.Length < 1 ) 
    //            return 0;

    //        int port = 0;
    //        int.TryParse(s[0], out port);
    //        return port;
    //    }


    //    #region IRemotableType Members
    //    public override void Read(NetBuffer buffer)
    //    {
    //        Direct = buffer.ReadBoolean();
    //        Name = buffer.ReadString();
    //        if (Direct) 
    //            HashedPassword = buffer.ReadString();
    //        else
    //        {
    //            // login data
    //            int length = buffer.ReadInt32();
    //            if (length > 0)
    //                AuthLogin = buffer.ReadBytes(length);
    //        }
    //        Tag = buffer.ReadString();
    //    }

    //    public override void Write(NetBuffer buffer)
    //    {
    //        buffer.Write(Direct);
    //        buffer.Write(Name);
    //        if (Direct) 
    //            buffer.Write(HashedPassword);
    //        else
    //        {
    //            // login length and data
    //            int length;
    //            if (AuthLogin == null) 
    //                length = 0;
    //            else
    //                length = AuthLogin.Length;
                
    //            buffer.Write(length);
    //            if (length > 0)
    //                buffer.Write(AuthLogin);
    //        }
    //        buffer.Write(Tag);
    //    }
    //    #endregion
    //}
}
