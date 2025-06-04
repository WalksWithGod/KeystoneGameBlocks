using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Net.Sockets;

namespace NetUtils
{

    //public class NetworkEnumerator
    //{
    //    [DllImport("Ws2", EntryPoint = "WSAIoctl", SetLastError = true)] 
    //    static extern Int32 WSAIoctl(IntPtr socket, int dwControlCode, 
    //    Int32 lpvInBuffer, 
    //    Int32 cbInBuffer,  
    //    IntPtr lpvOutBuffer,
    //    Int32 cbOutBuffer, out UInt32 lpcbBytesReturned, IntPtr lpOverlapped, IntPtr lpCompletionRoutine); 
        
    //    //private int Function WSAIoctl Lib "ws2_32.dll" (ByVal s As Integer, ByVal dwIoControlCode As Integer, _
    //    //    lpvInBuffer As Integer, ByVal cbInBuffer As Integer, ByVal lpOutBuffer As IntPtr, ByVal cbOutBuffer As Integer, _
    //    //    ByRef lpcbBytesReturned As Integer, ByRef lpOverlapped As Integer, ByVal lpCompletionRoutine As Integer) As Integer
        
    //    [DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]static extern Int32 WSAGetLastError();
    //    [DllImport("kernel32.dll", SetLastError=true)] static extern int lstrcpyA ( string lpString1, string lpString2) ;
    //    [DllImport("kernel32.dll", CharSet = CharSet.Auto, EntryPoint = lstrcpyA, BestFitMapping = false)]
    //    internal static extern IntPtr lstrcpy(IntPtr dst, String src);

       

    //    //private Declare Function lstrcpy Lib "kernel32" Alias "lstrcpyA" (ByVal lpString1 As String, ByVal lpString2 As Integer) As Integer

    //    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, EntryPoint = LSTRLENA)]
    //    internal static extern int lstrlenA(IntPtr ptr);
    //        //private Declare Function lstrlen Lib "kernel32" Alias "lstrlenA" (ByVal lpString As Integer) As Integer
    //    [DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]static extern Int32 inet_ntoa(Int32 inn);
    //    //private Declare Function inet_ntoa Lib "ws2_32.dll" (ByVal inn As Integer) As Integer

    //    private const short SOCKET_ERROR = -1;
    //    private const int SIO_GET_INTERFACE_LIST = 0x4004747f;
    //    private const short IFF_UP = 0x1; ///* Interface is up */
    //    private const short IFF_BROADCAST = 0x2; // Broadcast is supported
    //    private const short IFF_LOOPBACK = 0x4; // this is loopback interface
    //    private const short IFF_POINTTOPOINT = 0x8; // this is point-to-point interface
    //    private const short IFF_MULTICAST = 0x10; // multicast is supported */


    //    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    //    public struct sockaddr_in
    //    {
    //        public short sin_family;
    //        public short sin_port;
    //        public int sin_addr;
    //        public byte[] sin_zero;

    //        public void Initialize()
    //        {
    //            Int32 i = default(Int32);
    //           sin_zero = new byte[8];
    //            for (i = 0; i <= sin_zero.GetUpperBound(0); i++)
    //            {
    //                sin_zero[i] = 0;
    //            }
    //        }
    //    }


    //    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    //    public struct sockaddr_gen
    //    {
    //        // 24 bytes total
    //        public sockaddr_in AddressIn;
    //        public byte[] filler;

    //        public void Initialize()
    //        {
    //            Int32 i = default(Int32);
    //            AddressIn.Initialize();
    //             filler = new byte[8]; // we need to pad with 8 bytes since we can't create an instrinsic 'union'

    //            // we need to pad with 8 bytes since we can't create an instrinsic 'union'
    //            for (i = 0; i <= filler.GetUpperBound(0); i++)
    //            {
    //                filler[i] = 0;
    //            }
    //        }
    //    }


    //    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    //    public struct INTERFACE_INFO
    //    {
    //        // 76 bytes total
    //        public int iiFlags;
    //        // 4 bytes
    //        public sockaddr_gen iiAddress;
    //        // 24 bytes 
    //        public sockaddr_gen iiBroadcastAddress;
    //        // 24 bytes
    //        public sockaddr_gen iiNetmask;
    //        // 24 bytes

    //        public void Initialize()
    //        {
    //            iiAddress.Initialize();
    //            iiBroadcastAddress.Initialize();
    //            iiNetmask.Initialize();
    //        }
    //    }

    //    private INTERFACE_INFO[] m_Interfaces;

