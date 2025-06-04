using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Keystone.Types;

namespace KeyEdit.Database
{
    /// <summary>
    /// AppDatabaseHelper is EXE specific database helper for common database calls.
    /// TODO: This really belongs in Game00.dll or KeyCommon.dll (and KeyCommon must be removed as reference in Keystone.dll)
    /// but we will keep it here for now since on the loopback client/server EXE needs it.  But that could change
    /// if we discover that keyscript.dll needs db/storage access too.
    /// </summary>
    public class AppDatabaseHelper
    {
        public struct TaskRecord
        {
            public long ID;
        }


        // TODO: can/should we use StarRecord to actually be CelestialBodyRecords? that is, contain stars and worlds(including moons)?
        //       I'm thinking no because we'll eventually want to add more fields that are specific to each Stars and Worlds.  But
        //       I'm not sure.  We are still just dev'ing by the seat of our pants trying to figure out the dilineation between XML and sqliteDB.
        //       
        public struct StarRecord
        {
            public string RegionID;
            public string ID;
            public string Name;
            public double Mass;
            public double Radius;
            public Vector3d Translation;
            public Vector3d GlobalTranslation;
        }

        public struct WorldRecord
        {
            public string RegionID;
            public string ID;
            public string ParentID; // ID of Entity which this World orbits (Stars or for moons, other Worlds)
            public string Name;
            public double Mass;
            public double Radius;
            public double OrbitalRadius;
            public Vector3d Translation;
            public Vector3d GlobalTranslation;
        }

        public struct VehicleRecord
        {
            public string OwnerID;
            public string RegionID;
            public string ID;
            public string ParentID; // Is this the same as RegionID or can it be a star or world which this vehicle orbits?
            public string Name;
            public string RelativeEntitySavePath; // relative path to Vehicle Save Entity used by this vehicle
            public Vector3d Translation;
        }

        public struct CharacterRecord
        {
            public string ID;
            public string ParentID;
            public string FirstName;
            public string LastName;
            public string RelativeEntitySavePath;
            public Vector3d Translation;
        }


        public AppDatabaseHelper()
        {
        }

        public static SQLiteConnection GetConnection()
        {
            SQLiteConnection connect = new SQLiteConnection(@"Data Source=" + SaveFullPath + ";Version=3;");
            connect.Open();
            return connect;
        }

        // todo: i need a seperate db for server db that hosts the users table.  It does not belong in \\saves\\ 
        //       Further, the same server db should be used for all Campaigns 
        // this represents a saving of app specific state.  In a future multiplayer build, we can use a single db perhaps or seperate ones for each user's crew and vehicle game state
        // TODO: what do we do when first creating the Scene and a Vehicle has yet to be added to the Scene?  Joining a new game should require player select a Vehicle first.
        public static void CreateSave(string campaignName, string saveFileName)
        {
            string fullPath = System.IO.Path.Combine(AppMain.SCENES_PATH, campaignName);
            fullPath = System.IO.Path.Combine (fullPath, saveFileName);

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            SaveFullPath = fullPath;
            CreateDatabase(fullPath);

        }

        public static string SaveFullPath;

        public static string GetSavePath(string saveFileName)
        {
            string fullPath = System.IO.Path.Combine(AppMain.SAVES_PATH, saveFileName);
            SaveFullPath = fullPath;
            return fullPath;
        }



        public static void UpdateSave(string saveFileName)
        {
            string fullPath = System.IO.Path.Combine(AppMain.SAVES_PATH, saveFileName);
        }

