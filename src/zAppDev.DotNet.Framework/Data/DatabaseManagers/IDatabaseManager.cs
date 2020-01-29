using System.Data.Common;
using zAppDev.DotNet.Framework.Data.DatabaseManagers.AccessLogManager;

namespace zAppDev.DotNet.Framework.Data.DatabaseManagers
{
    public enum DatabaseServerType
    {
        MSSQL,
        MariaDB
    }

    public interface IDatabaseManager
    {
        DatabaseServerType DatabaseServerType { get; }

        DbConnectionStringBuilder GetConnectionStringBuilder(string connectionString);
        string GetMasterConnectionString(ref string databaseName);
        void RemoveSchemas(NHibernate.Cfg.Configuration configuration);
        void UpdateApplicationSettingsTable();
        void CreateSchemas();
    }
}