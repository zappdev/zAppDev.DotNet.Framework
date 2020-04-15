using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Data.Common;
using System.IO;
using zAppDev.DotNet.Framework.Data.DatabaseManagers.AccessLogManager;

namespace zAppDev.DotNet.Framework.Data.DatabaseManagers
{
    public class MariaDBManager : DBManager
    {
        public static bool SchemaExported { get; private set; }

        private static bool? _exportMariaDBSchema { get; set; }


        public override void ExportDBSchema(NHibernate.Cfg.Configuration nHibernateConfiguration)
        {
            if (SchemaExported) return;

            if (!_exportMariaDBSchema.HasValue)
            {
                string exportMariaDBSchemaSetting = "";
#if NETFRAMEWORK
                exportMariaDBSchemaSetting = System.Configuration.ConfigurationManager.AppSettings["ExportMariaDBSchema"];
#else
                exportMariaDBSchemaSetting = _configuration["configuration:appSettings:add:ExportMariaDBSchema:value"];
#endif
                if(bool.TryParse(exportMariaDBSchemaSetting, out bool v))
                {
                    _exportMariaDBSchema = v;
                }
                else
                {
                    _exportMariaDBSchema = false;
                }
            }

            if(_exportMariaDBSchema.Value == true)
            {
                var updateCode = new System.Text.StringBuilder();
                var schemaUpdate = new SchemaUpdate(nHibernateConfiguration);

                var dbName = (GetConnectionStringBuilder(this.ConnectionString) as MySqlBaseConnectionStringBuilder).Database;
                if (!string.IsNullOrWhiteSpace(dbName))
                {
                    updateCode.AppendLine($"create database {dbName};");
                    updateCode.AppendLine($"use {dbName};");
                    updateCode.AppendLine();
                }

                schemaUpdate.Execute(row =>
                {
                    if ((!string.IsNullOrWhiteSpace(row) && (!row.EndsWith(";"))))
                    {
                        row += ";";
                    }
                    updateCode.AppendLine(row);
                    updateCode.AppendLine();
                }, false);

#if NETFRAMEWORK
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
#else
                var baseDirectory = Environment.GetEnvironmentVariable("ASPNETCORE_CONTENTROOT");
#endif
                var destination = Path.Combine(baseDirectory, Mvc.DatabaseMigrator.MigrationsFolder, "MariaDB");
                if (!Directory.Exists(destination)) Directory.CreateDirectory(destination);
                if (Directory.Exists(destination))
                {
                    File.WriteAllText(Path.Combine(destination, $"DBSchema_{DateTime.Now.ToString("yyyymmdd_HHmmss")}.mysql"), updateCode.ToString());
                    SchemaExported = true;
                }
            }
        }

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