        private static void CreateDatabase(string databasePath)
        {
            if (System.IO.File.Exists(databasePath))
                System.IO.File.Delete(databasePath);

            // In SQLite3 opening the db will also create the file if it doesn't exist
            // NOTE: GetConnection() returns an opened connection 
            System.Data.SQLite.SQLiteConnection conn = GetConnection();

                               

            // Vehicles - including user's vehicle as Avatar
            // TODO: still a question as to how do we represent Vehicle "components" such as engines, reactors, bunks, sensors, crew stations, etc. as well as Damage to Hull and internal structure
            //       I think we just need to go with a simplified approach.
            string sql = "create table vehicles (id varchar(16), ownerid varchar(16), typename varchar(20), regionid varchar(16), parentid varchar(16), prefab varchar(256), name varchar(20), translationX double, translationY double, translationZ double)";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();

            // Stars
            sql = "create table stars (id varchar(16), typename varchar(20), name varchar(20), regionid varchar(16), mass double, radius double, translationX double, translationY double, translationZ double, globaltranslationX double, globaltranslationY double, globaltranslationZ double)";
            command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();

            // Worlds and Moons
            sql = "create table worlds (id varchar(16), parentid varchar(16), typename varchar(20), name varchar(20), regionid varchar(16), mass double, radius double, orbitalradius double, translationX double, translationY double, translationZ double, globaltranslationX double, globaltranslationY double, globaltranslationZ double)";
            command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();

            // Vehicle waypoints
            sql = "create table waypoints (ownerid varchar(16), regionid varchar(16), targetid varchar(16), translationX double, translationY double, translationZ double)";
            command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();

            // vehicle components - custom properties
            // todo: issue here is, why not just store all that in the XML using the custom properties?  They are defined in script why define them again here in SQL db?
            // todo: perhaps it's the difference between game agnostic SCENE data versus game specific data? Saving custom properties to the XML Scene may be undesireable?
            // todo: this makes sense afterall since things like Appearance, Material, Animations, Geometry, AudioNodes, Scripts are all scene node elements.  We store the actual gameplay specific data seperately.

            // npc characters
            sql = "create table characters (id varchar(16), parentid varchar(16), firstname varchar(20), middlename varchar(20), lastname varchar(20), prefab varchar(256), translationX double, translationY double, translationZ double)";
            command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();

            // requests

            // tasks // TODO: do we need a seperate table for completed tasks? Or is that just queried through "status" field?
            sql = "create table tasks_server (id unsigned big int, ownerid varchar(16), task int, status int, assigneddatetime bigint, assignedby varchar(16), args varchar(256), notes1 varchar(256), notes2 varchar(256))";
            command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();

            sql = "create table tasks_client (id unsigned big int, ownerid varchar(16), task int, status int, assigneddatetime bigint, assignedby varchar(16), args varchar(256), notes1 varchar(256), notes2 varchar(256))";
            command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();

            conn.Close();
        }

        #region Get


        public static TaskRecord[] GetTaskRecords()
        {
            //task.AssignedByID = request.AssignedByID;
            //task.AssignedDateTime = request.AssignedDateTime;
            //task.Task = request.Task;
            //task.Status = 0;
            //task.Args = request.Args;
            //task.Notes1 = request.Notes1;

            return null;
        }

        public static StarRecord[] GetStarRecords()
        {
            List<StarRecord> starList = new List<StarRecord>();

            using (SQLiteConnection connect = GetConnection())
            {
                using (SQLiteCommand fmd = connect.CreateCommand())
                {
                    fmd.CommandText = @"SELECT id, name, regionid, mass, radius, translationX, translationY, translationZ, globaltranslationX, globaltranslationY, globaltranslationZ FROM stars";
                    fmd.CommandType = System.Data.CommandType.Text;

                    SQLiteDataReader r = fmd.ExecuteReader();

                    while (r.Read())
                    {
                        StarRecord record;
                        record.ID = Convert.ToString(r["id"]);
                        record.Name = Convert.ToString(r["name"]);
                        record.RegionID = Convert.ToString(r["regionid"]);
                        record.Mass = Convert.ToDouble(r["mass"]);
                        record.Radius = Convert.ToDouble(r["radius"]);
                        Vector3d translation;
                        // NOTE: star translations are in global coords
                        translation.x = Convert.ToDouble(r["translationX"]);
                        translation.y = Convert.ToDouble(r["translationY"]);
                        translation.z = Convert.ToDouble(r["translationZ"]);
                        record.Translation = translation;

                        translation.x = Convert.ToDouble(r["globaltranslationX"]);
                        translation.y = Convert.ToDouble(r["globaltranslationY"]);
                        translation.z = Convert.ToDouble(r["globaltranslationZ"]);
                        record.GlobalTranslation = translation;

                        starList.Add(record);
                    }
                }
                connect.Close();
            }

            return starList.ToArray();
        }

