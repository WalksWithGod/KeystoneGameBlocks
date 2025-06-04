using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using System.Diagnostics;
using KeyCommon.DatabaseEntities ;
using KeyCommon.Persistence ;

namespace KeyServerCommon
{
    // '' <summary>
// '' 
// '' </summary>
// '' <remarks>ADO.NET which is the gateway to using Entity Framework is something we could
// '' migrate to eventually.  http://msdn.microsoft.com/en-us/library/bb399567(VS.100).aspx
// '' But our current method will be faster albeit less automated and requires more maintenance as 
// '' entities and database layouts change.
// '' </remarks>
public class SQLStorageContext : KeyCommon.Persistence.StorageContext 
{
    
    private PostgresProvider  mProvider;
    
    //  strategy pattern
    public SQLStorageContext(PostgresProvider provider)
    {
        if ((provider == null)) {
            throw new ArgumentNullException();
        }
        mProvider = provider;
    }
    
    // '' <summary>
    // '' Provider allows us to further swap in providers to handle the subtle syntax dissimilarties between SQL implementations
    // '' </summary>
    // '' <value></value>
    // '' <returns></returns>
    // '' <remarks></remarks>
    public PostgresProvider Provider
    {
        get {
            return mProvider;
        }
        set {
            mProvider = value;
        }
    }
    
    public string ConnectionString {
        get {
            if ((mProvider == null)) {
                return "";
            }
            return mProvider.ConnectionString;
        }
    }
    
    protected override void DisposeUnmanagedResources() {
        mProvider.Dispose();
    }
    
    public override int BeginTransaction() 
    {
        return mProvider.BeginTransaction();
    }
    
    public override void EndTransaction(int transactionID) 
    {
        mProvider.EndTransaction(transactionID);
    }

    public override void Delete(object value) 
    {
        Delete(value, -1);
    }
    public override void Delete(object value, int transactionID)
    {
        string typename = value.GetType().Name;
        string tableName;
        long recordID;
        
        //switch (typename) 
        //{
        //    case typeof(User).Name:
        //        User user;
        //        value;
        //        User;
        //        tableName = "users";
        //        recordID = user.PrimaryKey;
        //        break;
        //    case typeof(Game).Name:
        //        throw new NotImplementedException();
        //        break;
        //    case typeof(Player).Name:
        //        throw new NotImplementedException();
        //        break;
        //    case typeof(City).Name:
        //        throw new NotImplementedException();
        //        break;
        //    case typeof(ActiveUnit).Name:
        //        throw new NotImplementedException();
        //        break;
        //    default:
        //        Debug.WriteLine(("StorageContext.Delete() - Unknown type \'" 
        //                        + (typename + "\'")));
        //        throw new Exception();
        //        break;
        //}
        //  TODO: this really cant be at the bottom of this routine because some entities are spread across tables
        //          however, that doesnt always mean we're supposed to delete a record from every table.  Consider an ActiveUnit
        //         that references it's based stats in the Units table.  We certainly wont be deleting anything from the Units table
        //    So typically perhaps we only  delete one record, but as we flesh out the rest of the types we need, we may discover
        //   there are exceptions
        //mProvider.DeleteRecord(tableName, recordID, transactionID);
    }

    public override void Store(object entity)
    {
        Store(entity, -1);
    }

    public override void Store(object entity, int transactionID)
    {
        Type type = entity.GetType();
        // Warning!!! Optional parameters not supported
        EntityDefinition entityDef = GetEntityDefinition(type);
        FieldValuesFromObject(type, ref entity, ref entityDef);
        for (int i = 0; i < entityDef.TableDefinitions.Count; i++) 
        {
            long primaryKey = (long)(entityDef.TableDefinitions[i].FieldValues[0][0]);
            if (primaryKey != PostgresProvider.EMPTY_ID) 
            {
                mProvider.UpdateRecord(entityDef.TableDefinitions[i].TableName, entityDef.PKeySequence, primaryKey, entityDef.TableDefinitions[i].FieldNames, entityDef.TableDefinitions[i].FieldTypes, entityDef.TableDefinitions[i].FieldValues[0], transactionID);
                Debug.WriteLine(type.Name + " updated successfully.");
            }
            else 
            {
                //  this gets called when the record's primarykey = -1 which means its not currently in the database
                mProvider.AddRecord(entityDef.TableDefinitions[i].TableName, entityDef.PKeySequence, entityDef.TableDefinitions[i].FieldNames, entityDef.TableDefinitions[i].FieldTypes, entityDef.TableDefinitions[i].FieldValues[0], transactionID);
                Debug.WriteLine(type.Name + " added successfully.");
            }
        }
        //  if the entity was Added for the first time and not merely UPDATEd then it's "id" will have been set
        //  if that id was a serial or auto-incremented and so the 'entity' argument passed into this Store
        //  needs to have it's own 'id' updated to match what was set in the database record
        UpdateStoredEntitysID(entity, entityDef);
    }

