using System;
using Lidgren.Network;
using KeyCommon;
using System.Collections.Generic;
using KeyCommon.Commands;

namespace KeyCommon.DatabaseEntities
{
    public struct GameConfigParameter
    {
        public GameConfigParameterType ID;
        public object[] Args;

        public void Read(Lidgren.Network.NetBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public void Write(Lidgren.Network.NetBuffer buffer)
        {
            throw new NotImplementedException();
        }
    }

    public struct GameTurnSchedule
    {
        public GameScheduleType Schedule;
        public bool UseList;
        // if true, does not use Interval but rather the list
        public int Interval;


        public DateTime[] Times;
        //TODO: what to do if the server runs, and the turn takes longer to compute than the interval between turns?
        //public int Duration;
        //can either be a count(e.g. 10 turns a day running on an every 15 minute shcedule..)
        //Public Occurance As Occurance ' e.g. the function of occurance depends on the selected Schedule.
        // for instance, if minutely, then our occurance could  every day, every week, every other day, M,W,F, T

        public void Read(NetBuffer buffer)
        {

            try
            {
                Schedule = (GameScheduleType)buffer.ReadInt32();
                UseList = buffer.ReadBoolean();
                Interval = buffer.ReadInt32();
                int count = buffer.ReadInt32();
                if ((count > 0))
                {
                //ERROR: Not supported in C#: ReDimStatement

                    for (int i = 0; i < count; i++)
                    {
                        Times[i] = new DateTime(buffer.ReadInt64());
                    }
                }

               // Duration = buffer.ReadInt32();
            }
            catch
            {

            }
        //TODO: malformed packet could not be read.
        }

        public void Write(NetBuffer buffer)
        {
            if ((buffer == null)) return;

            try
            {
                buffer.Write((byte)Schedule);
                buffer.Write(UseList);
                buffer.Write(Interval);
                int count = 0;
                if ((Times != null && Times.Length > 0))
                {
                    count = Times.Length;
                }
                buffer.Write(count);
               // buffer.Write(Duration);
            }
            catch
            {

            //TODO: buffer write error
                return;
            }
            return;
        }
    }


    //a "table" and a "game" are now synonymous however
    // a table can be canceled at the registering stage in the lobby
    // WAIT:  Maybe not...maybe Game is Game and GameRegistration is what we called a "Table" before

    // match, campaign
    // registrationRequired?
    // status = registering, playing, ended
    // password
     
    // - two types of games, persistant games that only show up in the list if
    //   registration = false(no registration required, can join as long as open player slots)
    // - non persistant games (matches)

    public class Game : GameObject
    {

        public bool Registered = false;
        public string mName;      // friendly name of this game
        public string mPassword; // password to join the game, not the account password
        public string Map; // scene foldername?

        Host mHost;
        //TODO: need to modify "Host"
        // to explicitly be a per game object where each game on a multiple game server, uses a seperate Host since
        // authentication requires different accounts and we'd use different listening ports whcih means we'd use an array
        // of Lidgren.Network.NetServer[] mNetServer.
        // but for now this is not too important since for alpha we just have one game per server



        public Game(int id) : base (id)
        {
            // TODO: i think id here represents a db record index not the typeID of GameServerInfo
        }

        public Game(string name) : this((int)KeyCommon.Messages.Enumerations.GameServerInfo)
        {
            mName = name;
        }

        public Game(Host host) : this((int)KeyCommon.Messages.Enumerations.GameServerInfo)
        {
            if (host == null) throw new ArgumentNullException();
            mHost = host;
        }

        public GameSummary GetSummary()
        {
            GameSummary sum = new GameSummary();
            sum.Name = mName; // so a lookup can be done by the lobby to find the correct server.  if we pass a PrimaryKey id instead we can more directly query the dictionary of games
            sum.ServerName = mHost.Name;
            sum.PasswordProtected = !string.IsNullOrEmpty(mPassword);
            sum.Map = Map;
            sum.ListenTable = mHost.ListenTable;
            return sum;
        }

        public Host Host { get { return mHost; } }
        public long UserID { get { throw new NotImplementedException(); } }









       //// temporary concepts
       // public void Tick(long elapsed)
       // {
       //     int startTime =

       //         //update each AI entity for the provided time slice(this is to prevent us from doing nothing but AI updates for some undetermined interval

       //        // do AI stuff

       //       Do while elapsed < mAITimeSlice

       //             entity = mAI.GetNext()
       //            entity.Update()
       //            elapsed = Environment.TickCount() - startTime

       //         loop

       //      update positions of all entities

       //      do other stuff required in the simulation