        public static WorldRecord GetWorldRecordByName(string name)
        {
            WorldRecord record;

            using (SQLiteConnection connect = GetConnection())
            {
                using (SQLiteCommand fmd = connect.CreateCommand())
                {
                    fmd.CommandText = @"SELECT id, name, regionid, parentid, mass, radius, orbitalradius, translationX, translationY, translationZ, globaltranslationX, globaltranslationY, globaltranslationZ FROM worlds WHERE name = @name";
                    fmd.CommandType = System.Data.CommandType.Text;
                    fmd.Parameters.Add(new SQLiteParameter(@"name", name));

                    SQLiteDataReader r = fmd.ExecuteReader();

                    r.Read();
                                        
                    record.ID = Convert.ToString(r["id"]);
                    record.Name = Convert.ToString(r["name"]);
                    record.RegionID = Convert.ToString(r["regionid"]);
                    record.ParentID = Convert.ToString(r["parentid"]);
                    record.Mass = Convert.ToDouble(r["mass"]);
                    record.Radius = Convert.ToDouble(r["radius"]);
                    record.OrbitalRadius = Convert.ToDouble(r["orbitalradius"]);
                    Vector3d translation;
                    translation.x = Convert.ToDouble(r["translationX"]);
                    translation.y = Convert.ToDouble(r["translationY"]);
                    translation.z = Convert.ToDouble(r["translationZ"]);
                    record.Translation = translation;

                    translation.x = Convert.ToDouble(r["globaltranslationX"]);
                    translation.y = Convert.ToDouble(r["globaltranslationY"]);
                    translation.z = Convert.ToDouble(r["globaltranslationZ"]);
                    record.GlobalTranslation = translation;
                    
                }
                connect.Close();
            }
            return record;
        }

        public static WorldRecord[] GetWorldRecords(string parentID)
        {
            List<WorldRecord> worldList = new List<WorldRecord>();

            using (SQLiteConnection connect = GetConnection())
            {
                using (SQLiteCommand fmd = connect.CreateCommand())
                {
                    fmd.CommandText = @"SELECT id, name, regionid, mass, radius, orbitalradius, translationX, translationY, translationZ, globaltranslationX, globaltranslationY, globaltranslationZ FROM worlds WHERE parentid = @parentid";
                    fmd.CommandType = System.Data.CommandType.Text;
                    fmd.Parameters.Add(new SQLiteParameter(@"parentid", parentID));

                    SQLiteDataReader r = fmd.ExecuteReader();

                    while (r.Read())
                    {
                        WorldRecord record;
                        record.ID = Convert.ToString(r["id"]);
                        record.Name = Convert.ToString(r["name"]);
                        record.RegionID = Convert.ToString(r["regionid"]);
                        record.ParentID = parentID;
                        record.Mass = Convert.ToDouble(r["mass"]);
                        record.Radius = Convert.ToDouble(r["radius"]);
                        record.OrbitalRadius = Convert.ToDouble(r["orbitalradius"]);
                        Vector3d translation;
                        translation.x = Convert.ToDouble(r["translationX"]);
                        translation.y = Convert.ToDouble(r["translationY"]);
                        translation.z = Convert.ToDouble(r["translationZ"]);
                        record.Translation = translation;

                        translation.x = Convert.ToDouble(r["globaltranslationX"]);
                        translation.y = Convert.ToDouble(r["globaltranslationY"]);
                        translation.z = Convert.ToDouble(r["globaltranslationZ"]);
                        record.GlobalTranslation = translation;
                        worldList.Add(record);
                    }
                }
                connect.Close();
            }

            return worldList.ToArray();
        }

        public static VehicleRecord GetVehicleRecord(string ownerid)
        {
            VehicleRecord vehicle;
            vehicle.ID = null;
            vehicle.Name = null;
            vehicle.RegionID = null;
            vehicle.ParentID = null;
            vehicle.RelativeEntitySavePath = null;
            vehicle.OwnerID = null;
            vehicle.Translation = Vector3d.Zero();

            using (SQLiteConnection connect = GetConnection())
            {
                using (SQLiteCommand fmd = connect.CreateCommand())
                {
                    // TODO: include factionid field
                    fmd.CommandText = @"SELECT id, name, ownerid, parentid, prefab, regionid, translationX, translationY, translationZ FROM vehicles WHERE ownerid = @ownerid";
                    fmd.CommandType = System.Data.CommandType.Text;
                    fmd.Parameters.Add(new SQLiteParameter(@"ownerid", ownerid));

                    SQLiteDataReader r = fmd.ExecuteReader();

                    if (r.HasRows)
                    {
                        vehicle.ID = Convert.ToString(r["id"]);
                        vehicle.Name = Convert.ToString(r["name"]);
                        vehicle.RegionID = Convert.ToString(r["regionid"]);
                        vehicle.ParentID = Convert.ToString(r["parentid"]);
                        vehicle.RelativeEntitySavePath = Convert.ToString(r["prefab"]);
                        vehicle.OwnerID = ownerid;
                        Vector3d translation;
                        // NOTE: star translations are in global coords
                        translation.x = Convert.ToDouble(r["translationX"]);
                        translation.y = Convert.ToDouble(r["translationY"]);
                        translation.z = Convert.ToDouble(r["translationZ"]);
                        vehicle.Translation = translation;
                    }
                }
                connect.Close();
            }

            return vehicle;
        }

