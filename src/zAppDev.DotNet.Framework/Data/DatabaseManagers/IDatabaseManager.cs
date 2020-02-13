using System.Collections.Generic;
using System.Data.Common;
using zAppDev.DotNet.Framework.Data.DatabaseManagers.AccessLogManager;

namespace zAppDev.DotNet.Framework.Data.DatabaseManagers
{
    public enum DatabaseServerType
    {
        SQLite,
        MSSQL,
        MariaDB
    }

    public interface IDatabaseManager
    {
        string ConnectionString { get; }

        DatabaseServerType DatabaseServerType { get; }

        DbConnectionStringBuilder GetConnectionStringBuilder(string connectionString);
        string GetMasterConnectionString(ref string databaseName);

        void RemoveSchemas(NHibernate.Cfg.Configuration configuration);
        void UpdateApplicationSettingsTable();
        void CreateSchemas();

        DatabaseServerType GetDatabaseServerType();

        List<Dictionary<string, object>> RunSqlQuery(string query, Dictionary<string, object> parameters, string connectionString);
        List<Dictionary<string, object>> RunSqlQuery(string query, Dictionary<string, object> parameters = null, int? timeOut = null, string connectionString = null);
        List<T> RunSqlQuery<T>(string query, Dictionary<string, object> parameters = null, int? timeOut = null, string connectionString = null);
        List<Dictionary<string, object>> RunStoredProcedure(string procedureName, string connectionString);
        List<Dictionary<string, object>> RunStoredProcedure(string procedureName, Dictionary<string, object> parameters = null, string connectionString = null, List<string> outParams = null);
    }
}