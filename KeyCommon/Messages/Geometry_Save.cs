using System;
using Lidgren.Network;
using KeyCommon.Messages;


namespace KeyCommon.Messages
{
    /// <summary>
    /// Typically initiated when a user drags a tree branch onto a location in the Gallery.
    /// Or when they click "Save Current Node as Prefab" gallery button.
    /// Additionally, on plugin Right Mouse Click menu to save as prefab, we can popup
    /// a mini custom treeview with leaf nodes and allow the user to browse and select 
    /// as well as perhaps select the current mod to store it in as well.  In any case, this command
    /// is unchanged.
    /// </summary>
    public class Geometry_Save : MessageBase
    {
        public string CurrentRelativePath;
        public string NewRelativePath;
        

        public Geometry_Save()
            : base((int)Enumerations.GeometrySave)
        { }



        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            CurrentRelativePath = buffer.ReadString();
            NewRelativePath = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(CurrentRelativePath);
            buffer.Write(NewRelativePath);
        }
        #endregion
    }


}