    public override object RetreiveByID(Type type, long primaryKey) 
    {
        return RetreiveByID(type, primaryKey, -1);
    }

    public override object RetreiveByID(Type type, long primaryKey, int transactionID)
    {
        EntityDefinition entityDef = GetEntityDefinition(type);
        mProvider.RetreiveSingleRecord(entityDef, primaryKey, transactionID);
        object[] result = ObjectFromFieldValues(type, ref entityDef);

        if (result == null)
            return null;
        
        if (result.Length == 0) 
            return null;
        
        Debug.Assert(result.Length ==1);
        return result[0];
    }

    public override object Retreive(Type type, string searchFieldName, System.Data.DbType searchFieldType, object searchFieldValue)
    {
        // TODO: Here we could add a chache of EntityDefinition's we've retreived or stored recently to avoid a db lookup
        //         if possible
        return Retreive(type, searchFieldName, searchFieldType, searchFieldValue, -1);
    }
 
    public override object Retreive(Type type, string searchFieldName, System.Data.DbType searchFieldType, object searchFieldValue, int transactionID) 
    {
        EntityDefinition entityDef = GetEntityDefinition(type);
        mProvider.RetreiveSingleRecord(entityDef, searchFieldName, searchFieldType, searchFieldValue, transactionID);
        object[] result = ObjectFromFieldValues(type, ref entityDef);
        
        if (result == null) 
            return null;
        
        if (result.Length == 0)
            return null;
        
        Debug.Assert(result.Length ==1);
        return result[0];
    }

    public override object[] RetreiveList(Type type, string searchFieldName, System.Data.DbType searchFieldType, object searchFieldValue)
    {
        return RetreiveList(type, searchFieldName, searchFieldType, searchFieldValue, -1);
    }
    public override object[] RetreiveList(Type type, string searchFieldName, System.Data.DbType searchFieldType, object searchFieldValue, int transactionID)
    {
        EntityDefinition entityDef = GetEntityDefinition(type);
        mProvider.RetreiveRecords(entityDef, searchFieldName, searchFieldType, searchFieldValue, transactionID);
        if (entityDef.TableDefinitions[0].FieldValues != null) 
            return ObjectFromFieldValues(type, ref entityDef);
        
        return null;
    }
    