        public static CharacterRecord[] GetCharacterRecords(string parentid)
        {
            List<CharacterRecord> characterList = new List<CharacterRecord>();

            using (SQLiteConnection connect = GetConnection())
            {
                using (SQLiteCommand fmd = connect.CreateCommand())
                {
                    fmd.CommandText = @"SELECT id, firstname, prefab, translationX, translationY, translationZ FROM characters WHERE parentid = @parentid";
                    fmd.CommandType = System.Data.CommandType.Text;
                    fmd.Parameters.Add(new SQLiteParameter(@"parentid", parentid));

                    SQLiteDataReader r = fmd.ExecuteReader();

                    while (r.Read())
                    {
                        CharacterRecord record;
                        record.ID = Convert.ToString(r["id"]);
                        record.FirstName = Convert.ToString(r["firstname"]);
                        record.ParentID = parentid;
                        record.LastName = null;
                        record.RelativeEntitySavePath = Convert.ToString(r["prefab"]);
                        Vector3d translation;
                        translation.x = Convert.ToDouble(r["translationX"]);
                        translation.y = Convert.ToDouble(r["translationY"]);
                        translation.z = Convert.ToDouble(r["translationZ"]);
                        record.Translation = translation;
                        
                        characterList.Add(record);
                    }
                }
                connect.Close();
            }

            return characterList.ToArray();
        }

        public static Game01.GameObjects.NavPoint[] GetNavPoints(string vehicleID)
        {
            List<Game01.GameObjects.NavPoint> waypoints = new List<Game01.GameObjects.NavPoint>();

            using (SQLiteConnection connect = GetConnection())
            {
                using (SQLiteCommand fmd = connect.CreateCommand())
                {
                    fmd.CommandText = @"SELECT rowid, regionid, targetid, translationX, translationY, translationZ FROM waypoints";
                    fmd.CommandType = System.Data.CommandType.Text;

                    SQLiteDataReader r = fmd.ExecuteReader();

                    while (r.Read())
                    {
                        int temp = 0; // todo: do we need to retrieve a database index?
                        Game01.GameObjects.NavPoint record = new Game01.GameObjects.NavPoint(temp);
                        // do we need the database record index? 
                        // record.ID = Convert.ToInt64(r["id"]);
                        record.RowID = Convert.ToInt32(r["rowid"]);
                        record.RegionID = Convert.ToString(r["regionid"]);
                        record.TargetID = Convert.ToString(r["targetid"]);


                        Vector3d translation;
                        // NOTE: waypoint positions are in region coordinate space.
                        translation.x = Convert.ToDouble(r["translationX"]);
                        translation.y = Convert.ToDouble(r["translationY"]);
                        translation.z = Convert.ToDouble(r["translationZ"]);
                        record.Position = translation;

                        waypoints.Add(record);
                    }
                    connect.Close();
                }
            }

            return waypoints.ToArray();
        }
        #endregion

        #region Create


        public static void CreateTaskRecordClient(Game01.GameObjects.Order task, SQLiteConnection conn)
        {
            //SQLiteCommand insertSQL = new SQLiteCommand("INSERT INTO [tasks_client] ([id], [task], [status], [assigneddatetime], [assignedby], [args], [notes1]) VALUES (@id, @task, @status, @assigneddatetime, @assignedby, @args, @notes1)", conn);
            //insertSQL.Parameters.Add(new SQLiteParameter(@"id", task.ID));
            //insertSQL.Parameters.Add(new SQLiteParameter(@"task", task.Task));
            //insertSQL.Parameters.Add(new SQLiteParameter(@"status", task.Status));
            //insertSQL.Parameters.Add(new SQLiteParameter(@"assignedby", task.AssignedByID));
            //insertSQL.Parameters.Add(new SQLiteParameter(@"args", task.Args));
            //insertSQL.Parameters.Add(new SQLiteParameter(@"assigneddatetime", task.AssignedDateTime.ToBinary()));
            //insertSQL.Parameters.Add(new SQLiteParameter(@"notes1", task.Notes1));

            //try
            //{
            //    insertSQL.ExecuteNonQuery();
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception("AppDatabaseHelper.CreateStarRecord: ERROR - " + ex.Message);
            //}
        }

