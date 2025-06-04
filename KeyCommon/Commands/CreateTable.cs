using System;
using Lidgren.Network;

namespace KeyCommon.Commands
{
    //public class CreateTable : NetCommandBase
    //{

    //    public KeyCommon.Entities.GameConfig Game;
    //    public byte[] Data;

    //    public CreateTable()
    //    {
    //        mCommand = (int)Enumerations.Types.CreateTable;
    //    }

    //    #region IRemotableType Members

    //    public override void Read(NetBuffer buffer)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void Write(NetBuffer buffer)
    //    {
    //        throw new NotImplementedException();
    //    }
    //    #endregion

    ////    Public mGame As Game
    ////Public mData As Byte()    ' when clients receive a CreateGame message, mData will contain an authentication Reply packet (Reply packets contain a SessionKey + Ticket).  


    ////Public Sub New()
    ////    mGame = New Game()
    ////    mGame.mHost = New Host(Nothing)
    ////    mGame.mTurnSchedule = New GameTurnSchedule
    ////End Sub

    ////Public ReadOnly Property ID() As Integer Implements IRemotableType.ID
    ////    Get
    ////        Return Enumerations.CreateGame
    ////    End Get
    ////End Property

    ////Public ReadOnly Property Channel() As NetChannel Implements IRemotableType.Channel
    ////    Get
    ////        Return NetChannel.ReliableUnordered
    ////    End Get
    ////End Property

    ////Public Sub Read(ByVal buffer As NetBuffer) Implements IRemotableType.Read
    ////    mGame.Read(buffer)
    ////    Dim dataLength As Integer = buffer.ReadInt32()
    ////    If (dataLength > 0) Then
    ////        ReDim mData(dataLength - 1)
    ////        mData = buffer.ReadBytes(dataLength)
    ////    End If
    ////End Sub

    ////Public Sub Write(ByVal buffer As NetBuffer) Implements IRemotableType.Write
    ////    mGame.Write(buffer)

    ////    Dim dataLength As Integer = 0
    ////    If (mData IsNot Nothing) Then dataLength = mData.Length
    ////    buffer.Write(dataLength)
    ////    If (dataLength > 0) Then
    ////        buffer.Write(mData)
    ////    End If
    ////End Sub
    //}
}
