using System;
using System.Collections.Generic;

namespace Lidgren.Network
{
    internal class NetBufferPool
    {
        public NetBuffer CheckOut()
        {
            return CheckOut(4);
        }

        /// <summary>
        /// CheckOut's a user created buffer so that it can be managed by the pool
        /// </summary>
        /// <param name="buffer"></param>
        internal static void CheckOut(NetBuffer buffer)
        {
            
        }

        internal static NetBuffer CheckOut(string data)
        {
            NetBuffer buffer = CheckOut(System.Text.Encoding.UTF8.GetByteCount(data) + 1);
            buffer.Write(data); // todo: why isnt the buffer.m_bitLength set here?  Shouldnt it be?
            return buffer;
        }

        internal static NetBuffer CheckOut(byte[] dataToCopy)
        {
            if (dataToCopy != null)
            {
                NetBuffer buffer = CheckOut(dataToCopy.Length);
                Buffer.BlockCopy(dataToCopy, 0, buffer.Data, 0, dataToCopy.Length);
                buffer.m_bitLength = dataToCopy.Length * 8;
                return buffer;
            }
            else
                return CheckOut(4);
        }

        /// <summary>
        /// Primary CheckOut method which handles finding an existing buffer or creation of new ones
        /// </summary>
        /// <param name="capacity"></param>
        /// <returns></returns>
        internal static NetBuffer CheckOut(int capacityBytes)
        {

            //// can we find an existing?

            //// else create
            //NetBuffer buffer = new NetBuffer();

            //if (capacityBytes < 0)
            //    capacityBytes = 4;

            //buffer.Data = new byte[capacityBytes];

            //// add to 
            //return buffer;
            return null;
        }


        internal static void CheckIn(NetBuffer buffer)
        {

        }
    }
}

//// from MSDN - a buffer pool for sockets.  Below is actually pretty good for sockets 
//// but it's not really suitable for message buffers where the size is more abritrary.  Although
//// eventually we may try to remove the use of an intermediary IncomingMessage buffer
//// and instead have data in the socket buffers be written directly by the IRemotableType to its own properties

//// http://msdn.microsoft.com/en-us/library/bb517542.aspx
//C#

//// This class creates a single large buffer which can be divided up 
//// and assigned to SocketAsyncEventArgs objects for use with each 
//// socket I/O operation.  
//// This enables bufffers to be easily reused and guards against 
//// fragmenting heap memory.
//// 
//// The operations exposed on the BufferManager class are not thread safe.
//class BufferManager
//{
//    int m_numBytes;                 // the total number of bytes controlled by the buffer pool
//    byte[] m_buffer;                // the underlying byte array maintained by the Buffer Manager
//    Stack<int> m_freeIndexPool;     // 
//    int m_currentIndex;
//    int m_bufferSize;

//    public BufferManager(int totalBytes, int bufferSize)
//    {
//        m_numBytes = totalBytes;
//        m_currentIndex = 0;
//        m_bufferSize = bufferSize;
//        m_freeIndexPool = new Stack<int>();
//    }

//    // Allocates buffer space used by the buffer pool
//    public void InitBuffer()
//    {
//        // create one big large buffer and divide that 
//        // out to each SocketAsyncEventArg object
//        m_buffer = new byte[m_numBytes];
//    }

//    // Assigns a buffer from the buffer pool to the 
//    // specified SocketAsyncEventArgs object
//    //
//    // <returns>true if the buffer was successfully set, else false</returns>
//    public bool SetBuffer(SocketAsyncEventArgs args)
//    {

//        if (m_freeIndexPool.Count > 0)
//        {
//            args.SetBuffer(m_buffer, m_freeIndexPool.Pop(), m_bufferSize);
//        }
//        else
//        {
//            if ((m_numBytes - m_bufferSize) < m_currentIndex)
//            {
//                return false;
//            }
//            args.SetBuffer(m_buffer, m_currentIndex, m_bufferSize);
//            m_currentIndex += m_bufferSize;
//        }
//        return true;
//    }

//    // Removes the buffer from a SocketAsyncEventArg object.  
//    // This frees the buffer back to the buffer pool
//    public void FreeBuffer(SocketAsyncEventArgs args)
//    {
//        m_freeIndexPool.Push(args.Offset);
//        args.SetBuffer(null, 0, 0);
//    }

//}