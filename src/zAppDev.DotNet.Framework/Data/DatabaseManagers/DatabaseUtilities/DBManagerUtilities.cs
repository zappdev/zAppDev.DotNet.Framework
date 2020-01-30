using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Data.SqlClient;

namespace zAppDev.DotNet.Framework.Data.DatabaseManagers.DatabaseUtilities
{
    public static class DBManagerUtilities
    {
        public static DbParameter GetDbParameter(this IDatabaseManager databaseManager, string parameterName, object value)
        {
            switch (databaseManager.DatabaseServerType)
            {
                case DatabaseServerType.MSSQL:
                    return new SqlParameter(parameterName, value);
                case DatabaseServerType.MariaDB:
                    return new MySqlParameter(parameterName, value);
                default:
                    return null;
            }
        }//end GetDbParameter()

        public static DbCommand GetDbCommand(this IDatabaseManager databaseManager, string command)
        {
            switch (databaseManager.DatabaseServerType)
            {
                case DatabaseServerType.MSSQL:
                    return new SqlCommand(command);
                case DatabaseServerType.MariaDB:
                    return new MySqlCommand(command);
                default:
                    return null;
            }
        }//end GetDbCommand()

        public static DbCommand GetDbCommand(this IDatabaseManager databaseManager, DbConnection connection, DbTransaction transaction = null)
        {
            DbCommand command = connection.CreateCommand();
            command.Connection = connection;
            if (transaction != null) command.Transaction = transaction;
            return command;
        }//end GetDbCommand()

        public static DbCommand GetDbCommand(this IDatabaseManager databaseManager, DbConnection dbConnection, string command)
        {
            switch (databaseManager.DatabaseServerType)
            {
                case DatabaseServerType.MSSQL:
                    return new SqlCommand(command, dbConnection as SqlConnection);
                case DatabaseServerType.MariaDB:
                    return new MySqlCommand(command, dbConnection as MySqlConnection);
                default:
                    return null;
            }
        }//end GetDbCommand()

        public static DbConnection GetDatabaseConnection(this IDatabaseManager databaseManager, string connectionString)
        {
            switch (databaseManager.DatabaseServerType)
            {
                case DatabaseServerType.MSSQL:
                    return new SqlConnection(connectionString);
                case DatabaseServerType.MariaDB:
                    return new MySqlConnection(connectionString);
                default:
                    return null;
            }
        }//end GetDatabaseConnection()

        public static string ToProcedureConnectionString(this IDatabaseManager databaseManager, string connectionString)
        {
            if(databaseManager.DatabaseServerType == DatabaseServerType.MariaDB)
            {
                var builder = new MySqlConnectionStringBuilder(connectionString);
                builder.Database = builder.Database.ToLower();
                return builder.ConnectionString;
            }

            return connectionString;
        }//end ToProcedureConnectionString()

    }//end class
}//end namespace
