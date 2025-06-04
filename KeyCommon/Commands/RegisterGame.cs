using System;
using Lidgren.Network;


namespace KeyCommon.Commands
{
    //public class RegisterGame : NetCommandBase 
    //{
    //    private KeyCommon.Entities.GameConfig mGameConfig;


    //    public RegisterGame()
    //    {
    //        mCommand = (int)Enumerations.Types.RegisterGame;
    //    }

    //    public RegisterGame(GameConfig gameConfig) : this()
    //    {
    //        if (gameConfig == null) throw new ArgumentNullException();
    //        mGameConfig = gameConfig;
    //    }

    //    public KeyCommon.Entities.GameConfig Game { get { return mGameConfig; } }

    //    #region IRemotableType Members
    //    public override void Read(NetBuffer buffer)
    //    {
    //        mGameConfig = new KeyCommon.Entities.GameConfig ();
    //        mGameConfig.Read(buffer);
    //    }

    //    public override void Write(NetBuffer buffer)
    //    {
    //        mGameConfig.Write(buffer);
    //    }
    //    #endregion
    //}
}
