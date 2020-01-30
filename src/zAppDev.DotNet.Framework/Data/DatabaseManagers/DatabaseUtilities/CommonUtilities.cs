using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using zAppDev.DotNet.Framework.Configuration;


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

        public static string GetConnectionString()
        {

#if NETFRAMEWORK
            return System.Configuration.ConfigurationManager.ConnectionStrings["Database"].ConnectionString;
#else
            var config = ConfigurationHandler.GetDatabaseSetting("Database");
            return config.ConnectionString;
#endif
        }

        public static int GetCommandTimeout(int? timeOut = null)
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

    }//end class
}//end namespace
