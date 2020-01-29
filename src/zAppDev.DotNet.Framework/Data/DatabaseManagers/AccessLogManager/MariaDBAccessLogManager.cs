// Copyright (c) 2020 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using log4net;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using zAppDev.DotNet.Framework.Data.DatabaseManagers.DatabaseUtilities;

namespace zAppDev.DotNet.Framework.Data.DatabaseManagers.AccessLogManager
{
    public class MariaDBAccessLogManager: IAccessLogManager
    {
        private bool DoDisable { get; set; }
        private readonly string _applicationSettingsKey = "OperationAccessLog";
        private readonly string _connectionString;
        private readonly string _databaseName;

        private readonly ILog _logger;

        public MariaDBAccessLogManager(IDatabaseManager databaseManager, bool doDisable)
        {
            _logger = LogManager.GetLogger(this.GetType());
            _connectionString = databaseManager.GetMasterConnectionString(ref _databaseName);
            DoDisable = doDisable;
        }

        public void Run()
        {
            var table = $"`{_databaseName}`.`ApplicationSettings`";

            if (string.IsNullOrWhiteSpace(_databaseName))
            {
                _logger.Error($@"Failed to get the Connection String from the web.config of the Application, in order to update the {table} table.
                                 The Access Log configuration will remain unchanged.");
                return;
            }

            var value = DoDisable ? 0 : 1;

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                try
                {
                    using (var command = CommonUtilities.GetDbCommand(connection, transaction))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 600;
                        command.CommandText = $"select 1 from {table} where `Key` = '{_applicationSettingsKey}' limit 1;";

                        var recordExists = CommonUtilities.ReadCommandGetIsTrue(command);
                        if (recordExists)
                        {
                            command.CommandText =
                                $@"
                                update {table} set
                                    `value` = {value}
                                where `key` = '{_applicationSettingsKey}';
                            ";
                        }
                        else
                        {
                            command.CommandText = $"select ifnull(max(Id), 0) + 1 from {table}";
                            var nextId = 1;
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    nextId = reader.GetInt32(0);
                                }
                            }

                            command.CommandText =
                                $@"
                                    insert into {table}
                                    (
                                        `Id`, 
                                        `Key`, 
                                        `Value`
                                    )
                                    values
                                    (
                                        {nextId}, 
                                        '{_applicationSettingsKey}', 
                                        {value}
                                    );                                
                                ";
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        var verb = DoDisable ? "disabled" : "enabled";
                        _logger.Info($"Successfully {verb} the Access Log");
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.Error(
                        $@"Caught a [{ex.GetType()}] Exception while updating the {table} table.
                           The Access Log configuration will remain unchanged. 
                           Exception: {ex.Message}"
                    );
                }
            }
        }//end UpdateApplicationSettingsTable()
    }//end AccessLogManager()
}

