using log4net;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data.Common;
using zAppDev.DotNet.Framework.Data.DatabaseManagers.AccessLogManager;

namespace zAppDev.DotNet.Framework.Data.DatabaseManagers
{
    public class MariaDBManager : IDatabaseManager
    {
        public DatabaseServerType DatabaseServerType { get; }

        public MariaDBManager()
        {
            DatabaseServerType = DatabaseServerType.MariaDB;
        }

        public void UpdateApplicationSettingsTable()
        {
            var disableAccessLogValue = AccessLogManagerUtilities.GetDisableAccessLogValue();
            if (!disableAccessLogValue.HasValue)
            {
                return;
            }

            var accessLogManager = new MariaDBAccessLogManager(this, disableAccessLogValue.Value);
            accessLogManager.Run();
        }

        public DbConnectionStringBuilder GetConnectionStringBuilder(string connectionString)
        {
            return new MySqlConnectionStringBuilder(connectionString);
        }

        public string GetMasterConnectionString(ref string databaseName)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Database"]?.ConnectionString;

            var sqlConnectionStringBuilder = GetConnectionStringBuilder(connectionString) as MySqlConnectionStringBuilder;

            databaseName = sqlConnectionStringBuilder.Database;

            if (sqlConnectionStringBuilder.IntegratedSecurity == false)
            {
                sqlConnectionStringBuilder.PersistSecurityInfo = true;
            }

            sqlConnectionStringBuilder.Remove("Database");

            return sqlConnectionStringBuilder.ConnectionString;
        }

        public void RemoveSchemas(NHibernate.Cfg.Configuration configuration)
        {
            foreach (var clsMapping in configuration.ClassMappings)
            {
                clsMapping.Table.Schema = null;
                if ((clsMapping as NHibernate.Mapping.RootClass) != null) (clsMapping as NHibernate.Mapping.RootClass).CacheRegionName = null;

                if (clsMapping.IdentityTable != null)
                {
                    clsMapping.IdentityTable.Schema = null;
                    var identifier = clsMapping.IdentityTable.IdentifierValue as NHibernate.Mapping.SimpleValue;
                    if (identifier != null)
                    {
                        if(identifier?.IdentifierGeneratorProperties?.ContainsKey("schema") == true)
                        {
                            identifier.IdentifierGeneratorProperties["schema"] = null;
                        }
                    }
                }
            }

            foreach (var colMapping in configuration.CollectionMappings)
            {
                colMapping.Table.Schema = null;
                if (colMapping.CollectionTable != null) colMapping.CollectionTable.Schema = null;
                colMapping.CacheRegionName = null;
            }
        }

        public void CreateSchemas()
        {
            return;
        }
    }
}
