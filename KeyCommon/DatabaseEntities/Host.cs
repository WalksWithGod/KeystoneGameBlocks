using System;
using System.Collections.Generic;
using System.Net;
using System;
using Lidgren.Network;

namespace KeyCommon.DatabaseEntities
{
    /// <summary>
    /// Host is a sub table for a Game entity that is tracked in the database.
    /// </summary>
    public class Host
    {
        public string Name;
        public string Password;
        public Lidgren.Network.IPAddressWithMask[] EndPoints;
        public int Port;                  // TODO: since a Host can server multiple games, there's more than one listen port?
                                          // or will all just share?  maybe they do actually share...  since my network is detached from the hosting aspect
                                          // and the networking maybe does use just one?  maybe that should be changed then?
                                          // afterall, i need to use different authentication accounts for each game hosted on the same server
        public bool UsesNat;         //	Indicates if the host uses NAT punchthrough.
        public uint UpTime;

        public object Tag;         // not serialized in IRemotableType or stored \ retreived from DB


        public Host()
        {
           // mTypeID = (int)KeyCommon.Messages.Enumerations.Host;
        }


        public Host(string listenTable, string password) : this()
        {
            if (string.IsNullOrEmpty(listenTable)) return;
            Password = password;
            EndPoints = IPAddressWithMask.Parse(listenTable);
        }

        public Host(Lidgren.Network.IPAddressWithMask[] ipendpoints) : this()
        {
            EndPoints = ipendpoints;
        }


        public string ListenTable
        {
            get { return IPAddressWithMask.ToString(EndPoints); }
        }

        public Host Clone()
        {
            Host host = new Host();
            host.Name = this.Name;  // NOTE: Password is not cloned

            if (this.EndPoints != null && this.EndPoints.Length > 0)
            {
                host.EndPoints = new IPAddressWithMask[this.EndPoints.Length];
                for (int i = 0; i < this.EndPoints.Length; i++)
                {
                    host.EndPoints[i] = this.EndPoints[i].Clone();
                }
            }

            host.Port = this.Port;
            host.UsesNat = this.UsesNat;
            host.UpTime = this.UpTime;
            host.Tag = null;
            return host;
        }

        #region Entity members
        //public override void Read(Lidgren.Network.NetBuffer buffer)
        //{
        //    try
        //    {
        //        Name = buffer.ReadString();  // NOTE: Password is not serialized
        //        UpTime = buffer.ReadUInt32();
        //        UsesNat = buffer.ReadBoolean();
        //        Port = buffer.ReadInt32();
        //        byte count = 0;
        //        count = buffer.ReadByte();
        //        if (count > 0)
        //        {
        //            EndPoints = new IPAddressWithMask[count];
        //            for (int i = 0; i < count; i++)
        //            {
        //                IPAddress address = IPAddress.Parse(buffer.ReadString());
        //                IPAddress subnet = IPAddress.Parse(buffer.ReadString());
        //                EndPoints[i] = new IPAddressWithMask(address, subnet);
        //            }
        //        }
        //    }
        //    catch
        //    {

        //    }

        //    // TODO: malformed packet could not be read.
        //}

        //public override void Write(Lidgren.Network.NetBuffer buffer)
        //{
        //    if ((buffer == null)) return;

        //    try
        //    {
        //        buffer.Write(Name);  // NOTE: Password is not serialized
        //        buffer.Write(UpTime);
        //        buffer.Write(UsesNat);
        //        buffer.Write(Port);
        //        byte count = 0;
        //        if ((EndPoints != null && EndPoints.Length > 0))
        //        {
        //            count = (byte)EndPoints.Length;
        //        }
        //        buffer.Write(count);
        //        if (count > 0)
        //        {
        //            for (int i = 0; i <= count - 1; i++)
        //            {
        //                buffer.Write(EndPoints[i].Address.ToString());
        //                buffer.Write(EndPoints[i].SubnetMask.ToString());
        //            }
        //        }
        //    }
        //    catch
        //    {

        //    }
        //    // TODO: buffer write error


        //    return;
        //}
        #endregion
    }
}
