using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.Common;
using zAppDev.DotNet.Framework.Data.DatabaseManagers.DatabaseUtilities;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Data.DatabaseManagers
{
    public class DBManager : IDatabaseManager
    {
        public virtual string ConnectionString { get; }
        public virtual DatabaseServerType DatabaseServerType { get; }

#if NETFRAMEWORK
        public DBManager()
        {
            ConnectionString = CommonUtilities.GetConnectionString();
        }
#else
        protected readonly IConfiguration _configuration;

        public DBManager(IConfiguration configuration)
        {
            _configuration = configuration;
            ConnectionString = CommonUtilities.GetConnectionString(_configuration);
        }
#endif




        public virtual void UpdateApplicationSettingsTable() { }

        public virtual DbConnectionStringBuilder GetConnectionStringBuilder(string connectionString) { return null; }

        public virtual string GetMasterConnectionString(ref string databaseName) { return null; }

        public virtual void RemoveSchemas(NHibernate.Cfg.Configuration configuration) { }

        public virtual void CreateSchemas() { }

        //public virtual void ExportDBCreationSchema() { }

        public List<Dictionary<string, object>> RunSqlQuery(string query, Dictionary<string, object> parameters, string connectionString)
        {
            return RunSqlQuery(query, parameters, null, connectionString);
        }//end RunSqlQuery

        public List<Dictionary<string, object>> RunSqlQuery(string query, Dictionary<string, object> parameters = null, int? timeOut = null, string connectionString = null, DatabaseServerType databaseServerType = DatabaseServerType.None)
        {
            var results = new List<Dictionary<string, object>>();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = ConnectionString;
            }
            
            using (var conn = this.GetDatabaseConnection(connectionString))
            {
                conn.Open();
                
                using (var command = this.GetDbCommand(conn, query, databaseServerType))
                {
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandTimeout = CommonUtilities.GetCommandTimeout(timeOut);

                    if (parameters != null)
                    {
                        foreach (var p in parameters)
                        {
                            command.Parameters.Add(this.GetDbParameter(p.Key, p.Value));
                        }
                    }

                    using (var rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var result = new Dictionary<string, object>();

                            for (var i = 0; i < rdr.FieldCount; i++)
                            {
                                var columnName = rdr.GetName(i);
                                result.Add(columnName, rdr[columnName]);
                            }

                            results.Add(result);
                        }
                    }
                }

                return results;
            }
        }//end RunSqlQuery

        public List<T> RunSqlQuery<T>(string query, Dictionary<string, object> parameters = null, int? timeOut = null, string connectionString = null)
        {
            return new QueryResultSerializer<T>().Serialize(RunSqlQuery(query, parameters, timeOut, connectionString));
        }//end RunSqlQuery

        public List<Dictionary<string, object>> RunStoredProcedure(string procedureName, string connectionString)
        {
            return RunStoredProcedure(procedureName, null, connectionString);
        }//end RunStoredProcedure()

        public List<Dictionary<string, object>> RunStoredProcedure(string procedureName, Dictionary<string, object> parameters = null, string connectionString = null, List<string> outParams = null)
        {
            var results = new List<Dictionary<string, object>>();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = ConnectionString;
            }

            using (var conn = this.GetDatabaseConnection(connectionString))
            {
                conn.Open();

                using (var command = this.GetDbCommand(conn, procedureName))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    if (parameters != null)
                    {
                        foreach (var p in parameters)
                        {
                            var commandParam = this.GetDbParameter(p.Key, p.Value);
                            if (outParams != null)
                            {
                                if (outParams.Contains(p.Key))
                                {
                                    commandParam.Direction = System.Data.ParameterDirection.Output;
                                    commandParam.Size = 1000;
                                }
                            }
                            command.Parameters.Add(commandParam);
                        }
                    }

                    using (var rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var result = new Dictionary<string, object>();

                            for (var i = 0; i < rdr.FieldCount; i++)
                            {
                                var columnName = rdr.GetName(i);
                                result.Add(columnName, rdr[columnName]);
                            }

                            results.Add(result);
                        }
                    }

                    if (outParams != null)
                    {
                        foreach (var outP in outParams)
                        {
                            parameters[outP] = System.Convert.ToString(command.Parameters[outP].Value);
                        }
                    }
                }

                return results;
            }
        }//end RunStoredProcedure()

        public DatabaseServerType GetDatabaseServerType()
        {
            return DatabaseServerType;
        }

        public virtual void ExportDBSchema(NHibernate.Cfg.Configuration nHibernateConfiguration)
        {
            return;
        }
    }//end MSSQLManager
}
