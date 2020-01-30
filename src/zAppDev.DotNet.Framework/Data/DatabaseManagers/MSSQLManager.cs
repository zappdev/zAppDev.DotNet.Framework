using System.Data.Common;
using System.Data.SqlClient;
using zAppDev.DotNet.Framework.Data.DatabaseManagers.AccessLogManager;

namespace zAppDev.DotNet.Framework.Data.DatabaseManagers
{
    public class MSSQLManager : DBManager
    {
        public override DatabaseServerType DatabaseServerType { get; }

        public MSSQLManager(): base()
        {
            DatabaseServerType = DatabaseServerType.MSSQL;
        }

        public override void UpdateApplicationSettingsTable()
        {
            var disableAccessLogValue = AccessLogManagerUtilities.GetDisableAccessLogValue();
            if (!disableAccessLogValue.HasValue)
            {
                return;
            }

            var accessLogManager = new MSSQLAccessLogManager(this, disableAccessLogValue.Value);
            accessLogManager.Run();
        }

        public override DbConnectionStringBuilder GetConnectionStringBuilder(string connectionString)
        {
            return new SqlConnectionStringBuilder(connectionString);
        }

        public override string GetMasterConnectionString(ref string databaseName)
        {
            var connectionString = ConnectionString;

            var sqlConnectionStringBuilder = GetConnectionStringBuilder(connectionString) as SqlConnectionStringBuilder;

            databaseName = sqlConnectionStringBuilder.InitialCatalog;

            if (sqlConnectionStringBuilder.IntegratedSecurity == false)
            {
                sqlConnectionStringBuilder.PersistSecurityInfo = true;
            }

            sqlConnectionStringBuilder.Remove("Initial Catalog");

            return sqlConnectionStringBuilder.ConnectionString;
        }

        public override void CreateSchemas()
        {
            MiniSessionManager.ExecuteScript(@"IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'wf') EXEC('CREATE SCHEMA wf AUTHORIZATION [dbo]');
                                IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'security') EXEC('CREATE SCHEMA security AUTHORIZATION [dbo]');
                                IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'audit') EXEC('CREATE SCHEMA audit AUTHORIZATION [dbo]');");
        }

    }//end MSSQLManager
}
