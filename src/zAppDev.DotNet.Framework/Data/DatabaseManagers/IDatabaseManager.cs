using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Data.DatabaseManagers.AccessLogManager;

namespace zAppDev.DotNet.Framework.Data.DatabaseManagers
{
    public enum DatabaseServerType
    {
        None,
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
        void ExportDBSchema(NHibernate.Cfg.Configuration nHibernateConfiguration);

        DatabaseServerType GetDatabaseServerType();

        Task<List<Dictionary<string, object>>> RunSqlQueryAsync(string query, Dictionary<string, object> parameters, string connectionString);

        Task<List<Dictionary<string, object>>> RunSqlQueryAsync(
            string query, Dictionary<string, object> parameters = null,
            int? timeOut = null, string connectionString = null,
            DatabaseServerType databaseServerType = DatabaseServerType.None);

        List<Dictionary<string, object>> RunSqlQuery(string query, Dictionary<string, object> parameters, string connectionString);
        List<Dictionary<string, object>> RunSqlQuery(string query, Dictionary<string, object> parameters = null, int? timeOut = null, string connectionString = null, DatabaseServerType databaseServerType = DatabaseServerType.None);
        List<T> RunSqlQuery<T>(string query, Dictionary<string, object> parameters = null, int? timeOut = null, string connectionString = null);
        List<Dictionary<string, object>> RunStoredProcedure(string procedureName, string connectionString);
        List<Dictionary<string, object>> RunStoredProcedure(string procedureName, Dictionary<string, object> parameters = null, string connectionString = null, List<string> outParams = null);
    }
}