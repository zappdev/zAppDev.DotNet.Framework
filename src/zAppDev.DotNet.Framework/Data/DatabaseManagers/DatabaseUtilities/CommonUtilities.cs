using Microsoft.Extensions.Configuration;
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

        public static DatabaseServerType GetDatabaseServerTypeFromConfiguration(IConfiguration configuration = null)
        {
#if NETFRAMEWORK
            var dbServerType = System.Configuration.ConfigurationManager.AppSettings["DatabaseServerType"];
#else
            var dbServerType = configuration["configuration:appSettings:add:DatabaseServerType:value"];
#endif
            if (string.IsNullOrEmpty(dbServerType)) return _defaultDatabaseServerType;

            dbServerType = dbServerType.Trim().ToLower();

            switch (dbServerType)
            {
                case "mssql":
                    return DatabaseServerType.MSSQL;
                case "mariadb":
                    return DatabaseServerType.MariaDB;
                default:
                    return _defaultDatabaseServerType;
            }
        }//end GetDatabaseServerTypeFromConfiguration()



#if NETFRAMEWORK

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
#else
        public static IDatabaseManager CreateDatabaseManager(IConfiguration configuration)
        {
            var databaseServerType = GetDatabaseServerTypeFromConfiguration(configuration);
            return CreateDatabaseManager(databaseServerType, configuration);
        }//end CreateDatabaseManager()

        public static IDatabaseManager CreateDatabaseManager(DatabaseServerType databaseServerType, IConfiguration configuration)
        {
            switch (databaseServerType)
            {
                case DatabaseServerType.MSSQL:
                    return new MSSQLManager(configuration);
                case DatabaseServerType.MariaDB:
                    return new MariaDBManager(configuration);
                default:
                    return null;
            }
        }//end CreateDatabaseManager()
#endif	



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

        public static string GetConnectionString(IConfiguration configuration = null)
        {

#if NETFRAMEWORK
            return System.Configuration.ConfigurationManager.ConnectionStrings["Database"].ConnectionString;
#else
            return configuration[$"configuration:connectionStrings:add:Database:connectionString"];
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