    //    //#region "Properties"
    //    //public Int32 Count { get { Count = m_Interfaces.Length; }        }
    //    //public string HostIP {get { return StringFromPointer(inet_ntoa(m_Interfaces[index].iiAddress.AddressIn.sin_addr)); }        }
    //    //public string BroadCastIP { get { return StringFromPointer(inet_ntoa(m_Interfaces[index].iiBroadcastAddress.AddressIn.sin_addr)); }        }
    //    //public string NetMask {get { return StringFromPointer(inet_ntoa(m_Interfaces[index].iiNetmask.AddressIn.sin_addr)); }        }
    //    //public bool IsActive { get { return m_Interfaces[index].iiFlags & IFF_UP; }        }
    //    //public bool IsPoint2Point {get { return m_Interfaces[index].iiFlags & IFF_POINTTOPOINT; }        }
    //    //public bool IsLoopBack {get { return m_Interfaces[index].iiFlags & IFF_LOOPBACK; }}
    //    //public bool IsBroadcast {get { return m_Interfaces[index].iiFlags & IFF_BROADCAST; }        }
    //    //public bool IsMulticast {get { return m_Interfaces[index].iiFlags & IFF_MULTICAST; }        }
    //    //#endregion

    //    public NetworkEnumerator()
    //    {
    //        UInt32 lSizeOfInterface = default(Int32);
    //        Int32 lOffset = default(Int32);
    //        UInt32 lNumInterfacesFound = default(Int32);
    //        UInt32 nBytesReturned = default(UInt32);
    //        Int32 lTotalBytes = default(Int32);
    //        Int32 lErr = default(Int32);
    //        Int32 i = default(Int32);
    //        //Dim saiAddress As sockaddr_in
    //        IntPtr ptr = default(IntPtr);
            
    //        const Int32 MAX_INTERFACES = 10;
    //        // feel free to decrease / increase this.  I can't imagine a machine having more than 10 network adapters
            
    //        // todo: need to learn structured exception handling.  I will fix this up soon
    //        try {
    //            //m_Interfaces = new INTERFACE_INFO[MAX_INTERFACES];
    //            //lSizeOfInterface = Marshal.SizeOf(m_Interfaces(0)) ' why don't these report full size 76 bytes?  
    //            //lSizeOfInterface = Len(InterfaceList(0))
    //            lSizeOfInterface = 76;
    //            // apparently our struct is only coming out to a size of 52 bytes... 24 short.  Why? For now we hardcode the size
    //            //WriteLine("Size of Interface = " & lSizeOfInterface)
    //            lTotalBytes = (int)lSizeOfInterface * MAX_INTERFACES;
    //            ptr = Memory.CreateBuffer(lTotalBytes);
                
    //            Socket oSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);


    //            if (WSAIoctl(oSocket.Handle, SIO_GET_INTERFACE_LIST, 0, 0, ptr, lTotalBytes, out nBytesReturned, IntPtr.Zero, IntPtr.Zero) == SOCKET_ERROR)
    //            {
    //                lErr = WSAGetLastError();
    //                Exception ex = new Exception();
    //                throw ex;
    //            }
    //            else 
    //            {
    //                lNumInterfacesFound = nBytesReturned / lSizeOfInterface;
    //                 m_Interfaces = new INTERFACE_INFO[lNumInterfacesFound];

                    
    //                for (i = 0; i <= lNumInterfacesFound - 1; i++) {
    //                    lOffset = i * lSizeOfInterface;
    //                    // todo: would be nice to remove these hard coded offsets, but they are accurate.
    //                    {
    //                        m_Interfaces[i].iiAddress.AddressIn.sin_addr = (Marshal.ReadInt32(ptr, 8 + lOffset));
    //                        m_Interfaces[i].iiBroadcastAddress.AddressIn.sin_addr = Marshal.ReadInt32(ptr, 32 + lOffset);
    //                        m_Interfaces[i].iiNetmask.AddressIn.sin_addr = Marshal.ReadInt32(ptr, 56 + lOffset);
    //                        m_Interfaces[i].iiFlags = Marshal.ReadInt32(ptr, 0 + lOffset);
    //                    }
    //                }
    //            }
    //        }
    //        catch (Exception ex) 
    //        {
    //            System.Diagnostics.Trace.WriteLine(ex.Message);
    //        }
    //        finally {
    //            Marshal.FreeHGlobal(ptr);
    //        }
    //    }

    //    public static string StringFromPointer(int lPointer)
    //    {
    //        string strTemp = null;
    //        Int32 iRet = default(Int32);
            
    //        strTemp = new string(Strings.Chr(0), lstrlen(lPointer));
    //        iRet = lstrcpy(strTemp, lPointer);
            
    //        if (iRet) 
    //            return strTemp;
    //        else return "";
    //    }

    //}
}
