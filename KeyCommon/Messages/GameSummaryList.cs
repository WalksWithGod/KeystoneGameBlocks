using System;
using Lidgren.Network;
using KeyCommon.DatabaseEntities;

namespace KeyCommon.Messages
{
    public class GameSummaryList : MessageBase
    {
        public GameSummary[] List;

        public GameSummaryList()
            : base ((int)Enumerations.GameSummaryList)
        {
        }

        public void AddSummary(GameSummary summary)
        {
            int count = 0;
            if (List != null) 
                count = List.Length;
           
            count++;
            GameSummary[] newList = new GameSummary[count];

            if (List != null)
                List.CopyTo(newList, 0);

            int index = count - 1;
            newList[index] = summary;
            List = newList;
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            int count = buffer.ReadInt32();

            if (count > 0)
            {
                List = new GameSummary[count];
                for (int i = 0; i < count; i++)
                {
                    GameSummary summary = new GameSummary();
                    summary.Read(buffer);
                    List[i] = summary;
                }
            }
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            int count = 0;
            if (List != null)
                count = List.Length;

            buffer.Write(count);
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                    List[i].Write(buffer);
            }
        }
        #endregion
    }
}
