// Copyright (c) 2020 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using log4net;
using System;
using System.Data;
using System.Data.SqlClient;

namespace zAppDev.DotNet.Framework.Data.DatabaseManagers.AccessLogManager
{
    public class MSSQLAccessLogManager : IAccessLogManager
    {
        private bool DoDisable { get; set; }
        private readonly string _applicationSettingsKey = "OperationAccessLog";
        private readonly string _connectionString;
        private readonly string _databaseName;

        private readonly ILog _logger;

        public MSSQLAccessLogManager(IDatabaseManager databaseManager, bool doDisable)
        {
            _logger = LogManager.GetLogger(this.GetType());
            _connectionString = databaseManager.GetMasterConnectionString(ref _databaseName);
            DoDisable = doDisable;
        }

        public void Run()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                _logger.Error($@"Failed to get the Connection String from the web.config of the Application, in order to update the [{_databaseName}].[security].[ApplicationSettings] table.
                                 The Access Log configuration will remain unchanged.");
                return;
            }

            //Queen-MariaDB TODO
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {

                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 600;

                    var table = $"[{_databaseName}].[security].[ApplicationSettings]";
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
                            $@"Caught a [{ex.GetType()}] Exception while updating the [{_databaseName}].[security].[ApplicationSettings] table.
                           The Access Log configuration will remain unchanged. 
                           Exception: {ex.Message}"
                        );
                    }
                }
            }
        }//end UpdateApplicationSettingsTable()
    }//end AccessLogManager()
}