    // '' <summary>
    // '' </summary>
    // '' <param name="typeName"></param>
    // '' <returns>Returns an EntityDefinition object on success</returns>
    // '' <remarks>This may seem like it doesn't accomplish much.  Why not just hardcode the SQL queries in a giant Select Case statement?
    // '' well, that may very well be true.  The original idea behind this was to reduce redundant code in the Provider class and allow
    // '' the provider class to be inheritable and accomodating to any SQL Database you wanted including an embedded sql database for
    // '' the client such as sqlite with just tweaking minor tweaking and not re-implementations of huge swelect case statements.  With the 
    // '' following implementation, if you change a single class, you just change it here and you don't need to go through every singel
    // '' provider and tweak the SQL queries in each of them.  So just the editing of this single StorageContext class
    // '' allows any type of provider to "just work."</remarks>
    private EntityDefinition GetEntityDefinition(Type type) 
    {
        EntityDefinition entityDef = new EntityDefinition(type.Name);
        TableDefinition tableDef = new TableDefinition();

        if (type == typeof(AuthenticationRecord))
        {
            //        entityDef.PKeySequence = "users_history_id_seq";
            //        tableDef.TableName = "users_history";
            //        tableDef.Redim(5);
            //        tableDef.FieldNames(0) = "id";
            //        tableDef.FieldNames(1) = "userid";
            //        tableDef.FieldNames(2) = "ip";
            //        tableDef.FieldNames(3) = "time";
            //        tableDef.FieldNames(4) = "succeeded";
            //        tableDef.FieldTypes(0) = DbType.Int64;
            //        tableDef.FieldTypes(1) = DbType.Int32;
            //        tableDef.FieldTypes(2) = DbType.String;
            //        tableDef.FieldTypes(3) = DbType.DateTime;
            //        tableDef.FieldTypes(4) = DbType.Boolean;
            //        entityDef.TableDefinitions.Add(tableDef);

        }
        else if (type == typeof( User))
        {
            entityDef.PKeySequence = "";
            tableDef.TableName = "users";
            tableDef.Redim(7);
            tableDef.FieldNames[0] = "id";
            tableDef.FieldNames[1] = "name";
            tableDef.FieldNames[2] = "customerid";
            tableDef.FieldNames[3] = "password";
            tableDef.FieldNames[4] = "isadmin";
            tableDef.FieldNames[5] = "suspended";
            tableDef.FieldNames[6] = "date_inserted";
            tableDef.FieldTypes[0] = DbType.Int64;
            tableDef.FieldTypes[1] = DbType.String;
            tableDef.FieldTypes[2] = DbType.Int32;
            tableDef.FieldTypes[3] = DbType.String;
            tableDef.FieldTypes[4] = DbType.Boolean;
            tableDef.FieldTypes[5] = DbType.Boolean;
            tableDef.FieldTypes[6] = DbType.DateTime;
            entityDef.TableDefinitions.Add(tableDef);
     
        }
        else if (type == typeof(Host))
        {
            entityDef.PKeySequence = "";
            tableDef.TableName = "hosts";
            tableDef.Redim(3);
            tableDef.FieldNames[0] = "id";
            tableDef.FieldNames[1] = "name";
            tableDef.FieldNames[2] = "password";
            tableDef.FieldTypes[0] = DbType.Int64;
            tableDef.FieldTypes[1] = DbType.String;
            tableDef.FieldTypes[2] = DbType.String;
            entityDef.TableDefinitions.Add(tableDef);

        }
            //  ==================================================END AUTH DATABASE
            //        //  ==================================================START GAMES DATABASE

            //    else if (obj is Game)
            //      {
            //        entityDef.PKeySequence = "games_id_seq";
            //        tableDef.TableName = "games";
            //        tableDef.Redim(11);
            //        tableDef.FieldNames(0) = "id";
            //        tableDef.FieldNames(1) = "name";
            //        tableDef.FieldNames(2) = "server_name";
            //        tableDef.FieldNames(3) = "status";
            //        tableDef.FieldNames(4) = "resolution";
            //        tableDef.FieldNames(5) = "password";
            //        tableDef.FieldNames(6) = "worldwidth";
            //        tableDef.FieldNames(7) = "worldheight";
            //        tableDef.FieldNames(8) = "turn_number";
            //        tableDef.FieldNames(9) = "game_start";
            //        tableDef.FieldNames(10) = "game_end";
            //        tableDef.FieldTypes(0) = DbType.Int64;
            //        tableDef.FieldTypes(1) = DbType.String;
            //        tableDef.FieldTypes(2) = DbType.String;
            //        tableDef.FieldTypes(3) = DbType.Byte;
            //        tableDef.FieldTypes(4) = DbType.Byte;
            //        tableDef.FieldTypes(5) = DbType.String;
            //        tableDef.FieldTypes(6) = DbType.Int16;
            //        tableDef.FieldTypes(7) = DbType.Int16;
            //        tableDef.FieldTypes(8) = DbType.Int32;
            //        tableDef.FieldTypes(9) = DbType.DateTime;
            //        tableDef.FieldTypes(10) = DbType.DateTime;
            //        entityDef.TableDefinitions.Add(tableDef);
            //    }
            //    else if (obj  is Player)
            //  {
            //        entityDef.PKeySequence = "";
            //        tableDef.TableName = "players";
            //        tableDef.Redim(6);
            //        tableDef.FieldNames(0) = "id";
            //        tableDef.FieldNames(1) = "name";
            //        tableDef.FieldNames(2) = "game_id";
            //        tableDef.FieldNames(3) = "faction_id";
            //        tableDef.FieldNames(4) = "faction_leader_name";
            //        tableDef.FieldNames(5) = "status";
            //        tableDef.FieldTypes(0) = DbType.Int64;
            //        tableDef.FieldTypes(1) = DbType.String;
            //        tableDef.FieldTypes(2) = DbType.Int64;
            //        tableDef.FieldTypes(3) = DbType.Int32;
            //        tableDef.FieldTypes(4) = DbType.String;
            //        tableDef.FieldTypes(5) = DbType.Byte;
            //        entityDef.TableDefinitions.Add(tableDef);
            //        //  ==================================================END GAMES DATABASE
            //        //  ==================================================START SIMS DATABASE
            //    }
            //    else if (obj is ST_GameState)
            //  {
            //        entityDef.PKeySequence = "entities_id_seq";
            //        tableDef.TableName = "st_table";
            //        tableDef.Redim(12);
            //        tableDef.FieldNames(0) = "id";
            //        tableDef.FieldNames(1) = "val1";
            //        tableDef.FieldNames(2) = "val2";
            //        tableDef.FieldNames(3) = "val3";
            //        tableDef.FieldNames(4) = "const1";
            //        tableDef.FieldNames(5) = "const2";
            //        tableDef.FieldNames(6) = "const3";
            //        tableDef.FieldNames(7) = "const4";
            //        tableDef.FieldNames(8) = "const5";
            //        tableDef.FieldNames(9) = "s1";
            //        tableDef.FieldNames(10) = "s2";
            //        tableDef.FieldNames(11) = "s3";
            //        tableDef.FieldTypes(0) = DbType.Int64;
            //        tableDef.FieldTypes(1) = DbType.Int32;
            //        tableDef.FieldTypes(2) = DbType.Int32;
            //        tableDef.FieldTypes(3) = DbType.Int32;
            //        tableDef.FieldTypes(4) = DbType.Int32;
            //        tableDef.FieldTypes(5) = DbType.Int32;
            //        tableDef.FieldTypes(6) = DbType.Int32;
            //        tableDef.FieldTypes(7) = DbType.Int32;
            //        tableDef.FieldTypes(8) = DbType.Int32;
            //        tableDef.FieldTypes(9) = DbType.String;
            //        tableDef.FieldTypes(10) = DbType.String;
            //        tableDef.FieldTypes(11) = DbType.String;
            //        entityDef.TableDefinitions.Add(tableDef);
            //     }
            //    else if (obj is City)
            //    {
            //        throw new NotImplementedException();
            //        entityDef.TableDefinitions.Add(tableDef);
            //    }
            //    else if (obj is ActiveUnit)
            //  {
            //        entityDef.PKeySequence = "entities_id_seq";
            //        tableDef.TableName = "units_active";
            //        tableDef.Redim(8);
            //        tableDef.FieldNames(0) = "id";
            //        tableDef.FieldNames(1) = "base_unit_id";
            //        tableDef.FieldNames(2) = "game_id";
            //        tableDef.FieldNames(3) = "player_id";
            //        tableDef.FieldNames(4) = "name";
            //        tableDef.FieldNames(5) = "positionX";
            //        tableDef.FieldNames(6) = "positionY";
            //        tableDef.FieldNames(7) = "hitpoints_remaining";
            //        tableDef.FieldTypes(0) = DbType.Int64;
            //        tableDef.FieldTypes(1) = DbType.Int32;
            //        tableDef.FieldTypes(2) = DbType.Int64;
            //        tableDef.FieldTypes(3) = DbType.Int32;
            //        tableDef.FieldTypes(4) = DbType.String;
            //        tableDef.FieldTypes(5) = DbType.Int32;
            //        tableDef.FieldTypes(6) = DbType.Int32;
            //        tableDef.FieldTypes(7) = DbType.Int32;
            //        entityDef.TableDefinitions.Add(tableDef);
            //        //  table for the base definition
            //        tableDef = new TableDefinition();
            //        tableDef.TableName = "units_base";
            //        tableDef.Redim(13);
            //        tableDef.FieldNames(0) = "id";
            //        tableDef.FieldNames(1) = "name";
            //        tableDef.FieldNames(2) = "category";
            //        tableDef.FieldNames(3) = "strength";
            //        tableDef.FieldNames(4) = "movement_points";
            //        tableDef.FieldNames(5) = "cost";
            //        tableDef.FieldNames(6) = "tech_reqts";
            //        tableDef.FieldNames(7) = "abilities_flags";
            //        tableDef.FieldNames(8) = "promotions_flags";
            //        tableDef.FieldNames(9) = "resource_reqts";
            //        tableDef.FieldNames(10) = "icon_filename";
            //        tableDef.FieldNames(11) = "model_filename";
            //        tableDef.FieldNames(12) = "definition_filename";
            //        tableDef.FieldTypes(0) = DbType.Int64;
            //        tableDef.FieldTypes(1) = DbType.String;
            //        tableDef.FieldTypes(2) = DbType.Int32;
            //        tableDef.FieldTypes(3) = DbType.Int32;
            //        tableDef.FieldTypes(4) = DbType.Int32;
            //        tableDef.FieldTypes(5) = DbType.Int32;
            //        tableDef.FieldTypes(6) = DbType.Int32;
            //        tableDef.FieldTypes(7) = DbType.Binary;
            //        // DbType.Binary ADO type maps to bytea (byte-array) in postgresql
            //        tableDef.FieldTypes(8) = DbType.Binary;
            //        tableDef.FieldTypes(9) = DbType.Int32;
            //        tableDef.FieldTypes(10) = DbType.String;
            //        tableDef.FieldTypes(11) = DbType.String;
            //        tableDef.FieldTypes(12) = DbType.String;
            //        entityDef.TableDefinitions.Add(tableDef);
            //  }
        else
                {
                    throw new Exception("SQLStorageContext:GetEntityDefinition() - Unknown entity type '" + type.Name + "'");

                }
        
        return entityDef;
        
    }
    