       //     mGame.mTurn += 1;
       //     mSimulationTick = GetPerformanceCounter - iStartTime  ' how long it took to update the simulation.  We can even take averages over time.  
       // }

       // //   this will be based on how long each SimulationTick Requires to run
       // private void CalcAITimeSlice(ByVal iPercentage As Integer)
       // {
       //     mAITimeSlice = 50;
       // }





        public NetChannel Channel
        {
            get { return NetChannel.ReliableUnordered; }
        }


        public override void Read(NetBuffer buffer)
        {
            mID = buffer.ReadInt64();
            mName = buffer.ReadString();

            mHost = new Host();
            //mHost.Read(buffer);


            Map = buffer.ReadString();
            mPassword = buffer.ReadString();  // not the authentication password but a local password so only invited players can play this game

            
        }

        public override void Write(NetBuffer buffer)
        {
            buffer.Write(mID);
            buffer.Write(mName);

           // mHost.Write(buffer);

            buffer.Write(Map);
            buffer.Write(mPassword);
            
        }
    }

    //public class OldGame : GameObject
    //{

    //    public string mName;
    //    public string mServerName;  // if an end user is hosting this game this is their user's name, else it's the name of one our own game servers' account

    //    // not a typeo, this is in fact supposed to be a Long
    //    public string mPassword;
    //    public string mVersion;
    //    public DateTime mStart;
    //    public DateTime mEnd;


    //    public Host mHost;

    //    public GameType mType;
    //    public GameConfigParameter[] mParameters;  // can include time_step and other params? client and server need to run at same frequency
    //    // shouldn't we want to persist this?
    //    public GameTurnSchedule mTurnSchedule;
    //    public GameStatus mStatus;
    //    public GameResolution mResolution;




    //    public OldGame()
    //    {

    //    }


    //    public OldGame(Table table, string password)
    //    {
    //        mPassword = password;

    //        // we use -1 when creating a game from a table since we dont want the Table's id, the table's id is only relevant in the context of the Lobby.  We 
    //        // instead get the game ID from the database itself since game ID's use a SERIAL value so that every single game ever created is unique
    //        // this way we can always reference the game a user was in for admin purposes and log tracking since every game is unique.
    //        // Anyway, using a -1 will instruct the SQLContext.Store() to use the DEFAULT keyword for this type

    //        mName = table.Name;

    //        //mHost.IP = 
    //        //mHost.Port = 
    //        //mHost.UpTime <-- this field should be changed to just .StartTime  for when it connect to the Lobby 
    //        // mHost.UsesNat 
    //        // mType As GameType
    //        // mVersion As String  <-- TODO: this should be moved to the Host yes?  it's the Host's exe version
    //        // 

    //        mStart = DateTime.Now;
    //        mStatus = GameStatus.Registering;
    //        mResolution = GameResolution.Unresolved;

    //        mParameters = table.Settings.ToArray();

    //    }

    //    public void GetSummary()
    //    {

    //    }


    //    public int ID
    //    {
    //        get { return (int)Enumerations.Game; } // TODO: no, not creategame, but should just be a "game" entity 
    //    }

    //    public NetChannel Channel
    //    {
    //        get { return NetChannel.ReliableUnordered; }
    //    }

    //    public override void Read(NetBuffer buffer)
    //    {
    //        mPrimaryKey = buffer.ReadInt64();
    //        mName = buffer.ReadString();
    //        mServerName = buffer.ReadString();
    //        mPassword = buffer.ReadString();
    //        mType = (GameType)buffer.ReadInt32();
    //        mVersion = buffer.ReadString();
    //        mHost = new Host();
    //        mHost.Read(buffer);
    //        // mParameters ' first mParameters.Count
    //        // mTurnSchedule
    //        mStatus = (GameStatus)buffer.ReadByte();
    //        mResolution = (GameResolution)buffer.ReadByte();

    //        mStart = new DateTime(buffer.ReadInt64());

    //        mEnd = new DateTime(buffer.ReadInt64());
    //    }

    //    public override void Write(NetBuffer buffer)
    //    {
    //        buffer.Write(mPrimaryKey);
    //        buffer.Write(mName);
    //        buffer.Write(mServerName);
    //        buffer.Write(mPassword);
    //        buffer.Write((byte)mType);
    //        buffer.Write(mVersion);
    //        mHost.Write(buffer);
    //        // buffer.Write parametersCount
    //        // for each param parameter.Write(buffer)
    //        // mTurnSchedule.Write(buffer
    //        buffer.Write((byte)mStatus);
    //        buffer.Write((byte)mResolution);

    //        buffer.Write(mStart.Ticks);


    //        buffer.Write(mEnd.Ticks);
    //    }
    //}
}