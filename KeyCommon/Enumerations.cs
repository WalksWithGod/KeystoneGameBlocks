using System;
using System.Collections.Generic;

namespace KeyCommon
{



        // for table lists and such, i think since it does include a Table type
        // we shoudl instead have an EntityList command type where
        // internally these entities are Entity.Write(buffer)  
        // and then when handling the entities, the EntityList has the type
        // specified in it... so its all entities of the same type OR the entity list
        // is delimited by an entity type id

        // scope is used for both userlist (lobby vs table) and chat.  I think 
        // we should have seperate scope enums for chat or actually, seperate commands
        // entirely for when a user has joined a table vs a lobby ?  Or 
        // at least, if the scope is specified, it shoudl also then be followed by a targetID
        // so if joining the entire lobby, scope= global and targetID = -1
        // if a table, scope = Local, targetID = tableID
        public enum Scope
        {
            Global,    // lobby wide or in the case of an admin message, game server wide
            Local,    // or table
            Organization,     // person to guild/team/clan/etc
            Private   // person to person 
        }


        public enum GameType : byte
        {
           Campaign,
           InstantAction_Persistant,
            InstantAction_Match
        }

        public enum MasterServerFilters
        {
            NONE, 	           //   All servers.
            FULL,               // 	Full servers.
            NOTFULL,         // 	Servers that aren't full.
            EMPTY,             // 	Empty servers.
            NOTEMPTY,       // 	Servers that aren't empty.
            PASSWORD,      // 	Servers that require a password.
            NOPASSWORD,  // 	Servers that don't require a password. 
        }


        public enum GameScheduleType : byte
        {
            Minutely,
            Hourly,
            Daily,
            Weekly,
            Monthly,
            Yearly
        }

        public enum GameStatus : byte
        {
            Registering,
            InProgress,
            Completed
        }

        public enum GameResolution : byte
        {
            Unresolved,
            Expired,
            Abandoned,
            Terminated,
            // by admin
            Finished
        }

        public enum GameFlags : byte
        {
            Open,
            Closed,
            PasswordProtected,
            Admin,        // visible only to users (only sent to admin level clients) with admin priveledges (primarily for testing)
            InProgress,
            Ended
        }

        // certain filters if set per connection can decrease the amount of data a server needs to send
        public enum GameFilters : byte
        {
            None,     //   All servers.
            Full,     // 	Full servers.
            NotFull,     // 	Servers that aren't full.
            Empty,     // 	Empty servers.
            NotEmpty,     // 	Servers that aren't empty.
            Password,     // 	Servers that require a password.
            NoPassword     // 	Servers that don't require a password. 
        }

        public enum GameConfigParameterType : byte
        {
            GameType,
            NumPlayers,
            NumAI,
            StartingUnits,
            StartingResources,
            VictoryCondition,
            EliminationCondition
        }
}