        public static void CreateTaskRecordServer(Game01.GameObjects.Order task, SQLiteConnection conn)
        {
            //SQLiteCommand insertSQL = new SQLiteCommand("INSERT INTO [tasks_server] ([id], [task], [status], [assigneddatetime], [assignedby], [args], [notes1]) VALUES (@id, @task, @status, @assigneddatetime, @assignedby, @args, @notes1)", conn);
            //insertSQL.Parameters.Add(new SQLiteParameter(@"id", task.ID));
            //insertSQL.Parameters.Add(new SQLiteParameter(@"task", task.Task));
            //insertSQL.Parameters.Add(new SQLiteParameter(@"status", task.Status));
            //insertSQL.Parameters.Add(new SQLiteParameter(@"assignedby", task.AssignedByID));
            //insertSQL.Parameters.Add(new SQLiteParameter(@"args", task.Args));
            //insertSQL.Parameters.Add(new SQLiteParameter(@"assigneddatetime", task.AssignedDateTime.ToBinary()));
            //insertSQL.Parameters.Add(new SQLiteParameter(@"notes1", task.Notes1));

            //try
            //{
            //    insertSQL.ExecuteNonQuery();
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception("AppDatabaseHelper.CreateStarRecord: ERROR - " + ex.Message);
            //}
        }

        public static void CreateStarRecord(Keystone.Celestial.Star star, SQLiteConnection conn)
        {
            SQLiteCommand insertSQL = new SQLiteCommand("INSERT INTO [stars] ([id], [typename], [name], [regionid], [mass], [radius], [translationX], [translationY], [translationZ], [globaltranslationX], [globaltranslationY], [globaltranslationZ]) VALUES (@id, @typename, @name, @regionid, @mass, @radius, @translationX, @translationY, @translationZ, @globaltranslationX, @globaltranslationY, @globaltranslationZ)", conn);
            insertSQL.Parameters.Add(new SQLiteParameter(@"id", star.ID));
            insertSQL.Parameters.Add(new SQLiteParameter(@"typename", star.TypeName));
            insertSQL.Parameters.Add(new SQLiteParameter(@"name", star.Name));
            insertSQL.Parameters.Add(new SQLiteParameter(@"regionid", star.Region.ID));
            insertSQL.Parameters.Add(new SQLiteParameter(@"mass", System.Data.DbType.Double) { Value = star.MassKg });
            insertSQL.Parameters.Add(new SQLiteParameter(@"radius", System.Data.DbType.Double) { Value = star.Radius });
            insertSQL.Parameters.Add(new SQLiteParameter(@"translationX", System.Data.DbType.Double) { Value = star.Translation.x });
            insertSQL.Parameters.Add(new SQLiteParameter(@"translationY", System.Data.DbType.Double) { Value = star.Translation.y });
            insertSQL.Parameters.Add(new SQLiteParameter(@"translationZ", System.Data.DbType.Double) { Value = star.Translation.z });
            insertSQL.Parameters.Add(new SQLiteParameter(@"globaltranslationX", System.Data.DbType.Double) { Value = star.GlobalTranslation.x });
            insertSQL.Parameters.Add(new SQLiteParameter(@"globaltranslationY", System.Data.DbType.Double) { Value = star.GlobalTranslation.y });
            insertSQL.Parameters.Add(new SQLiteParameter(@"globaltranslationZ", System.Data.DbType.Double) { Value = star.GlobalTranslation.z });
            try
            {
                insertSQL.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("AppDatabaseHelper.CreateStarRecord: ERROR - " + ex.Message);
            }
        }

