using System.Collections.Generic;
using System.Configuration;
using Npgsql;
using KeyCommon.Persistence;
using KeyCommon.DatabaseEntities;
using System;
using System.Data;
using System.Diagnostics;

namespace KeyServerCommon
{
    public class PostgresProvider : IDisposable 
    {
        private string mConnectionString;
        private string mDatabaseAddress = "127.0.0.1";
        private int mDatabasePort = 5432;
        private string mDatabaseUserID = "projectevo";
        private string mDatabasePassword = "evoftw";
        private string mDatabasename = "ProjectEvo";
        private int mMaxConnections = 4;

        private Dictionary<int, NpgsqlTransaction> mTransactions;

        //Private mDBConnection As NpgsqlConnection

        //Pooling - True or False. Controls whether connection pooling is used. Default = True
        //MinPoolSize - Min size of connection pool. Min pool size, when specified, will make NpgsqlConnection
        //  pre-allocate the specified number of connections with the server. Default: 1
        //MaxPoolSize - Max size of connection pool. Pooled connections will be disposed of when returned to the pool 
        // if the pool contains more than this number of connections. Default: 20
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Database Name</param>
        /// <param name="address">Database IP address</param>
        /// <param name="port">Database port</param>
        /// <param name="user">Database User Name</param>
        /// <param name="password">Database Password</param>
        /// <remarks></remarks>
        public PostgresProvider(string name, string address, int port, string user, string password)
        {
            mTransactions = new Dictionary<int, NpgsqlTransaction>();

            mDatabasename = name;
            mDatabaseAddress = address;
            mDatabasePort = port;
            mDatabaseUserID = user;
            mDatabasePassword = password;

            // postgesql style connection string
            mConnectionString = string.Format("Server={0};Port={1};" + "User Id={2};Password={3};Database={4};", mDatabaseAddress, mDatabasePort, mDatabaseUserID, mDatabasePassword, mDatabasename);
        }

        /// <summary>
        /// Initialize with no ip and port... assumes loopback 127.0.0.1 and default port 5432
        /// </summary>
        private PostgresProvider()
        {
            mTransactions = new Dictionary<int, NpgsqlTransaction>();

            mConnectionString = string.Format("Server={0};Port={1};" + "User Id={2};Password={3};Database={4};", mDatabaseAddress, mDatabasePort, mDatabaseUserID, mDatabasePassword, mDatabasename);
        }

        #region IDisposable Members
        public void Dispose()
        {
            // TODO: any cleanup?
        }
        #endregion


        public string ConnectionString
        {
            get { return mConnectionString; }
        }
        
        public NpgsqlConnection GetTransactionConnection(int transactionID)
        {
            NpgsqlConnection connection = null;
            NpgsqlTransaction transaction;
            if (!mTransactions.TryGetValue(transactionID, out transaction))
            {
                connection = GetFreeDatabaseConnection();
            }
            else
            {
                connection = transaction.Connection;
            }
            return connection;
        }

        private NpgsqlConnection GetFreeDatabaseConnection()
        {
            try
            {
                NpgsqlConnection DBConnection = new NpgsqlConnection(mConnectionString);
                DBConnection.Open();
                return DBConnection;
            }

            catch (Exception msg)
            {
                // something went wrong, and you wanna know why
                System.Diagnostics.Trace.WriteLine(msg.ToString());
                Console.WriteLine("PostgresProvider.GetFreeDatabaseConnection() - Failed. " + msg.Message);
            }
            return null;
        }

        private void ReleaseConnection(NpgsqlConnection connection, int transactionID)
        {
            if (mTransactions.ContainsKey(transactionID))
            {
                return;
            }
            connection.Dispose();
        }

        public int BeginTransaction()
        {
            Npgsql.NpgsqlConnection connection = GetFreeDatabaseConnection();
            Npgsql.NpgsqlTransaction transaction;
            transaction = connection.BeginTransaction();

            int hash = transaction.GetHashCode();
            mTransactions.Add(hash, transaction);

            return hash;

        }

        public void EndTransaction(int transactionID)
        {
            Npgsql.NpgsqlTransaction transaction = mTransactions[transactionID];
            transaction.Commit();
            //transaction.Connection.Dispose()
            transaction.Dispose();
            mTransactions.Remove(transactionID);
        }

