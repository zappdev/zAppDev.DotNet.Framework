﻿// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using zAppDev.DotNet.Framework.Configuration;
using zAppDev.DotNet.Framework.Data.DatabaseManagers.DatabaseUtilities;

namespace zAppDev.DotNet.Framework.Utilities
{
    public class SqlHelper
    {
        public static string _GetConnectionString()
        {
#if NETFRAMEWORK
            return System.Configuration.ConfigurationManager.ConnectionStrings["Database"].ConnectionString;
#else
            var config = ConfigurationHandler.GetDatabaseSetting("Database");
            return config.ConnectionString;
#endif
        }

        private static int _GetCommandTimeout(int? timeOut = null)
        {
            if (timeOut.HasValue && timeOut.Value > -1)
            {
                return timeOut.Value;
            }
#if NETFRAMEWORK
            var timeoutParam = System.Configuration.ConfigurationManager.AppSettings["SQLQueryTimeoutInSeconds"];
#else
            var timeoutParam = ConfigurationHandler.GetAppSetting("SQLQueryTimeoutInSeconds");
#endif

            return int.TryParse(timeoutParam, out var commandTimeout) ? commandTimeout : 30;
        }

        public static List<Dictionary<string, object>> RunSqlQuery(string query, Dictionary<string, object> parameters, string connectionString)
        {
            return RunSqlQuery(query, parameters, null, connectionString);
        }

        public static List<Dictionary<string, object>> RunSqlQuery(string query, Dictionary<string, object> parameters = null, int? timeOut = null, string connectionString = null)
        {
            var results = new List<Dictionary<string, object>>();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = CommonUtilities.GetConnectionString();
            }

            using (var conn = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                conn.Open();

                using (var command = new System.Data.SqlClient.SqlCommand(query, conn)
                {
                    CommandType = System.Data.CommandType.Text,
                    CommandTimeout = CommonUtilities.GetCommandTimeout(timeOut)
                })
                {
                    if (parameters != null)
                    {
                        foreach (var p in parameters)
                        {
                            command.Parameters.Add(new System.Data.SqlClient.SqlParameter(p.Key, p.Value));
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
        }

        public static List<T> RunSqlQuery<T>(string query, Dictionary<string, object> parameters = null, int? timeOut = null, string connectionString = null)
        {
            return new QueryResultSerializer<T>().Serialize(RunSqlQuery(query, parameters, timeOut, connectionString));
        }

        public static List<Dictionary<string, object>> RunStoredProcedureWithConnectionString(string procedureName, string connectionString)
        {
            return RunStoredProcedure(procedureName, null, connectionString);
        }

        public static List<Dictionary<string, object>> RunStoredProcedure(string procedureName, Dictionary<string, object> parameters = null, string connectionString = null, List<string> outParams = null)
        {
            var results = new List<Dictionary<string, object>>();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = CommonUtilities.GetConnectionString();
            }

            using (var conn = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                conn.Open();

                using (var command = new System.Data.SqlClient.SqlCommand(procedureName, conn) { CommandType = System.Data.CommandType.StoredProcedure })
                {
                    if (parameters != null)
                    {
                        foreach (var p in parameters)
                        {
                            var commandParam = new System.Data.SqlClient.SqlParameter(p.Key, p.Value);
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
        }
    }
}
