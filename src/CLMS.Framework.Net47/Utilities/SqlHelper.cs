using System.Collections.Generic;
using NHibernate;

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

            if (int.TryParse(System.Configuration.ConfigurationManager.AppSettings["SQLQueryTimeoutInSeconds"], out int commandTimeout))
            {
                return commandTimeout;
            }

            //Return the default value, in seconds
            return 30;
        }

        public static List<Dictionary<string, object>> RunSqlQuery(string query, Dictionary<string, object> parameters = null, int? timeOut = null)
        {
            var results = new List<Dictionary<string, object>>();

            using (var conn = new System.Data.SqlClient.SqlConnection(GetConnectionString()))
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

                    using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var result = new Dictionary<string, object>();

                            for (int i = 0; i < rdr.FieldCount; i++)
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

        public static List<Dictionary<string, object>> RunStoredProcedure(ISession session, string procedureName, Dictionary<string, object> parameters = null)
        {
            var results = new List<Dictionary<string, object>>();

            var conn = session.Connection as System.Data.SqlClient.SqlConnection;
            //using (var conn = new System.Data.SqlClient.SqlConnection(connectionString))
            using (var command = new System.Data.SqlClient.SqlCommand(procedureName, conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            })
            {
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        command.Parameters.Add(new System.Data.SqlClient.SqlParameter(p.Key, p.Value));
                    }
                }

                using (System.Data.SqlClient.SqlDataReader rdr = command.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var result = new Dictionary<string, object>();

                        for (int i = 0; i < rdr.FieldCount; i++)
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