        public void UpdateRecord(string tableName, string pkeySequeneName, long recordID, string[] fieldNames, DbType[] fieldTypes, object[] fieldValues, int transactionID)
        {
            if ((fieldNames == null || fieldValues == null)) throw new ArgumentNullException();
            if ((fieldNames.Length != fieldValues.Length)) throw new ArgumentException();

            NpgsqlConnection connection = GetTransactionConnection(transactionID);

            int count;
            NpgsqlCommand sql = connection.CreateCommand();
            sql.CommandType = CommandType.Text;


            string fields = BuildFieldNamesString(fieldNames);
            fields = fields.Substring(0, fields.Length - 2);
            // NOTE: if this is resulting in a comma still at the end, then you probably forgot that VB array dimensions uses the value as upper bounds and not as the element count when buidling the field arrays

            sql.CommandText = "SELECT " + fields + " FROM " + tableName + " WHERE " + "id = " + recordID.ToString();
            count = fieldNames.Length;

            NpgsqlParameter[] @params = new NpgsqlParameter[count];
            for (int i = 0; i <count ; i++)
            {
                @params[i] = new NpgsqlParameter(fieldNames[i], fieldTypes[i]);
                @params[i].Direction = System.Data.ParameterDirection.Output;
                sql.Parameters.Add(@params[i]);
            }

            try
            {
                sql.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            // if the record doesnt exist, add it instead
            if (object.ReferenceEquals(@params[0].Value, DBNull.Value))
            {
                AddRecord(tableName, pkeySequeneName, fieldNames, fieldTypes, fieldValues, transactionID);
                return;
            }

            // still here, we can update the existing record
            count = fieldNames.Length;

            string sqlset = "";
            for (int i = 0; i < count; i++)
            {
                sqlset += fieldNames[i] + "=@f" + i.ToString() + ", ";
            }

            // remove last comma
            sqlset = sqlset.Substring (1, sqlset.Length - 2);

            sql.CommandText = "UPDATE " + tableName + " SET " + sqlset + " WHERE id  = " + recordID.ToString();

            for (int i = 0; i < count; i++)
            {
                NpgsqlParameter param = new NpgsqlParameter();
                param.ParameterName = "f" + i.ToString() + "";
                param.SourceColumn = fieldNames[i];
                param.Direction = System.Data.ParameterDirection.Input;
                param.DbType = fieldTypes[i];
                param.Value = fieldValues[i];
                sql.Parameters.Add(param);
            }

            try
            {
                object result = sql.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("PostgresProvider.UpdateRecord() - " + ex.Message);
                return;
            }
            finally
            {
                ReleaseConnection(connection, transactionID);
            }
        }

        // sequence id for games and any entity must start at 1
        public static long EMPTY_ID = 0;

        public void AddRecord(string tableName, string pkeySequeneName, string[] fieldNames, DbType[] fieldTypes, object[] fieldValues, int transactionID)
        {

            if (fieldNames == null || fieldValues == null) throw new ArgumentNullException();
            if (fieldNames.Length != fieldValues.Length) throw new ArgumentException();

            NpgsqlConnection connection = GetTransactionConnection(transactionID);

            int count = fieldNames.Length;

            NpgsqlCommand sql = connection.CreateCommand();
            sql.CommandType = CommandType.Text;

            bool usesSerialSequence = false;
            string sequence = "";
            // since this is a Enity based datastore, one thing we've done is require that the "id" be the first field 
            // however if a serial is used, then when we call BuildFieldNamesAndValuesString() below that field is removed
            // so that it will be auto-populated.  However if serial is used we'll also add a second sql at the end of our COmmandText
            // that will return the value of that serial field.  To do that though, we need to figure out the name of the seq 
            // NOTE: that this method of appending a second sql query is guaranteed to be atomic so we get the same id used
            // in the adding of the entity
            if (fieldNames[0] == "id")
            {
                long id;
                if (long.TryParse((string)fieldValues[0], out id) && id == EMPTY_ID)
                {
                    //sequence = tableName + "_" + fieldNames[0] + "_seq"
                    sequence = pkeySequeneName;
                    sequence = string.Format(";select currval('{0}')", sequence);
                    usesSerialSequence = true;
                }
            }
            else
            {
                throw new Exception("First field of an entity must always be the 'id' field and must be named 'id'");
            }


            // id field is usually autonumbered but currently for the Sims\Players table, 'id' is same as the user_id in Auth\Users and so are not unique in the Players table
            sql.CommandText = "INSERT INTO " + tableName + " " + BuildFieldNamesAndValuesString(fieldNames, fieldValues, usesSerialSequence);
            sql.CommandText += sequence;

            for (int i = 0; i < count ; i++)
            {
                NpgsqlParameter param = new NpgsqlParameter();

                if (usesSerialSequence && i == 0) continue;
                param.ParameterName = "fieldValues" + i.ToString() + "";
                param.SourceColumn = fieldNames[i];
                param.Direction = System.Data.ParameterDirection.Input;
                param.DbType = fieldTypes[i];
                param.Value = fieldValues[i];

                sql.Parameters.Add(param);
            }

            long result;
            // 18,446,744,073,709  ' our PrimaryKey uses a shared entity_seq that is 64 bits.  This allows for a unique entity id to be generated for
            // over 18 trillion games and assuming that there are 1 million unique units across all players in that game.
            // When max is reached, I believe postgresql automatically then wraps the sequence.  
            // NOTE:  you can configure the seq by adding either CYCLE or NO CYCLE option to the seq's SQL construction statement
            // however, i advise that you do not cycle, start the seq at 0 since we use -1 PrimaryKey to indicate no value
            if (usesSerialSequence)
            {
                // this will execute the version of the query taht does not include the primarykey which is auto numbered for us.  
                // it will then return the value of that auto generated primarykey so we can use it to fill in the PrimaryKey property of our entity object instance
                result = (long)sql.ExecuteScalar();
                Debug.Assert(result > 0);
                fieldValues[0] = result;
            }
            else
            {
                result = sql.ExecuteNonQuery();
                Debug.Assert(result == 1);
            }

            ReleaseConnection(connection, transactionID);
        }

        public void DeleteRecord(string tableName, long id, int transactionID)
        {
            NpgsqlConnection connection = GetTransactionConnection(transactionID);

            NpgsqlCommand sql = connection.CreateCommand();
            sql.CommandType = CommandType.Text;
            sql.CommandText = "DELETE FROM " + tableName + " WHERE id = " + id.ToString();

            int result = sql.ExecuteNonQuery();
            Debug.Assert(result == 1);

            ReleaseConnection(connection, transactionID);
        }


        public object RetreiveSingleRecord(EntityDefinition entityDef, long id, int transactionID)
        {
            return RetreiveSingleRecord(entityDef, "id", DbType.Int32, id, transactionID);
        }

        /// <summary>
        /// Retreives a single record matching a value on a specific search field.  Supports single table and multi-table queries via LEFT OUTER JOINs
        /// </summary>
        /// <param name="entityDef"></param>
        /// <param name="searchFieldName"></param>
        /// <param name="searchFieldType"></param>
        /// <param name="searchFieldValue"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public object RetreiveSingleRecord(EntityDefinition entityDef, string searchFieldName, DbType searchFieldType, object searchFieldValue, int transactionID)
        {

            if (entityDef == null) throw new ArgumentException();
            if (entityDef.TableDefinitions == null || entityDef.TableDefinitions.Count == 0) throw new ArgumentOutOfRangeException();

            NpgsqlConnection connection = GetTransactionConnection(transactionID);

            NpgsqlCommand sql = connection.CreateCommand();
            sql.CommandType = CommandType.Text;

            // TODO: ideally here, we'd modify things so we can potentially grab a cached copy of any/all ofa specific record
            // the cache could exist in the TableDef's as static collections

            // build the sql query starting with the field names
            string fields = BuildFieldNamesString(entityDef);
           // fields = fields.Substring (0, fields.Length - 2);

            // build the table names and joins
            string joins = "SELECT " + fields + " FROM ";
            if (entityDef.TableDefinitions.Count > 1)
            {
                for (int i = 0; i < entityDef.TableDefinitions.Count - 1; i++)
                {
                    joins += entityDef.TableDefinitions[i].TableName + " LEFT OUTER JOIN " + entityDef.TableDefinitions[i + 1].TableName + " ON " + GetFullyQualifiedName(entityDef.TableDefinitions[i], entityDef.TableDefinitions[i].JoinSourceFieldIndex) + " = " + GetFullyQualifiedName(entityDef.TableDefinitions[i + 1], entityDef.TableDefinitions[i].JoinTargetFieldIndex) + ";";
                }
            }
            else
            {
                // a regular sql with no join clause
                string value;
                // string types need to have the search value enclosed between single quotes
                if (searchFieldType == DbType.String)
                {
                    value = "'" + searchFieldValue.ToString() + "'";
                }
                else
                {
                    value = searchFieldValue.ToString();
                }
                joins += entityDef.TableDefinitions[0].TableName + " WHERE " + searchFieldName + " = " + value;
            }

            sql.CommandText = joins;

            // finally build the parameters from all tables 
            int tableCount = entityDef.TableDefinitions.Count;
            int paramsCount  = 0;

            for (int i = 0; i < tableCount ; i++)
            {
                paramsCount += entityDef.TableDefinitions[i].FieldsCount;
            }

            NpgsqlParameter[] @params = new NpgsqlParameter[paramsCount];
            paramsCount = 0;
            for (int i = 0; i < tableCount; i++)
            {
                int fieldCount = entityDef.TableDefinitions[i].FieldsCount;

                for (int j = 0; j < fieldCount; j++)
                {
                    @params[paramsCount] = new NpgsqlParameter(entityDef.TableDefinitions[i].FieldNames[j], entityDef.TableDefinitions[i].FieldTypes[j]);
                    @params[paramsCount].Direction = System.Data.ParameterDirection.Output;
                    //@params[paramsCount].SourceColumn = entityDef.TableDefinitions[i].FieldNames[j];
                    sql.Parameters.Add(@params[paramsCount]);
                    paramsCount++;
                }
            }

            try
            {
                // can use a ExecuteNonQuery since we're only retreiving a single record (othewise have to use .ExecuteReader)
                int result = sql.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                ReleaseConnection(connection, transactionID);
            }

            // if the record doesnt exist, nothing to return
            if (object.ReferenceEquals(@params[0].Value, DBNull.Value))
            {
                return false;
            }

            // fill the fieldvalues for each table def
            paramsCount = 0;
            for (int i = 0; i < entityDef.TableDefinitions.Count ; i++)
            {
                int count = entityDef.TableDefinitions[i].FieldsCount;
                //ReDim entityDef.TableDefinitions(i).FieldValues(entityDef.TableDefinitions(i).FieldsCount - 1)
                object[] values = new object[count];
                for (int j = 0; j < count; j++)
                {
                    values[j] = @params[paramsCount].Value;
                    paramsCount++;
                }
                entityDef.TableDefinitions[i].FieldValues.Add(values);
            }
            return true;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityDef"></param>
        /// <param name="searchFieldName"></param>
        /// <param name="searchFieldType"></param>
        /// <param name="searchFieldValue"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool RetreiveRecords(EntityDefinition entityDef, string searchFieldName, DbType searchFieldType, object searchFieldValue,int transactionID)
        {

            if (entityDef == null) throw new ArgumentException();
            if (entityDef.TableDefinitions == null || entityDef.TableDefinitions.Count == 0) throw new ArgumentOutOfRangeException();

            NpgsqlConnection connection = GetTransactionConnection(transactionID);
            NpgsqlCommand sql = connection.CreateCommand();
            sql.CommandType = CommandType.Text;

            // build the sql query starting with the field names
            string fields = BuildFieldNamesString(entityDef);
            fields = fields.Substring (0, fields.Length - 2);

            // buidl the table names and joins
            string joins = "SELECT " + fields + " FROM ";
            if (entityDef.TableDefinitions.Count > 1)
            {
                for (int i = 0; i < entityDef.TableDefinitions.Count - 1; i++)
                {
                    joins += entityDef.TableDefinitions[i].TableName + " LEFT OUTER JOIN " + entityDef.TableDefinitions[i + 1].TableName + " ON " + GetFullyQualifiedName(entityDef.TableDefinitions[i], entityDef.TableDefinitions[i].JoinSourceFieldIndex) + " = " + GetFullyQualifiedName(entityDef.TableDefinitions[i + 1], entityDef.TableDefinitions[i].JoinTargetFieldIndex) + ";";
                }
            }
            else
            {
                // a regular sql with no join clause
                string value;
                // string types need to have the search value enclosed between single quotes
                if (searchFieldType == DbType.String)
                {
                    value = "'" + searchFieldValue.ToString() + "'";
                }
                else
                {
                    value = searchFieldValue.ToString();
                }
                joins += entityDef.TableDefinitions[0].TableName + " WHERE " + searchFieldName + " = " + value;
            }

            sql.CommandText = joins;

            // finally build the parameters from all tables 
            int tableCount = entityDef.TableDefinitions.Count;
            int paramsCount=0;

            for (int i = 0; i < tableCount ; i++)
            {
                paramsCount += entityDef.TableDefinitions[i].FieldsCount;
            }

            NpgsqlParameter[] @params = new NpgsqlParameter[paramsCount - 1];
            paramsCount = 0;
            for (int i = 0; i < tableCount ; i++)
            {
                int fieldCount = entityDef.TableDefinitions[i].FieldsCount;

                for (int j = 0; j < fieldCount ; j++)
                {
                    @params[paramsCount] = new NpgsqlParameter(entityDef.TableDefinitions[i].FieldNames[j], entityDef.TableDefinitions[i].FieldTypes[j]);
                    @params[paramsCount].Direction = System.Data.ParameterDirection.Output;
                    sql.Parameters.Add(@params[paramsCount]);
                    paramsCount ++;
                }
            }

            // have to use reader for queries returning > 1 record.  Note that the reader
            // (can) continues to fill with values as we call reader.Read() and so there is no way
            // of knowing in advance exactly how many records are going to be retreived. 
            // That is why entityDef.TableDefintions(i).AddValues results in the values added to a List object
            try
            {
                using (Npgsql.NpgsqlDataReader reader = sql.ExecuteReader())
                {
                    int currentRecord = 0;
                    int currentField = 0;
                    while (reader.Read())
                    {
                        for (int i = 0; i < entityDef.TableDefinitions.Count ; i++)
                        {

                            object[] values = new object[entityDef.TableDefinitions[i].FieldsCount ];
                            for (int j = 0; j < entityDef.TableDefinitions[i].FieldsCount ; j++)
                            {
                                values[j] = reader[currentField];
                                currentField++;
                            }
                            entityDef.TableDefinitions[i].AddValues(values);
                        }
                        currentRecord++;
                        currentField = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                ReleaseConnection(connection, transactionID);
            }

            return true;
        }


        /// <summary>
        /// Used by INSERT command
        /// </summary>
        /// <param name="fieldNames"></param>
        /// <param name="fieldValues"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private string BuildFieldNamesAndValuesString(string[] fieldNames, object fieldValues, bool skipAutoSequencedID)
        {
            string values = "(";
            string columns = "(";
            int count = fieldNames.Length;

            // TODO: if this represents any kind of performance issue, we can try using stringBuilder
            for (int i = 0; i < count; i++)
            {
                if (skipAutoSequencedID && i == 0) continue;
                values += "@fieldValues" + i.ToString() + ", ";
                columns += fieldNames[i] + ", ";
            }

            // replace the last comma with a close paren
            values = values.Substring ( 1, values.Length - 2);
            values += ")";
            columns = columns.Substring ( 1, columns.Length - 2);
            columns += ")";

            return columns + " VALUES " + values;
        }

        public string BuildFieldNamesString(string[] fieldNames)
        {

            string columns = "(";
            int count = fieldNames.Length;

            // TODO: if this represents any kind of performance issue, we can try using stringBuilder
            for (int i = 0; i < count ; i++)
            {
                columns += fieldNames[i] + ", ";
            }

            // replace the last comma with a close paren
            columns = columns.Substring ( 1, columns.Length - 2);
            columns += ")";

            return columns;
        }

        /// <summary>
        /// Builds fully qualified (table.field) field names for the sql command text for multi-table join commands
        /// </summary>
        /// <param name="entitydef"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private string BuildFieldNamesString(EntityDefinition entitydef)
        {

            string columns = ""; // "(";

            // TODO: if this represents any kind of performance issue, we can try using stringBuilder
            for (int i = 0; i < entitydef.TableDefinitions.Count ; i++)
            {
                for (int j = 0; j < entitydef.TableDefinitions[i].FieldsCount; j++)
                {
                    columns += entitydef.TableDefinitions[i].TableName + "." + entitydef.TableDefinitions[i].FieldNames[j] + ", ";
                    //columns += entitydef.TableDefinitions[i].FieldNames[j] + ", ";
                }
            }
            // replace the last comma with a close paren
            columns = columns.Substring (0, columns.Length - 2);
            //columns += ")";

            return columns;
        }

        private string GetFullyQualifiedName(TableDefinition tableDef, int fieldIndex)
        {
            return tableDef.TableName + "." + tableDef.FieldNames[fieldIndex];
        }
    }
}