    // '' <summary>
    // '' The PrimaryKey field must be the first field always.
    // '' </summary>
    // '' <param name="typeName"></param>
    // '' <param name="entityDef"></param>
    // '' <returns></returns>
    // '' <remarks></remarks>
    private object[] ObjectFromFieldValues(Type type, ref EntityDefinition entityDef) 
    {
        int count = entityDef.TableDefinitions[0].FieldValues.Count;
        
        
         if (type == typeof(AuthenticationRecord))
         {
        //        AuthenticationRecord[,] authenticationRecord;
        //        for (int i = 0; i <count; i++) 
        //        {
        //            authenticationRecord[i] = new AuthenticationRecord();
        //            authenticationRecord[i].PrimaryKey = Convert.ToInt64(entityDef.TableDefinitions(0).FieldValues(i)[0]);
        //            authenticationRecord[i].UserID = Convert.ToInt32(entityDef.TableDefinitions(0).FieldValues(i)[1]);
        //            authenticationRecord[i].IPAddress = Convert.ToString(entityDef.TableDefinitions(0).FieldValues(i)[2]);
        //            authenticationRecord[i].Date = Convert.ToDateTime(entityDef.TableDefinitions(0).FieldValues(i)[3]);
        //            authenticationRecord[i].Succeeded = Convert.ToBoolean(entityDef.TableDefinitions(0).FieldValues(i)[4]);
        //        }
              return null; // return authenticationRecord;
         }
         else if (type == typeof( User))
         {
             User[] user = new User[count];
             for (int i = 0; i < count; i++)
             {
                 long ID = Convert.ToInt64(entityDef.TableDefinitions[0].FieldValues[i][0]);
                 user[i] = new User(ID);
                 user[i].Name = Convert.ToString(entityDef.TableDefinitions[0].FieldValues[i][1]);
                 user[i].CustomerID = Convert.ToInt32(entityDef.TableDefinitions[0].FieldValues[i][2]);
                 user[i].Password = Convert.ToString(entityDef.TableDefinitions[0].FieldValues[i][3]);
                 user[i].IsAdmin = Convert.ToBoolean(entityDef.TableDefinitions[0].FieldValues[i][4]);
                 user[i].IsSuspended = Convert.ToBoolean(entityDef.TableDefinitions[0].FieldValues[i][5]);
                 //user[i].mJoinedDate = DateTime.Parse (string) (entityDef.TableDefinitions[0].FieldValues[i][6]);
             }
             return user;
         }
             // obsolete now that host and user tables are merged
         //else if (type == typeof( Host))
         //{
         //       Host[] host = new Host[count];
         //       for (int i = 0; i < count; i++)
         //       {
         //           host[i] = new Host(null);
         //           host[i].PrimaryKey = Convert.ToInt64(entityDef.TableDefinitions[0].FieldValues[i][0]);
         //           host[i].Name = Convert.ToString(entityDef.TableDefinitions[0].FieldValues[i][1]);
         //           host[i].Password = Convert.ToString(entityDef.TableDefinitions[0].FieldValues[i][2]);                
         //       }
         //       return host;
         //}
        //    else if( type == typeof(Game))
       //    {
        //        Game[,] game;
        //        for (int i = 0; i < count ; i++) 
        //        {
        //            game[i] = new Game();
        //            game[i].PrimaryKey = Convert.ToInt64(entityDef.TableDefinitions(0).FieldValues(i)[0]);
        //            game[i].mName = Convert.ToString(entityDef.TableDefinitions(0).FieldValues(i)[1]);
        //            game[i].mServerName = Convert.ToString(entityDef.TableDefinitions(0).FieldValues(i)[2]);
        //            Convert.ToByte(entityDef.TableDefinitions(0).FieldValues(i)[3]);
        //            Game.GameStatus;
        //            Convert.ToByte(entityDef.TableDefinitions(0).FieldValues(i)[4]);
        //            Game.GameResolution;
        //            game[i].mPassword = Convert.ToString(entityDef.TableDefinitions(0).FieldValues(i)[5]);
        //            game[i].mWidth = Convert.ToInt16(entityDef.TableDefinitions(0).FieldValues(i)[6]);
        //            game[i].mHeight = Convert.ToInt16(entityDef.TableDefinitions(0).FieldValues(i)[7]);
        //            game[i].mTurn = Convert.ToInt32(entityDef.TableDefinitions(0).FieldValues(i)[8]);
        //            game[i].mStart = Convert.ToDateTime(entityDef.TableDefinitions(0).FieldValues(i)[9]);
        //            game[i].mEnd = Convert.ToDateTime(entityDef.TableDefinitions(0).FieldValues(i)[10]);
        //        }
        //        return game;
        //      }
        //    else if ( type == typeof(Player))
             // {
        //        Player[,] player;
        //        for (int i = 0; i < count ; i++) 
        //        {
        //            player[i] = new Player();
        //            player[i].PrimaryKey = Convert.ToInt64(entityDef.TableDefinitions(0).FieldValues(i)[0]);
        //            player[i].mName = Convert.ToString(entityDef.TableDefinitions(0).FieldValues(i)[1]);
        //            player[i].mGameID = Convert.ToInt64(entityDef.TableDefinitions(0).FieldValues(i)[2]);
        //            ActiveFaction faction = new ActiveFaction();
        //            Faction baseFaction = new Faction();
        //            baseFaction.PrimaryKey = Convert.ToInt64(entityDef.TableDefinitions(0).FieldValues(i)[3]);
        //            faction.LeaderName = Convert.ToString(entityDef.TableDefinitions(0).FieldValues(i)[4]);
        //            faction.BaseFaction = baseFaction;
        //            player[i].mFaction = faction;
        //            Convert.ToByte(entityDef.TableDefinitions(0).FieldValues(i)[5]);
        //            ClientServerCommon.Player.PlayerStatus;
        //            // faction.mPositionID() ' from the next def TableDefinition(1)
        //        }
        //        return player;
        //        break;
        //    case typeof(City).Name:
        //        City[,] city;
        //        throw new NotImplementedException();
        //        return city;
        //        break;
        //    case typeof(ActiveUnit).Name:
        //        ActiveUnit[,] unit;
        //        for (int i = 0; i < count; i++) 
        //        {
        //            unit[i] = new ActiveUnit();
        //            unit[i].PrimaryKey = Convert.ToInt64(entityDef.TableDefinitions(0).FieldValues(i)[0]);
        //            // Dim baseunit As New BaseUnit()
        //            // baseunit.PrimaryKey = Convert.ToInt64(entityDef.TableDefinitions(0).FieldValues(i)(0))
        //            // baseunit.name = Convert.ToString(entityDef.TableDefinitions(0).FieldValues(i)(1))
        //            // baseunit.Category = "category"
        //            // baseunit.Strength = "strength"
        //            // baseunit.MovementPoints = "movement_points"
        //            // baseunit.Cost = "cost"
        //            // baseunit.TechRequirements = "tech_reqts"
        //            // baseunit.Abilities = "abilities_flags"
        //            // baseunit.SetPromotionsFlag = CType(entityDef.TableDefinitions(1).FieldValues(i)(8), Byte())
        //            // baseunit.ResourceRequirements = "resource_reqts"
        //            // baseunit.IconFile = Convert.ToString(entityDef.TableDefinitions(0).FieldValues(i)(10))
        //            // baseunit.ModelFile = Convert.ToString(entityDef.TableDefinitions(0).FieldValues(i)(11))
        //            // baseunit.DefinitionFile = Convert.ToString(entityDef.TableDefinitions(0).FieldValues(i)(12))
        //            // unit(i).Unit = baseunit
        //            // unit(i).PlayerID = Convert.ToInt64(entityDef.TableDefinitions(0).FieldValues(i)(3)) ' TODO: verify PlayerID is int64 and not int32
        //            // unit(i).Name = Convert.ToString(entityDef.TableDefinitions(0).FieldValues(i)(4))
        //            // unit(i).PositionX = Convert.ToInt32(entityDef.TableDefinitions(0).FieldValues(i)(5))
        //            // unit(i).PositionY = Convert.ToInt32(entityDef.TableDefinitions(0).FieldValues(i)(6))
        //            // unit(i).HitPointsRemaining = Convert.ToInt32(entityDef.TableDefinitions(0).FieldValues(i)(7))
        //        }
        //        return unit;
        //        break;
            else 
            {
                throw new Exception("SQLStorageContext:ObjectFromFieldValues() - Unknown entity type '" + type.Name + "'");
            }
        
    }
    
