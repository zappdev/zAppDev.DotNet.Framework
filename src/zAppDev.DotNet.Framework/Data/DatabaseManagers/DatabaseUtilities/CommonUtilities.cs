using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Data.SqlClient;

namespace zAppDev.DotNet.Framework.Data.DatabaseManagers.DatabaseUtilities
{
    public class CommonUtilities
    {
        private static readonly DatabaseServerType _defaultDatabaseServerType = DatabaseServerType.MSSQL;

        public static DatabaseServerType GetDatabaseServerTypeFromConfiguration()
        {
            var configuration = System.Configuration.ConfigurationManager.AppSettings["DatabaseServerType"];
            if (string.IsNullOrEmpty(configuration)) return _defaultDatabaseServerType;

            configuration = configuration.Trim().ToLower();

            switch (configuration)
            {
                case "mssql":
                    return DatabaseServerType.MSSQL;
                case "mariadb":
                    return DatabaseServerType.MariaDB;
                default:
                    return _defaultDatabaseServerType;
            }
        }//end GetDatabaseServerTypeFromConfiguration()


        public static IDatabaseManager CreateDatabaseManager()
        {
            var databaseServerType = GetDatabaseServerTypeFromConfiguration();
            return CreateDatabaseManager(databaseServerType);
        }//end CreateDatabaseManager()

        public static IDatabaseManager CreateDatabaseManager(DatabaseServerType databaseServerType)
        {
            switch (databaseServerType)
            {
                case DatabaseServerType.MSSQL:
                    return new MSSQLManager();
                case DatabaseServerType.MariaDB:
                    return new MariaDBManager();
                default:
                    return null;
            }
        }//end CreateDatabaseManager()

        public static DbCommand GetDbCommand(IDatabaseManager databaseManager, string command)
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

        public static DbCommand GetDbCommand(DbConnection connection, DbTransaction transaction = null)
        {
            DbCommand command = connection.CreateCommand();
            command.Connection = connection;
            if (transaction != null) command.Transaction = transaction;
            return command;
        }//end GetDbCommand()

        public static DbCommand GetDbCommand(IDatabaseManager databaseManager, DbConnection dbConnection, string command)
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

        public static bool ReadCommandGetIsTrue(DbCommand command)
        {
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return reader.GetInt32(0) == 1;
                }
            }
            return false;
        }



        public static DbConnection GetDatabaseConnection(IDatabaseManager databaseManager, string connectionString)
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

    }//end class
}//end namespace
