using Game01.GameObjects;
using Lidgren.Network;
using System;
using System.Collections.Generic;


namespace Game01
{
    public class Game
    {
        private ulong mTick;
        private int mSeed;
        private const ushort TYPE_SENSOR_CONTACT_ARRAY = 1001;



        public Game(int seed)
        {
            KeyCommon.Helpers.ExtensionMethods.mUserTypeReader = new KeyCommon.Helpers.ExtensionMethods.ReadUserTypeDelegate(UserTypeReader);
            KeyCommon.Helpers.ExtensionMethods.mUserTypeWriter = new KeyCommon.Helpers.ExtensionMethods.WriteUserTypeDelegate(UserTypeWriter);
            KeyCommon.Helpers.ExtensionMethods.mUserTypeIDFromTypename = new KeyCommon.Helpers.ExtensionMethods.GetUserTypeIDFromTypename(UserTypeIDFromTypename);
            KeyCommon.Helpers.ExtensionMethods.mUserTypenameFromTypeID = new KeyCommon.Helpers.ExtensionMethods.GetUserTypeNameFromTypeID(UserTypenameFromTypeID);
            KeyCommon.Helpers.ExtensionMethods.mUserArrayElementsMerge = new KeyCommon.Helpers.ExtensionMethods.UserArrrayElementsMerge(MergeArrayElementsForUserTypes);


            mTick = 0;
            mSeed = seed;

        }

        

        private static ushort UserTypeIDFromTypename(string typename)
        {
            switch (typename)
            {
                case "SensorContact[]":
                    return TYPE_SENSOR_CONTACT_ARRAY;
   
                case "NavPoint[]":
                case "Path[]":
                case "UserData":
                case "UserData[]":
                case "Waypoint[]":

                default:
                    return 0;
            }
        }

        private static string UserTypenameFromTypeID(ushort typeID)
        {
            switch (typeID)
            {
                case TYPE_SENSOR_CONTACT_ARRAY: return "SensorContact[]";
                default:
                    return null;
            }
        }

        private static object UserTypeReader(Lidgren.Network.NetBuffer buffer, ushort typeID, out string typeName)
        {
            switch (typeID)
            {
                case TYPE_SENSOR_CONTACT_ARRAY:
                    typeName = UserTypenameFromTypeID(TYPE_SENSOR_CONTACT_ARRAY);
                    int count = buffer.ReadInt32();
                    SensorContact[] contacts = new SensorContact[count];
                    for (int i = 0; i < count; i++)
                    {
                        contacts[i].ContactID = buffer.ReadString();
                        contacts[i].Position.x = buffer.ReadDouble();
                        contacts[i].Position.y = buffer.ReadDouble();
                        contacts[i].Position.z = buffer.ReadDouble();
                        contacts[i].Velocity.x = buffer.ReadDouble();
                        contacts[i].Velocity.y = buffer.ReadDouble();
                        contacts[i].Velocity.z = buffer.ReadDouble();
                        contacts[i].IFF = buffer.ReadInt32();            // identify friend or foe. This could be an Enum FRIEND, FOE, UNKNOWN
                        contacts[i].IsTarget = buffer.ReadBoolean ();
                        contacts[i].IsGhost = buffer.ReadBoolean();       // occurs when the contact moves out of sensor range, or has used evasive maneuvers sufficiently, or has cloaked
                        contacts[i].Age = buffer.ReadInt32 ();
                        contacts[i].GhostAge = buffer.ReadInt32();
                        contacts[i].Priority = buffer.ReadInt32();
                        contacts[i].ThreatLevel = buffer.ReadInt32(); // todo: this should probably be a float
                    }
                    return contacts;
                    break;
                default:
                    typeName = null;
                    return null;
            }
        }

        private static bool UserTypeWriter(Lidgren.Network.NetBuffer buffer, object value, string typeName)
        {
            switch (typeName)
            {
                case "SesnsorContact":
                    break;
                case "SensorContact[]":
                    SensorContact[] contacts = (SensorContact[])value;
                    if (contacts == null || contacts.Length == 0) break;
                    buffer.Write(contacts.Length);
                    for (int i = 0; i < contacts.Length; i++)
                    {
                        buffer.Write(contacts[i].ContactID);
                        buffer.Write(contacts[i].Position.x);
                        buffer.Write(contacts[i].Position.y);
                        buffer.Write(contacts[i].Position.z);
                        buffer.Write(contacts[i].Velocity.x);
                        buffer.Write(contacts[i].Velocity.y);
                        buffer.Write(contacts[i].Velocity.z);
                        buffer.Write(contacts[i].IFF);            // identify friend or foe. This could be an Enum FRIEND, FOE, UNKNOWN
                        buffer.Write(contacts[i].IsTarget);
                        buffer.Write(contacts[i].IsGhost);       // occurs when the contact moves out of sensor range, or has used evasive maneuvers sufficiently, or has cloaked
                        buffer.Write(contacts[i].Age);
                        buffer.Write(contacts[i].GhostAge);
                        buffer.Write(contacts[i].Priority);
                        buffer.Write(contacts[i].ThreatLevel); // todo: this should probably be a float
                    }
                    break;
                default:
                    return false;
                    //break;
            }

            return true;
        }


        private static object MergeArrayElementsForUserTypes(string typeName, object currentValue, object value)
        {
            object result = null;

            ushort typeID = UserTypeIDFromTypename(typeName);
            switch (typeID)
            {

                case TYPE_SENSOR_CONTACT_ARRAY: //we don't want duplicates
                    if (currentValue == null) return value;

                    SensorContact[] existing = (SensorContact[])currentValue;
                    SensorContact[] newValues = (SensorContact[])value;

                    if (newValues == null) return existing;
                    bool found = false;

                    for (int i = 0; i < newValues.Length; i++)
                    {
                        for (int j = 0; j < existing.Length; j++)
                        {
                            if (newValues[i].ContactID == existing[j].ContactID)
                            {
                                //update the existing with newValue contact data
                                existing[j] = newValues[i];

                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            existing = Keystone.Extensions.ArrayExtensions.ArrayAppend(existing, newValues[i]);
                        found = false;
                    }

                    return existing;

                    // NOTE: we dont want a ArrayUnion because we don't want duplicates. We want to update the existing element with the new element with same contactID
                   // result = Keystone.Extensions.ArrayExtensions.ArrayUnion((string[])currentValue, (string[])value);
                    break;

                
                default:
                    throw new NotImplementedException("MergeArrayElementsForUserTypes() - ERROR: unsupported type '" + typeName + "'");
                    break;
            }

            return result;
        }
    }
}
