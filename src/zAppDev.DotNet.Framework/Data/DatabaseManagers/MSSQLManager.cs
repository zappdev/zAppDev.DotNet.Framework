using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using log4net;
using NHibernate.Cfg;
using zAppDev.DotNet.Framework.Data.DatabaseManagers.AccessLogManager;

namespace zAppDev.DotNet.Framework.Data.DatabaseManagers
{
    public class MSSQLManager : IDatabaseManager
    {
        public DatabaseServerType DatabaseServerType { get; }

        public MSSQLManager()
        {
            DatabaseServerType = DatabaseServerType.MSSQL;
        }

        public void UpdateApplicationSettingsTable()
        {
            var disableAccessLogValue = AccessLogManagerUtilities.GetDisableAccessLogValue();
            if (!disableAccessLogValue.HasValue)
            {
                return;
            }

            var accessLogManager = new MSSQLAccessLogManager(this, disableAccessLogValue.Value);
            accessLogManager.Run();
        }

        public DbConnectionStringBuilder GetConnectionStringBuilder(string connectionString)
        {
            return new SqlConnectionStringBuilder(connectionString);
        }

        public string GetMasterConnectionString(ref string databaseName)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Database"]?.ConnectionString;

            var sqlConnectionStringBuilder = GetConnectionStringBuilder(connectionString) as SqlConnectionStringBuilder;

            databaseName = sqlConnectionStringBuilder.InitialCatalog;

            if (sqlConnectionStringBuilder.IntegratedSecurity == false)
            {
                sqlConnectionStringBuilder.PersistSecurityInfo = true;
            }

            sqlConnectionStringBuilder.Remove("Initial Catalog");

            return sqlConnectionStringBuilder.ConnectionString;
        }

        public void RemoveSchemas(NHibernate.Cfg.Configuration configuration)
        {
            return;
        }

        public void CreateSchemas()
        {
            MiniSessionManager.ExecuteScript(@"IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'wf') EXEC('CREATE SCHEMA wf AUTHORIZATION [dbo]');
                                IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'security') EXEC('CREATE SCHEMA security AUTHORIZATION [dbo]');
                                IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'audit') EXEC('CREATE SCHEMA audit AUTHORIZATION [dbo]');");
        }
    }
}
