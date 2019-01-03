using System;
using System.Collections.Generic;

namespace CLMS.Framework.Utilities
{
    public class SqlHelper
    {

        public static string GetConnectionString()
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["Database"].ConnectionString;
        }

        private static int GetCommandTimeout(int? timeOut = null)
        {
            if (timeOut.HasValue && timeOut.Value > -1)
            {
                return timeOut.Value;
            }
            var timeoutParam = System.Configuration.ConfigurationManager.AppSettings["SQLQueryTimeoutInSeconds"];
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
                connectionString = GetConnectionString();
            }

            using (var conn = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                conn.Open();

                using (var command = new System.Data.SqlClient.SqlCommand(query, conn)
                {
                    CommandType = System.Data.CommandType.Text,
                    CommandTimeout = GetCommandTimeout(timeOut)
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

        public static List<Dictionary<string, object>> RunStoredProcedureWithConnectionString(string procedureName, string connectionString)
        {
            return RunStoredProcedure(procedureName, null, connectionString);
        }

        public static List<Dictionary<string, object>> RunStoredProcedure(string procedureName, Dictionary<string, object> parameters = null, string connectionString = null)
        {
            var results = new List<Dictionary<string, object>>();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = GetConnectionString();
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
    }
}
