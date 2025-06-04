/* Copyright (c) 2008 Michael Lidgren

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Lidgren.Network
{
    public class IPAddressWithMask
    {
        public IPAddress Address;
        public IPAddress SubnetMask;

        public IPAddressWithMask(string address,  string subnet) : this (IPAddress.Parse (address), IPAddress.Parse(subnet))
        { 
        }

        public IPAddressWithMask(IPAddress ip, IPAddress subnet)
        {
            if (ip == null || subnet == null) throw new ArgumentNullException();
            Address = ip;
            SubnetMask = subnet;
        }

        public static IPAddressWithMask[] Parse(string csvTable)
        {
            string[] table = csvTable.Split(new char[] { ',' });
            if (table == null || table.Length < 1) return null;

            List<IPAddressWithMask> endpoints = new List<IPAddressWithMask>();
            IPAddress address;
            IPAddress subnet;

            // parse out the ip addresses which are not valid or which do not also have a port or subnet associated with it
            for (int i  = 0 ; i < table.Length ; i++)
            {
                string[] components = table[i].Split(new char[] { ':' });
                if (components == null || components.Length != 2) continue;

                address = ParseIP( components[0]);
                if (address == null) continue;

                subnet = ParseIP(components[1]);
                if (subnet == null) continue;

                endpoints.Add(new IPAddressWithMask(address, subnet));
            
            }
            return endpoints.ToArray();
        }

        private static IPAddress ParseIP(string ipAddr) 
        {
            try
            {
                return IPAddress.Parse(ipAddr);
            }
            catch (ArgumentNullException e)
            {
                Debug.WriteLine("ArgumentNullException caught!!!");
                return null; 
            }
            catch (FormatException e)
            {
                Console.WriteLine("FormatException caught!!!");
                Console.WriteLine(("Source : " + e.Source));
                Console.WriteLine(("Message : " + e.Message));
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception caught!!!");
                Console.WriteLine(("Source : " + e.Source));
                Console.WriteLine(("Message : " + e.Message));
                return null;
            }
        }

        public static string ToString(IPAddressWithMask[] endpoints)
        {
            if (endpoints == null || endpoints.Length < 1) return "";

            string result = "";
            
            string[] s = new string[endpoints.Length ];

            for (int i = 0; i < endpoints.Length ; i++)
                s[i] = endpoints[i].ToString();

            return  string.Join(",", s);
        }

        public override string ToString()
        {
            return Address.ToString() +  ":" + SubnetMask.ToString();
        }

        public IPAddressWithMask Clone()
        {
            IPAddressWithMask result = new IPAddressWithMask(this.Address.ToString(), this.SubnetMask.ToString());
            return result;
        }
    }

	/// <summary>
	/// Utility methods
	/// </summary>
	public static class NetUtility
	{
		private static Regex s_regIP;

		/// <summary>
		/// Get IP address from notation (xxx.xxx.xxx.xxx) or hostname
		/// </summary>
		public static IPAddress Resolve(string ipOrHost)
		{
			if (string.IsNullOrEmpty(ipOrHost))
				throw new ArgumentException("Supplied string must not be empty", "ipOrHost");

			ipOrHost = ipOrHost.Trim();

			if (s_regIP == null)
			{
				string expression = "\\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\b";
				RegexOptions options = RegexOptions.Compiled;
				s_regIP = new Regex(expression, options);
			}

			// is it an ip number string?
			IPAddress ipAddress = null;
			if (s_regIP.Match(ipOrHost).Success && IPAddress.TryParse(ipOrHost, out ipAddress))
				return ipAddress;

			// ok must be a host name
			IPHostEntry entry;
			try
			{
				entry = Dns.GetHostEntry(ipOrHost);
				if (entry == null)
					return null;

				// check each entry for a valid IP address
				foreach (IPAddress ipCurrent in entry.AddressList)
				{
					string sIP = ipCurrent.ToString();
					bool isIP = s_regIP.Match(sIP).Success && IPAddress.TryParse(sIP, out ipAddress);
					if (isIP)
						break;
				}
				if (ipAddress == null)
					return null;

				return ipAddress;
			}
			catch (SocketException ex)
			{
				if (ex.SocketErrorCode == SocketError.HostNotFound)
				{
					//LogWrite(string.Format(CultureInfo.InvariantCulture, "Failed to resolve host '{0}'.", ipOrHost));
					return null;
				}
				else
				{
					throw;
				}
			}
		}

		private static NetworkInterface GetNetworkInterface()
		{
			IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
			if (computerProperties == null)
				return null;

		    NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
			if (nics == null || nics.Length < 1)
				return null;

			foreach (NetworkInterface adapter in nics)
			{
				if (adapter.OperationalStatus != OperationalStatus.Up)
					continue;
				if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback || adapter.NetworkInterfaceType == NetworkInterfaceType.Unknown)
					continue;
				if (!adapter.Supports(NetworkInterfaceComponent.IPv4))
					continue;

               
				// A computer could have several adapters (more than one network card)
				// here but just return the first one for now...
				return adapter;
			}
			return null;
        }

        public static IPAddressWithMask FindFirstReachableEndpoint(IPAddress remote, IPAddressWithMask[] localEndPoints)
        {
            if (localEndPoints == null ) throw new ArgumentNullException ();
        
            for (int i = 0; i < localEndPoints.Length; i++)
                if (IsInSameSubnet(localEndPoints[i].Address, remote, localEndPoints[i].SubnetMask))
                    return localEndPoints[i];

            return null;
        }

        #region IPAddressExtensions  
        // by knom 
        // public domain
        // http://blogs.msdn.com/knom/archive/2008/12/31/ip-address-calculations-with-c-subnetmasks-networks.aspx
        public static IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }

        public static IPAddress GetNetworkAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
            }
            return new IPAddress(broadcastAddress);
        }

        public static bool IsInSameSubnet(IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            IPAddress network1 = GetNetworkAddress(address, subnetMask);
            IPAddress network2 = GetNetworkAddress(address2,subnetMask);

            return network1.Equals(network2);
        }

        #endregion

        //public static class SubnetMask
        //{
        #region SubnetMask
        // by knom 
        // public domain
        // http://blogs.msdn.com/knom/archive/2008/12/31/ip-address-calculations-with-c-subnetmasks-networks.aspx
        public static readonly IPAddress ClassA = IPAddress.Parse("255.0.0.0");
            public static readonly IPAddress ClassB = IPAddress.Parse("255.255.0.0");
            public static readonly IPAddress ClassC = IPAddress.Parse("255.255.255.0");

            public static IPAddress CreateByHostBitLength(int hostpartLength)
            {
                int hostPartLength = hostpartLength;
                int netPartLength = 32 - hostPartLength;

                if (netPartLength < 2)
                    throw new ArgumentException("Number of hosts is to large for IPv4");

                Byte[] binaryMask = new byte[4];

                for (int i = 0; i < 4; i++)
                {
                    if (i * 8 + 8 <= netPartLength)
                        binaryMask[i] = (byte)255;
                    else if (i * 8 > netPartLength)
                        binaryMask[i] = (byte)0;
                    else
                    {
                        int oneLength = netPartLength - i * 8;
                        string binaryDigit =
                            String.Empty.PadLeft(oneLength, '1').PadRight(8, '0');
                        binaryMask[i] = Convert.ToByte(binaryDigit, 2);
                    }
                }
                return new IPAddress(binaryMask);
            }

            public static IPAddress CreateByNetBitLength(int netpartLength)
            {
                int hostPartLength = 32 - netpartLength;
                return CreateByHostBitLength(hostPartLength);
            }

            public static IPAddress CreateByHostNumber(int numberOfHosts)
            {
                int maxNumber = numberOfHosts + 1;

                string b = Convert.ToString(maxNumber, 2);

                return CreateByHostBitLength(b.Length);
            }
            //}
        #endregion

            /// <summary>
		/// Gets my local IP address (not necessarily external) and subnet mask
		/// </summary>
		public static IPAddress GetMyAddress(out IPAddress mask)
		{
			NetworkInterface ni = GetNetworkInterface();
			if (ni == null)
			{
				mask = null;
				return null;
			}

			IPInterfaceProperties properties = ni.GetIPProperties();
			foreach (UnicastIPAddressInformation unicastAddress in properties.UnicastAddresses)
			{
				if (unicastAddress != null && unicastAddress.Address != null && unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
				{
					mask = unicastAddress.IPv4Mask;
                    Console.WriteLine(unicastAddress.Address.ToString());
                    return unicastAddress.Address;
				}
			}
            
            //foreach (IPAddress dhcpAddress in properties.DhcpServerAddresses)
            //{
            //    if (dhcpAddress != null && dhcpAddress.Address != null && dhcpAddress.AddressFamily  == AddressFamily.InterNetwork)
            //    {
            //        //mask = dhcpAddress.IPv4Mask;
            //        Console.WriteLine(dhcpAddress.ToString());

            //        //return unicastAddress.Address;
            //    }
            //}

            //foreach (IPAddress dnsAddress in properties.DnsAddresses)
            //{
            //    if (dnsAddress != null && dnsAddress.Address != null && dnsAddress.AddressFamily == AddressFamily.InterNetwork)
            //    {
            //        //mask = dnsAddress.;
            //        Console.WriteLine(dnsAddress.ToString());

            //        //return unicastAddress.Address;
            //    }
            //}

            //foreach (GatewayIPAddressInformation  gatewayAddress in properties.GatewayAddresses)
            //{
            //    if (gatewayAddress != null && gatewayAddress.Address != null && gatewayAddress.Address.AddressFamily  == AddressFamily.InterNetwork)
            //    {
            //        //mask = gatewayAddress.;
            //        Console.WriteLine(gatewayAddress.Address.ToString());

            //        //return gatewayAddress;
            //    }
            //}

            //foreach (MulticastIPAddressInformation multicastAddress in properties.MulticastAddresses)
            //{
            //    if (multicastAddress != null && multicastAddress.Address != null && multicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
            //    {
            //        //mask = multicastAddress.IPv4Mask;
            //        Console.WriteLine(multicastAddress.Address.ToString());

            //        //return unicastAddress.Address;
            //    }
            //}

			mask = null;
			return null;
		}

		/// <summary>
		/// Returns true if the IPEndPoint supplied is on the same subnet as this host
		/// </summary>
		public static bool IsLocal(IPEndPoint endPoint)
		{
			if (endPoint == null)
				return false;
			return IsLocal(endPoint.Address);
		}

		/// <summary>
		/// Returns true if the IPAddress supplied is on the same subnet as this host
		/// </summary>
		public static bool IsLocal(IPAddress remote)
		{
			IPAddress mask;
			IPAddress local = GetMyAddress(out mask);

			if (mask == null)
				return false;

			uint maskBits = BitConverter.ToUInt32(mask.GetAddressBytes(), 0);
			uint remoteBits = BitConverter.ToUInt32(remote.GetAddressBytes(), 0);
			uint localBits = BitConverter.ToUInt32(local.GetAddressBytes(), 0);

			// compare network portions
			return ((remoteBits & maskBits) == (localBits & maskBits));
		}

		/// <summary>
		/// Returns how many bits are necessary to hold a certain number
		/// </summary>
		[CLSCompliant(false)]
		public static int BitsToHoldUInt(uint value)
		{
			int bits = 1;
			while ((value >>= 1) != 0)
				bits++;
			return bits;
		}

		[CLSCompliant(false)]
		public static UInt32 SwapByteOrder(UInt32 value)
		{
			return
				((value & 0xff000000) >> 24) |
				((value & 0x00ff0000) >> 8) |
				((value & 0x0000ff00) << 8) |
				((value & 0x000000ff) << 24);
		}
		
		[CLSCompliant(false)]
		public static UInt64 SwapByteOrder(UInt64 value)
		{
			return
				((value & 0xff00000000000000L) >> 56) |
				((value & 0x00ff000000000000L) >> 40) |
				((value & 0x0000ff0000000000L) >> 24) |
				((value & 0x000000ff00000000L) >> 8) |
				((value & 0x00000000ff000000L) << 8) |
				((value & 0x0000000000ff0000L) << 24) |
				((value & 0x000000000000ff00L) << 40) |
				((value & 0x00000000000000ffL) << 56);
		}

		public static bool CompareElements(byte[] one, byte[] two)
		{
			if (one.Length != two.Length)
				return false;
			for (int i = 0; i < one.Length; i++)
				if (one[i] != two[i])
					return false;
			return true;
		}
	}
}
