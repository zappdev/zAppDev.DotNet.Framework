using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
#if NETFRAMEWORK
using System.Web;
#else
using CLMS.Framework.Utilities;
#endif

namespace CLMS.Framework.Mvc
{
    internal class DatabaseMigrator
    {
        private List<string> _migrationFiles { get; set; }

        private readonly ILog _logger;

        public bool ShouldRun { get; private set; }


        public DatabaseMigrator()
        {
            _migrationFiles = new List<string>();
            _logger = LogManager.GetLogger(this.GetType());
            LoadMigrations();
        }

        public bool CanConnectToDatabase()
        {
            var databaseName = "";

            try
            {
                var connectionString = GetConnectionString(false, ref databaseName);
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    _logger.Error("Failed to get the Connection String from the web.config of the Application!");
                    return false;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Caught a {e.GetType()} exception while testing the Connection to the Database. Message: {e.Message}\r\nStackTrace:{e.StackTrace}");
                return false;
            }
        }
        private void LoadMigrations()
        {
            ShouldRun = false;
#if NETFRAMEWORK
            var _migrationsDirectory = HttpContext.Current.Server.MapPath("~/App_Data/Migrations");
#else
            var _migrationsDirectory = Web.MapPath("~/App_Data/Migrations");
#endif

            if (!Directory.Exists(_migrationsDirectory))
            {
                return;
            }

            var migrationFiles = Directory.GetFiles(_migrationsDirectory, "*.sql", SearchOption.TopDirectoryOnly).ToList();

            if (!migrationFiles.Any())
            {
                return;
            }

            _migrationFiles = migrationFiles.OrderBy(x => new Version(Path.GetFileNameWithoutExtension(x))).ToList();

            ShouldRun = true;
        }

        private string GetConnectionString(bool asMaster, ref string databaseName)
        {
            try
            {
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Database"]?.ConnectionString;
                if (string.IsNullOrWhiteSpace(connectionString)) return null;

                if (!asMaster) return connectionString;

                var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

                databaseName = sqlConnectionStringBuilder.InitialCatalog;

                if (sqlConnectionStringBuilder.IntegratedSecurity == false)
                {
                    sqlConnectionStringBuilder.PersistSecurityInfo = true;
                }

                sqlConnectionStringBuilder.Remove("Initial Catalog");


                return sqlConnectionStringBuilder.ConnectionString;
            }
            catch (Exception e)
            {
                _logger.Error($"Caught a {e.GetType()} exception while retrieving the Connection String from the web.config. Message: {e.Message}\r\nStackTrace:{e.StackTrace}");
                return null;
            }
        }

        private void CreateDatabaseIfNotExists()
        {
            var databaseName = "";
            var connectionString = GetConnectionString(true, ref databaseName);

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = connection.CreateCommand())
					{
						command.Connection = connection;
						command.CommandType = CommandType.Text;
						command.CommandText = 
							$@"
								IF NOT EXISTS (SELECT TOP 1 1 FROM master.sys.databases WHERE name = N'{databaseName}')
									CREATE DATABASE [{databaseName}];
							";
						command.ExecuteNonQuery();
					}
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Caught a [{ex.GetType()}] Exception while checking or creating the [{databaseName}] Database: {ex.Message}");
                throw ex;
            }
        }

        private static bool IgnoreSQLString(string sqlString)
        {
            return (string.IsNullOrWhiteSpace(sqlString) || string.IsNullOrEmpty(sqlString));
        }

        public static List<string> ToBatches(string sqlcmd, bool removePrints = true)
        {
            var commands = sqlcmd.Split(new [] { "GO\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            var result = new List<string>();

            foreach (var command in commands)
            {
                var trimmedCommand = command.Trim();
                if (IgnoreSQLString(trimmedCommand)) continue;

                if (removePrints)
                {
                    if (trimmedCommand.StartsWith("PRINT", StringComparison.OrdinalIgnoreCase))
                    {
                        var firstSemicolon = trimmedCommand.IndexOf(';');
                        if (firstSemicolon > -1)
                        {
                            trimmedCommand = trimmedCommand.Remove(0, firstSemicolon + 1);
                        }
                    }
                }

                if (IgnoreSQLString(trimmedCommand)) continue;
                result.Add(trimmedCommand);
            }
            return result;
        }

        public bool Run()
        {
            //CreateDatabaseIfNotExists();

            var databaseName = "";
            var connectionString = GetConnectionString(false, ref databaseName);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    var shortGUI =
                        new Regex("[^a-zA-Z0-9]").Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "");
                    var transcationName = $"T{shortGUI}";
                    using (
                        var transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted,
                            transcationName))
                    {

                        //transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted, transcationName);

                        command.Connection = connection;
                        command.Transaction = transaction;
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 600; //let's try it out with 10 minutes timeout.

                        try
                        {
                            command.CommandText =
                                $@"
                        IF EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Migrations')
                        BEGIN
                            SELECT TOP 1 MigrationVersion
	                        FROM Migrations 
	                        ORDER BY CAST('/' + MigrationVersion + '/' AS HIERARCHYID) DESC
                        END;                                               
                    ";

                            var highestAppliedVersionString = "0.0.0.0";
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    var result = reader.GetString(0);
                                    if (!string.IsNullOrWhiteSpace(result)) highestAppliedVersionString = result;
                                }
                            }
                            //reader.Close();


                            var highestAppliedVersion = new Version(highestAppliedVersionString);
                            var ranMigrations = new List<string>();

                            foreach (var migrationFile in _migrationFiles)
                            {
                                var currentMigrationVersionString = Path.GetFileNameWithoutExtension(migrationFile);
                                var currentMigrationVersion = new Version(currentMigrationVersionString);

                                if (currentMigrationVersion > highestAppliedVersion)
                                {
                                    var migrationScript = File.ReadAllText(migrationFile);

                                    var migrationScriptCommands = ToBatches(migrationScript);
                                    foreach (var migrationScriptCommand in migrationScriptCommands)
                                    {
                                        if (IgnoreSQLString(migrationScriptCommand)) continue;
                                        command.CommandText = migrationScriptCommand;
                                        command.ExecuteNonQuery();
                                    }

                                    command.CommandText =
                                        $"INSERT INTO Migrations VALUES ('{currentMigrationVersionString}', GETDATE())";
                                    command.ExecuteNonQuery();
                                    ranMigrations.Add(currentMigrationVersionString);
                                }
                            }

                            if (ranMigrations.Any())
                                _logger.Info($"Committing Database Migrations: [{string.Join(", ", ranMigrations)}]");

                            transaction.Commit();
                            if (ranMigrations.Any())
                                _logger.Info("Database Migrations have been successfully Commited");
                            else
                                _logger.Info("Found no pending Database Migrations");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(
                                $"Caught a [{ex.GetType()}] Exception while Committing the DB Migration Scripts: {ex.Message}");

                            try
                            {
                                _logger.Info($"Rolling back!");
                                transaction.Rollback();
                            }
                            catch (Exception ex2)
                            {
                                _logger.Error(
                                    $"Caught a [{ex2.GetType()}] Exception while trying to Rollback: {ex2.Message}");

                                return false;
                            }
                            return false;
                        }
                    }
                }
            }
        }		
    }
}