    //  this will unfortunatly be a relatively high maintenance bit of code
    //  we can later add seperate private functions to handle each type to make writing unit tests easier
    //  but overall there probably can't be a need for more than 30 and probably more like a dozen?  
    //  The only real alternative that would cut down on code would involve reflection and thus slower
    //  Note: It's preferable to have this code in SQLStorageContext and not inside each type
    //  because here the type isnt restricted to any single storage mechanism.  By implementing a new StroageContext
    //  we can save it to any storage we want (xml, external sql, embedded db, csv files, etc)
    private void FieldValuesFromObject(Type type, ref object target, ref EntityDefinition entityDef) 
    {

        //    case typeof(ST_GameState).Name:
        //        ST_GameState GameState;
        //        target;
        //        ST_GameState;
        //        entityDef.TableDefinitions(0).FieldValues.Add(new object[] {
        //                    12});
        //        entityDef.TableDefinitions(0).FieldValues(0)[0] = GameState.PrimaryKey;
        //        entityDef.TableDefinitions(0).FieldValues(0)[1] = GameState.val1;
        //        entityDef.TableDefinitions(0).FieldValues(0)[2] = GameState.val2;
        //        entityDef.TableDefinitions(0).FieldValues(0)[3] = GameState.val3;
        //        entityDef.TableDefinitions(0).FieldValues(0)[4] = GameState.const1;
        //        entityDef.TableDefinitions(0).FieldValues(0)[5] = GameState.const2;
        //        entityDef.TableDefinitions(0).FieldValues(0)[6] = GameState.const3;
        //        entityDef.TableDefinitions(0).FieldValues(0)[7] = GameState.const4;
        //        entityDef.TableDefinitions(0).FieldValues(0)[8] = GameState.const5;
        //        entityDef.TableDefinitions(0).FieldValues(0)[9] = GameState.s1;
        //        entityDef.TableDefinitions(0).FieldValues(0)[10] = GameState.s2;
        //        entityDef.TableDefinitions(0).FieldValues(0)[11] = GameState.s3;
        //        break;
        if  (type == typeof( AuthenticationRecord))
        {
        //        AuthenticationRecord authenticationRecord = (AuthenticationRecord) target;
        //        entityDef.TableDefinitions(0).FieldValues.Add(new object[5]);
        //        entityDef.TableDefinitions(0).FieldValues(0)[0] = authenticationRecord.PrimaryKey;
        //        entityDef.TableDefinitions(0).FieldValues(0)[1] = authenticationRecord.UserID;
        //        entityDef.TableDefinitions(0).FieldValues(0)[2] = authenticationRecord.IPAddress;
        //        entityDef.TableDefinitions(0).FieldValues(0)[3] = authenticationRecord.Date;
        //        entityDef.TableDefinitions(0).FieldValues(0)[4] = authenticationRecord.Succeeded;
        }
        else if (type == typeof(User))
        {
            User user = (User)target;
            entityDef.TableDefinitions[0].FieldValues.Add(new object[7]);
            entityDef.TableDefinitions[0].FieldValues[0][0] = user.ID;
            entityDef.TableDefinitions[0].FieldValues[0][1] = user.Name;
            entityDef.TableDefinitions[0].FieldValues[0][2] = user.CustomerID;
            entityDef.TableDefinitions[0].FieldValues[0][3] = user.Password;
            entityDef.TableDefinitions[0].FieldValues[0][4] = user.IsAdmin;
            entityDef.TableDefinitions[0].FieldValues[0][5] = user.IsSuspended;
            //entityDef.TableDefinitions[0].FieldValues[0][6] = DateTime.Now;
        }
            // obsolete now that host and user tables are merged
        //else if (type == typeof(Host))
        //{
        //    Host host = (Host)target;

        //    entityDef.TableDefinitions[0].FieldValues.Add(new object[3]);
        //    entityDef.TableDefinitions[0].FieldValues[0][0] = host.PrimaryKey;
        //    entityDef.TableDefinitions[0].FieldValues[0][1] = host.Name;
        //    entityDef.TableDefinitions[0].FieldValues[0][2] = host.Password;
        //}
        //    case typeof(Game).Name:
        //        Game game;
        //        target;
        //        Game;
        //        entityDef.TableDefinitions(0).FieldValues.Add(new object[] {
        //                    11});
        //        entityDef.TableDefinitions(0).FieldValues(0)[0] = game.PrimaryKey;
        //        entityDef.TableDefinitions(0).FieldValues(0)[1] = game.mName;
        //        entityDef.TableDefinitions(0).FieldValues(0)[2] = game.mServerName;
        //        entityDef.TableDefinitions(0).FieldValues(0)[3] = game.mStatus;
        //        entityDef.TableDefinitions(0).FieldValues(0)[4] = game.mResolution;
        //        entityDef.TableDefinitions(0).FieldValues(0)[5] = game.mPassword;
        //        entityDef.TableDefinitions(0).FieldValues(0)[6] = game.mWidth;
        //        entityDef.TableDefinitions(0).FieldValues(0)[7] = game.mHeight;
        //        entityDef.TableDefinitions(0).FieldValues(0)[8] = game.mTurn;
        //        entityDef.TableDefinitions(0).FieldValues(0)[9] = game.mStart;
        //        entityDef.TableDefinitions(0).FieldValues(0)[10] = game.mEnd;
        //        break;
        //    case typeof(Player).Name:
        //        Player player;
        //        target;
        //        Player;
        //        entityDef.TableDefinitions(0).FieldValues.Add(new object[] {
        //                    6});
        //        entityDef.TableDefinitions(0).FieldValues(0)[0] = player.PrimaryKey;
        //        entityDef.TableDefinitions(0).FieldValues(0)[1] = player.mName;
        //        entityDef.TableDefinitions(0).FieldValues(0)[2] = player.mGameID;
        //        entityDef.TableDefinitions(0).FieldValues(0)[3] = player.mFaction.BaseFaction.PrimaryKey;
        //        entityDef.TableDefinitions(0).FieldValues(0)[4] = player.mFaction.LeaderName;
        //        entityDef.TableDefinitions(0).FieldValues(0)[5] = player.mStatus;
        //        break;
        //    case typeof(City).Name:
        //        City city;
        //        target;
        //        City;
        //        entityDef.TableDefinitions(0).FieldValues.Add(new object[] {
        //                    6});
        //        throw new NotImplementedException();
        //        break;
        //    case typeof(ActiveUnit).Name:
        //        ActiveUnit unit;
        //        target;
        //        ActiveUnit;
        //        entityDef.TableDefinitions(0).FieldValues.Add(new object[] {
        //                    6});
        //        entityDef.TableDefinitions(0).FieldValues(0)[0] = unit.PrimaryKey;
        //        throw new NotImplementedException();
        //        break;
        else
        {
            throw new Exception("SQLStorageContext:FieldValuesFromObject() - Unknown entity type '" + type.Name + "'");
        }
        
    }
    