        public static void CreateWorldRecord(Keystone.Celestial.World world, SQLiteConnection conn)
        {
            // http://stackoverflow.com/questions/1609637/is-it-possible-to-insert-multiple-rows-at-a-time-in-an-sqlite-database
            // TODO: we could try to generate a sqlite command string for batch insertion of rows because one at a time seems very slow
            // However, encapsulating the inserts within a single transaction speeds it up considerably.  It doesn't matter if the inserts
            // are even to two seperate tables.
            SQLiteCommand insertSQL = new SQLiteCommand("INSERT INTO [worlds] ([id], [typename], [name], [regionid], [parentid], [mass], [radius], [orbitalradius], [translationX], [translationY], [translationZ], [globaltranslationX], [globaltranslationY], [globaltranslationZ]) VALUES (@id, @typename, @name, @regionid, @parentid, @mass, @radius, @orbitalradius, @translationX, @translationY, @translationZ, @globaltranslationX, @globaltranslationY, @globaltranslationZ)", conn);
            insertSQL.Parameters.Add(new SQLiteParameter(@"id", world.ID));
            insertSQL.Parameters.Add(new SQLiteParameter(@"typename", world.TypeName));
            insertSQL.Parameters.Add(new SQLiteParameter(@"name", world.Name));
            insertSQL.Parameters.Add(new SQLiteParameter(@"regionid", world.Region.ID));
            insertSQL.Parameters.Add(new SQLiteParameter(@"parentid", world.Parent.ID));
            insertSQL.Parameters.Add(new SQLiteParameter(@"mass", System.Data.DbType.Double) { Value = world.MassKg });
            insertSQL.Parameters.Add(new SQLiteParameter(@"radius", System.Data.DbType.Double) { Value = world.Radius });
            insertSQL.Parameters.Add(new SQLiteParameter(@"orbitalradius", System.Data.DbType.Double) { Value = world.OrbitalRadius });
            insertSQL.Parameters.Add(new SQLiteParameter(@"translationX", System.Data.DbType.Double) { Value = world.Translation.x });
            insertSQL.Parameters.Add(new SQLiteParameter(@"translationY", System.Data.DbType.Double) { Value = world.Translation.y });
            insertSQL.Parameters.Add(new SQLiteParameter(@"translationZ", System.Data.DbType.Double) { Value = world.Translation.z });
            insertSQL.Parameters.Add(new SQLiteParameter(@"globaltranslationX", System.Data.DbType.Double) { Value = world.GlobalTranslation.x });
            insertSQL.Parameters.Add(new SQLiteParameter(@"globaltranslationY", System.Data.DbType.Double) { Value = world.GlobalTranslation.y });
            insertSQL.Parameters.Add(new SQLiteParameter(@"globaltranslationZ", System.Data.DbType.Double) { Value = world.GlobalTranslation.z });
            try
            {
                insertSQL.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("AppDatabaseHelper.CreateWorldRecord: ERROR - " + ex.Message);
            }
        }

        // todo: i should pass in a Game01.GameObjects.Vehicle and just use Keystone.Containers.Container ?
        public static void CreateVehicleRecord(string ownerid, Keystone.Vehicles.Vehicle vehicle, string regionID, string parentID, string prefab, SQLiteConnection conn)
        {
            SQLiteCommand insertSQL = new SQLiteCommand("INSERT INTO [vehicles] ([id], [ownerid], [typename], [regionid], [parentid], [prefab], [name], [translationX], [translationY], [translationZ]) VALUES (@id, @ownerid, @typename, @regionid, @parentid, @prefab, @name, @translationX, @translationY, @translationZ)", conn);
            insertSQL.Parameters.Add(new SQLiteParameter(@"id", vehicle.ID));
            insertSQL.Parameters.Add(new SQLiteParameter(@"typename", vehicle.TypeName));
            insertSQL.Parameters.Add(new SQLiteParameter(@"name", vehicle.Name));
            insertSQL.Parameters.Add(new SQLiteParameter(@"ownerid", ownerid));
            insertSQL.Parameters.Add(new SQLiteParameter(@"prefab", prefab));
            // Region.ID and Parent.ID are not set on initial vehicle creation.  Furthermore,
            // if we do keep these fields in the table, we need to update the record everytime the 
            // vehicle changes Zones and orbits a new world or de-orbits to Zone (aka: deep space)
            string regionid = regionID; // vehicle.Region.ID;
            string parentid = null;     // vehicle.Parent.ID;
            insertSQL.Parameters.Add(new SQLiteParameter(@"regionid", regionid));
            insertSQL.Parameters.Add(new SQLiteParameter(@"parentid", parentid));
            // TODO: I don't think this should store GlobalTranslation! it works so far with just one Zone, but will likely fails if more than that
            insertSQL.Parameters.Add(new SQLiteParameter(@"translationX", System.Data.DbType.Double) { Value = vehicle.GlobalTranslation.x });
            insertSQL.Parameters.Add(new SQLiteParameter(@"translationY", System.Data.DbType.Double) { Value = vehicle.GlobalTranslation.y });
            insertSQL.Parameters.Add(new SQLiteParameter(@"translationZ", System.Data.DbType.Double) { Value = vehicle.GlobalTranslation.z });
            try
            {
                insertSQL.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("AppDatabaseHelper.CreateVehicleRecord: ERROR - " + ex.Message);
            }
        }

