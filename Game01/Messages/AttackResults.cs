using System;
using System.Collections.Generic;
using System.Linq;
using KeyCommon.Messages;
using Lidgren.Network;

namespace Game01.Messages
{
    public class AttackResults : MessageBase
    {
        public struct Result
        {
            public string EntityID;
            public int Damage;
        }

        public string WeaponID;
        public bool Malfunction; // a malfunction is automatically a hit = false, but can also cause damage to the weapon that is firing.  That can result in a damage Result[] element that points to the weapon Entity
        public bool Hit; // atttack misses if false
        public double DistanceToTarget;
        public Result[] Results; // todo: we may need multiple damaged Entities such as the exterior hull/structure and any interior components.  This could be an array of struct Damag {public string EntityID; public int Amount;
        // public int Award; // xp awarded to operator?

        // todo: for attackresults between NPCs, we could make tricorders (portable sensors and computers) behave like stations with an operator associated with them
        public AttackResults() : base ((int)Enums.UserMessage.Game_AttackResults)
        {
            // todo: how does this fit in with our production/consumption model of damage?

            // todo: even microwaves production and consumption should be based on rules.

            // todo: what about heat from proximity to a star causing hull damage?  well, some of these production/consumption functions should be done in server-side script.  but how do we have seperate scripts for any single entity... one for server and one for client?
        }


        public void AddResult(string entityID, int damage)
        {
            Result r = new Result();
            r.EntityID = entityID;
            r.Damage = damage;
            Results = Keystone.Extensions.ArrayExtensions.ArrayAppend(Results, r);
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);

            Malfunction = buffer.ReadBoolean();
            Hit = buffer.ReadBoolean();
            if (Hit)
            {
                WeaponID = buffer.ReadString();
                DistanceToTarget = buffer.ReadDouble();
                int count = buffer.ReadInt32();
                Results = new Result[count];
                for (int i = 0; i < count; i++)
                {
                    Results[i].EntityID = buffer.ReadString();
                    Results[i].Damage = buffer.ReadInt32();
                }
                
                
                //StationID = buffer.ReadString();
                //WeaponID = buffer.ReadString();
                //TargetID = buffer.ReadString();

     

            }

        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);

            buffer.Write(Malfunction);
            buffer.Write(Hit);
            if (Hit)
            {
                buffer.Write(WeaponID);
                buffer.Write(DistanceToTarget);
                buffer.Write(Results.Length);
                for (int i = 0; i < Results.Length; i++)
                {
                    buffer.Write(Results[i].EntityID);
                    buffer.Write(Results[i].Damage);
                }
               
                //buffer.Write(StationID);
                //buffer.Write(WeaponID);
                //buffer.Write(TargetID);



            }
        }
        #endregion



    }
}
