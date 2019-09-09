// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK

using zAppDev.DotNet.Framework.Data;
using zAppDev.DotNet.Framework.Data.DAL;
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Configuration;
using zAppDev.DotNet.Framework.Utilities;
using log4net;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements
{
    public class PerformanceMonitorManager
    {
        private static string _key = "PerformanceMonitor";

        private readonly ILog _logger;
        private readonly bool _isEnabled;

        public PerformanceMonitorManager(bool isEnabled = false)
        {
            _isEnabled = isEnabled;
            _logger = LogManager.GetLogger(this.GetType());
        }

        private string GetConnectionString(ref string databaseName)
        {
            try
            {
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Database"]?.ConnectionString;
                if (string.IsNullOrWhiteSpace(connectionString)) return null;
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
        }//end GetConnectionString()

        public void Update()
        {
            var databaseName = "";
            var connectionString = GetConnectionString(ref databaseName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.Error($@"Failed to get the Connection String from the web.config of the Application, in order to update the [{databaseName}].[security].[ApplicationSettings] table.
                                 The Performance Monitor configuration will remain unchanged.");
                return;
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 600;
                    var table = $"[{databaseName}].[security].[ApplicationSettings]";
                    var value = _isEnabled ? 1 : 0;
                    try
                    {
                        command.CommandText =
                            $@"
                        if not exists(select top 1 1 from {table}
                            where[Key] = '{_key}')
                        begin
                            insert into {table}
                            (
                                [Id], 
                                [Key], 
		                        [Value]
	                        )

                            values
                            (
		                        (select isnull(max(Id), 0)+1 from {table}), 
		                        '{_key}', 
		                        {value}
	                        )
                        end
                        else begin
                            update {table} set
                                [Value] = {value}
	                        where[Key] = '{_key}'
                        end;
                        ";
                        command.ExecuteNonQuery();
                        var verb = _isEnabled ? "enabled" : "disabled" ;
                        _logger.Info($"Successfully {verb} the Performance Monitor");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(
                            $@"Caught a [{ex.GetType()}] Exception while updating the [{databaseName}].[security].[ApplicationSettings] table.
                           The Performance Monitor configuration will remain unchanged. 
                           Exception: {ex.Message}"
                        );
                    }
                }
            }
        }//end Update()

        public static bool? IsEnabledByApplicationSetting()
        {
            using (var manager = new MiniSessionManager())
            {
                manager.OpenSessionWithTransaction();
                var repo = ServiceLocator.Current.GetInstance<IRepositoryBuilder>().CreateRetrieveRepository(manager);
                var setting = repo.Get<Identity.Model.ApplicationSetting>(s => s.Key == _key).FirstOrDefault();
                if (setting == null)
                {
                    return null;
                }
                return ((string.Compare(setting.Value, "true", true) == 0) || (string.Compare(setting.Value, "1", true) == 0));
            }
        } //end IsEnabledByApplicationSetting()

        public static bool IsEnabled()
        {
            var enabledByApplicationSetting = IsEnabledByApplicationSetting();
            if (enabledByApplicationSetting.HasValue && enabledByApplicationSetting.Value == false) return false;
            return ServiceLocator.Current.GetInstance<PerformanceMonitorConfiguration>()
                       ?.ControllerAction?.FrontEnd?.Enabled == true;
        }
    }
}
#endif