        public static void CreateCharacterRecords(Keystone.Entities.BonedEntity[] characters, string parentID, string[] prefab)
        {
            // NOTE: We use a single transaction for each ExecuteNonQuery()
            using (SQLiteConnection connect = GetConnection())
            {
                using (var transaction = connect.BeginTransaction())
                {
                    SQLiteCommand insertSQL = new SQLiteCommand("INSERT INTO [characters] ([id], [parentid], [prefab], [firstname], [translationX], [translationY], [translationZ]) VALUES (@id, @parentid, @prefab, @firstname, @translationX, @translationY, @translationZ)", connect);

                    for (int i = 0; i < characters.Length; i++)
                    {                          
                        insertSQL.Parameters.Add(new SQLiteParameter(@"id", characters[i].ID)); ;
                        insertSQL.Parameters.Add(new SQLiteParameter(@"firstname", characters[i].Name));
                        insertSQL.Parameters.Add(new SQLiteParameter(@"prefab", prefab[i]));

                        insertSQL.Parameters.Add(new SQLiteParameter(@"parentid", parentID));
                        insertSQL.Parameters.Add(new SQLiteParameter(@"translationX", System.Data.DbType.Double) { Value = characters[i].Translation.x });
                        insertSQL.Parameters.Add(new SQLiteParameter(@"translationY", System.Data.DbType.Double) { Value = characters[i].Translation.y });
                        insertSQL.Parameters.Add(new SQLiteParameter(@"translationZ", System.Data.DbType.Double) { Value = characters[i].Translation.z });

                        try
                        {
                            insertSQL.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("AppDatabaseHelper.CreateCharacterRecords() - ERROR - " + ex.Message);
                        }
                    }
                    transaction.Commit();
                }
                
            }
        }

        public static void CreateWaypointRecord(Keystone.Entities.Entity entity, Game01.GameObjects.NavPoint navpoint)
        {
            using (SQLiteConnection connect = new SQLiteConnection(@"Data Source=" + SaveFullPath + ";Version=3;"))
            {
                connect.Open();

                Vector3d translation = navpoint.Position;
                string regionID = navpoint.RegionID;
                SQLiteCommand insertSQL = new SQLiteCommand("INSERT INTO [waypoints] ([ownerid], [regionid], [translationX], [translationY], [translationZ]) VALUES (@ownerid, @regionid, @translationX, @translationY, @translationZ)", connect);
                insertSQL.Parameters.Add(new SQLiteParameter(@"ownerid", entity.ID));
                insertSQL.Parameters.Add(new SQLiteParameter(@"regionid", regionID));
                insertSQL.Parameters.Add(new SQLiteParameter(@"translationX", System.Data.DbType.Double) { Value = translation.x });
                insertSQL.Parameters.Add(new SQLiteParameter(@"translationY", System.Data.DbType.Double) {Value = translation.y});
                insertSQL.Parameters.Add(new SQLiteParameter(@"translationZ", System.Data.DbType.Double) { Value = translation.z });
                try
                {
                    insertSQL.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("AppDatabaseHelper.CreateWaypointRecord: ERROR - " + ex.Message);
                }
                finally
                {
                    connect.Close();
                }
            }
        }

        #endregion

        #region UpdateRecord
        public static void UpdateVehicleRecord(string vehicleID, string regionID, Vector3d translation)
        {
            using (SQLiteConnection connect = GetConnection())
            {
                SQLiteCommand updateSQL = new SQLiteCommand("UPDATE [vehicles] SET regionid=:regionid, translationX=:translationX, translationY=:translationY, translationZ=:translationZ WHERE id=:vehicleID", connect);
                updateSQL.Parameters.Add(new SQLiteParameter(@"regionid", regionID));
                updateSQL.Parameters.Add(new SQLiteParameter(@"translationX", System.Data.DbType.Double) { Value = translation.x });
                updateSQL.Parameters.Add(new SQLiteParameter(@"translationY", System.Data.DbType.Double) { Value = translation.y });
                updateSQL.Parameters.Add(new SQLiteParameter(@"translationZ", System.Data.DbType.Double) { Value = translation.z });
                updateSQL.Parameters.Add(new SQLiteParameter(@"vehicleID", vehicleID));
                
                try
                {
                    updateSQL.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("AppDatabaseHelper.UpdateVehicleRecord(): ERROR - " + ex.Message);
                }
            }

        }

