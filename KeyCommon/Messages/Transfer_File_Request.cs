using System;
using System.Collections.Generic;
using KeyCommon.Messages;
using Lidgren.Network;
using KeyCommon.Helpers;

namespace KeyCommon.Messages
{
    // todo: im not sure this is needed or desired.  The server should be performing Area of Interest management
    // and be the one sending Transfer_File_Begin commands.
    public class Transfer_File_Request : MessageBase
    {
        public string mRelativeFilePath;
        public Transfer_File_Request() : base ((int)Enumerations.FileTransferRequest)
        {

        }



    }
}
