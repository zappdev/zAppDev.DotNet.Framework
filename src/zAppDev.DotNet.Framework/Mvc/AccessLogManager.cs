﻿// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using log4net;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using zAppDev.DotNet.Framework.Data.DatabaseManagers;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Mvc
{
    public class AccessLogManager
    {
        private bool DoDisable { get; set; }
        private readonly string _applicationSettingsKey = "OperationAccessLog";

        private readonly ILog _logger;

        public AccessLogManager()
        {
            _logger = LogManager.GetLogger(this.GetType());
        }

        public void Run()
        {
            var setting = ConfigurationManager.AppSettings["DisableAccessLog"];
            if (string.IsNullOrWhiteSpace(setting))
            {
                _logger.Warn("No {DisableAccessLog} found in configuration. Will not enable/disable the Access Log");
                return;
            }

            if(!bool.TryParse(setting, out var doDisable))
            {
                _logger.Warn("The value of {DisableAccessLog} is incorrect. The Access Log will be Enabled, by default");
            }

            this.DoDisable = doDisable;

            UpdateApplicationSettingsTable();
        }//end Run()

        private void UpdateApplicationSettingsTable()
        {
            var databaseName = "";

            var databaseManager = ServiceLocator.Current.GetInstance<IDatabaseManager>();
            var connectionString = databaseManager.GetMasterConnectionString(ref databaseName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.Error($@"Failed to get the Connection String from the web.config of the Application, in order to update the [{databaseName}].[security].[ApplicationSettings] table.
                                 The Access Log configuration will remain unchanged.");
                return;
            }

            //Queen-MariaDB TODO
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {

                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 600;

                    var table = $"[{databaseName}].[security].[ApplicationSettings]";
                    var value = DoDisable ? 0 : 1;

                    try
                    {
                        command.CommandText =
                            $@"
                        if not exists(select top 1 1 from {table}
                            where[Key] = '{_applicationSettingsKey}')
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
		                        '{_applicationSettingsKey}', 
		                        {value}
	                        )
                        end
                        else begin
                            update {table} set
                                [Value] = {value}
	                        where[Key] = '{_applicationSettingsKey}'
                        end;
                        ";

                        command.ExecuteNonQuery();
                        var verb = DoDisable ? "disabled" : "enabled";
                        _logger.Info($"Successfully {verb} the Access Log");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(
                            $@"Caught a [{ex.GetType()}] Exception while updating the [{databaseName}].[security].[ApplicationSettings] table.
                           The Access Log configuration will remain unchanged. 
                           Exception: {ex.Message}"
                        );
                    }
                }
            }
        }//end UpdateApplicationSettingsTable()
    }//end AccessLogManager()
}//end namespace