        public static void UpdateCharacterRecord(string characterID, Vector3d translation)
        {
            using (SQLiteConnection connect = GetConnection())
            {
                SQLiteCommand updateSQL = new SQLiteCommand("UPDATE [characters] SET translationX=:translationX, translationY=:translationY, translationZ=:translationZ WHERE id=:characterID", connect);
                updateSQL.Parameters.Add(new SQLiteParameter(@"translationX", System.Data.DbType.Double) { Value = translation.x });
                updateSQL.Parameters.Add(new SQLiteParameter(@"translationY", System.Data.DbType.Double) { Value = translation.y });
                updateSQL.Parameters.Add(new SQLiteParameter(@"translationZ", System.Data.DbType.Double) { Value = translation.z });
                updateSQL.Parameters.Add(new SQLiteParameter(@"characterID", characterID));

                try
                {
                    updateSQL.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("AppDatabaseHelper.UpdateCharacterRecord(): ERROR - " + ex.Message);
                }
            }

        }

        public static void UpdateCharacterRecords(string[] characterIDs, Vector3d[] translations)
        {
            using (SQLiteConnection connect = GetConnection())
            {
                using (var transaction = connect.BeginTransaction())
                {
                    SQLiteCommand updateSQL = new SQLiteCommand("UPDATE [characters] SET translationX=:translationX, translationY=:translationY, translationZ=:translationZ WHERE id=:characterID", connect);

                    for (int i = 0; i < characterIDs.Length; i++)
                    {
                        updateSQL.Parameters.Add(new SQLiteParameter(@"translationX", System.Data.DbType.Double) { Value = translations[i].x });
                        updateSQL.Parameters.Add(new SQLiteParameter(@"translationY", System.Data.DbType.Double) { Value = translations[i].y });
                        updateSQL.Parameters.Add(new SQLiteParameter(@"translationZ", System.Data.DbType.Double) { Value = translations[i].z });
                        updateSQL.Parameters.Add(new SQLiteParameter(@"characterID", characterIDs[i]));

                        try
                        {
                            updateSQL.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("AppDatabaseHelper.UpdateCharacterRecord(): ERROR - " + ex.Message);

                        }
                    }
                    transaction.Commit();
                }

            }
        }

        public static void Waypoint_UpdateRecord(string entityID, int rowid, Game01.GameObjects.NavPoint navpoint)
        {
            using (SQLiteConnection connect = new SQLiteConnection(@"Data Source=" + SaveFullPath + ";Version=3;"))
            {
                connect.Open();

                Vector3d translation = navpoint.Position;
                // TODO: do i want a "index" field for each waypoint?  Does that mean we need to shift all waypoint indices
                //       as our ship traverses waypoints during travel?  And what about old waypoints that show us our
                //       waypoint history?  we need an "active" boolean for each waypoint as well.  When we add a new
                //       waypoint, how do we increment that waypoint?  Or we can use the internal sql db's rowid
                //       which will always be unique but not necessarily sequential.
                SQLiteCommand updateSQL = new SQLiteCommand("UPDATE [waypoints] SET regionid=:regionid, translationX=:translationX, translationY=:translationY, translationZ=:translationZ WHERE rowid=:rowid", connect);
                updateSQL.Parameters.Add(new SQLiteParameter(@"regionid", navpoint.RegionID));
                updateSQL.Parameters.Add(new SQLiteParameter(@"translationX", System.Data.DbType.Double) { Value = translation.x });
                updateSQL.Parameters.Add(new SQLiteParameter(@"translationY", System.Data.DbType.Double) { Value = translation.y });
                updateSQL.Parameters.Add(new SQLiteParameter(@"translationZ", System.Data.DbType.Double) { Value = translation.z });
               // insertSQL.Parameters.Add(new SQLiteParameter(@"ownerID", entityID));
                updateSQL.Parameters.Add(new SQLiteParameter(@"rowid", System.Data.DbType.Int32) { Value = rowid });
                try
                {
                    updateSQL.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("AppDatabaseHelper.Waypoint_UpdateRecord: ERROR - " + ex.Message);
                }
                finally
                {
                    connect.Close();
                }
            }
        }

        #endregion

        #region Delete record
        public static void DeleteCharacterRecords(string[] characterIDs)
        {
            using (SQLiteConnection connect = GetConnection())
            {
                using (var transaction = connect.BeginTransaction())
                {
                    SQLiteCommand updateSQL = new SQLiteCommand("DELETE FROM [characters] WHERE id=:characterID", connect);

                    for (int i = 0; i < characterIDs.Length; i++)
                    {
                        updateSQL.Parameters.Add(new SQLiteParameter(@"characterID", characterIDs[i]));

                        try
                        {
                            updateSQL.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("AppDatabaseHelper.DeleteCharacterRecords(): ERROR - " + ex.Message);

                        }
                    }
                    transaction.Commit();
                }

            }
        }

        #endregion
    }
}