    private void UpdateStoredEntitysID(object entity, EntityDefinition entityDef) 
    {
        
        
        //    case typeof(ST_GameState).Name:
        //        ST_GameState state;
        //        entity;
        //        ST_GameState;
        //        state.PrimaryKey = long.Parse(entityDef.TableDefinitions(0).FieldValues(0)[0]);
        //        break;
        //    case typeof(AuthenticationRecord).Name:
        //        AuthenticationRecord authenticationRecord;
        //        entity;
        //        AuthenticationRecord;
        //        authenticationRecord.PrimaryKey = long.Parse(entityDef.TableDefinitions(0).FieldValues(0)[0]);
        //        break;
        if (entity is User)
        {
            User user = (User)entity;
            //user.ID = (long)(entityDef.TableDefinitions[0].FieldValues[0][0]);
        }
        else if (entity is Host)
        {
            Host host = (Host)entity;
            //host.PrimaryKey = (long)(entityDef.TableDefinitions[0].FieldValues[0][0]);
        }
        //    case typeof(Game).Name:
        //        Game game;
        //        entity;
        //        Game;
        //        game.PrimaryKey = long.Parse(entityDef.TableDefinitions(0).FieldValues(0)[0]);
        //        break;
        //    case typeof(Player).Name:
        //        Player player;
        //        entity;
        //        Player;
        //        player.PrimaryKey = long.Parse(entityDef.TableDefinitions(0).FieldValues(0)[0]);
        //        break;
        //    case typeof(City).Name:
        //        City city;
        //        entity;
        //        City;
        //        city.PrimaryKey = long.Parse(entityDef.TableDefinitions(0).FieldValues(0)[0]);
        //        break;
        //    case typeof(ActiveUnit).Name:
        //        ActiveUnit unit;
        //        entity;
        //        ActiveUnit;
        //        unit.PrimaryKey = long.Parse(entityDef.TableDefinitions(0).FieldValues(0)[0]);
        //        break;
        else
        {
            throw new Exception();
        }
        
    }
}
}
