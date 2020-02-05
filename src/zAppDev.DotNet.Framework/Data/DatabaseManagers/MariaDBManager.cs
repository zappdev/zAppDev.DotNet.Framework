using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Data.Common;
using zAppDev.DotNet.Framework.Data.DatabaseManagers.AccessLogManager;

namespace zAppDev.DotNet.Framework.Data.DatabaseManagers
{
    public class MariaDBManager : DBManager
    {
        public override DatabaseServerType DatabaseServerType { get; }

#if NETFRAMEWORK
        public MariaDBManager():base()
        {
            DatabaseServerType = DatabaseServerType.MariaDB;
        }
#else
        public MariaDBManager(IConfiguration configuration = null) : base(configuration)
        {
            DatabaseServerType = DatabaseServerType.MariaDB;
        }
#endif	



        public override void UpdateApplicationSettingsTable()
        {
            return;
            var disableAccessLogValue = AccessLogManagerUtilities.GetDisableAccessLogValue();
            if (!disableAccessLogValue.HasValue)
            {
                return;
            }

            var accessLogManager = new MariaDBAccessLogManager(this, disableAccessLogValue.Value);
            accessLogManager.Run();
        }

        public override DbConnectionStringBuilder GetConnectionStringBuilder(string connectionString)
        {
            return new MySqlConnectionStringBuilder(connectionString);
        }

        public override string GetMasterConnectionString(ref string databaseName)
        {
            var sqlConnectionStringBuilder = GetConnectionStringBuilder(ConnectionString) as MySqlConnectionStringBuilder;

            databaseName = sqlConnectionStringBuilder.Database;

            if (sqlConnectionStringBuilder.IntegratedSecurity == false)
            {
                sqlConnectionStringBuilder.PersistSecurityInfo = true;
            }

            sqlConnectionStringBuilder.Remove("Database");

            return sqlConnectionStringBuilder.ConnectionString;
        }

        public override void RemoveSchemas(NHibernate.Cfg.Configuration configuration)
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

        public override void CreateSchemas()
        {
            return;
        }
    }
}
