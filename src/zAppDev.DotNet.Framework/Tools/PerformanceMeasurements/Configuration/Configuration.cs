// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Configuration
{
    #region Base Classes
    public class Enabled_Configuration
    {
        public bool Enabled { get; set; }
        
        public ILog GetLogger(bool specific = true, Type @for = null)
        {
#if NETFRAMEWORK //queen
            if (specific || @for == null) return LogManager.GetLogger(PerformanceMonitorConfiguration.LoggerName);
#else
            if (specific || @for == null) return LogManager.GetLogger(typeof(PerformanceMonitor));
#endif
            return LogManager.GetLogger(typeof(Type));
        }
    }
#endregion

#region Common Attributes, linked together

    public class MinimumMilliseconds_Enabled_Configuration : Enabled_Configuration
    {
        public long MinimumMilliseconds { get; set; }
    }

    public class IgnoreNulls_Enabled_Configuration : Enabled_Configuration
    {
        public bool IgnoreNulls { get; set; }
    }

    public class MinimumBytes_Enabled_Configuration : Enabled_Configuration
    {
        public long MinimumBytes { get; set; }
    }
#endregion


#region Simple Configurations
    public class FrontEndConfiguration : Enabled_Configuration { }
    public class SizeConfiguration : MinimumBytes_Enabled_Configuration { }
    public class TimeConfiguration : MinimumMilliseconds_Enabled_Configuration { }
    public class RAMConfiguration : MinimumBytes_Enabled_Configuration
    {
        public int IntervalMilliseconds { get; set; }
    }

    public class SessionConfiguration : IgnoreNulls_Enabled_Configuration {
        public List<string> MonitoredStatistics { get; set; }
        public SessionConfiguration()
        {
            MonitoredStatistics = new List<string>();
        }
    }

    public class EntitiesConfiguration : IgnoreNulls_Enabled_Configuration {
        public List<string> MonitoredStatistics { get; set; }
        public List<string> MonitoredEntities { get; set; }

        public EntitiesConfiguration()
        {
            MonitoredStatistics = new List<string>();
            MonitoredEntities = new List<string>();
        }
    }

    public class CPUConfiguration : Enabled_Configuration
    {
        public float MinimumPercentage { get; set; }

        public int IntervalMilliseconds { get; set; }
    }
#endregion

#region Complex Configurations
    public class DatabaseConfiguration : Enabled_Configuration
    {
        public SessionConfiguration Session { get; set; }

        public EntitiesConfiguration Entities { get; set; }

        public DatabaseConfiguration(DatabaseElement configuration = null)
        {
            Session = new SessionConfiguration();
            Entities = new EntitiesConfiguration();
        }
    }

    public class ClientDataConfiguration : Enabled_Configuration
    {
        public SizeConfiguration Size { get; set; }

        public TimeConfiguration Time { get; set; }

        public ClientDataConfiguration()
        {
            Size = new SizeConfiguration();
            Time = new TimeConfiguration();
        }
    }

    public class DBFlushConfiguration : Enabled_Configuration
    {
        public TimeConfiguration Time { get; set; }

        public DatabaseConfiguration Database { get; set; }

        public DBFlushConfiguration()
        {
            Time = new TimeConfiguration();
            Database = new DatabaseConfiguration();
        }
    }

    public class ActionConfiguration : Enabled_Configuration
    {
        public TimeConfiguration Time { get; set; }

        public CPUConfiguration CPU { get; set; }

        public RAMConfiguration RAM { get; set; }

        public DatabaseConfiguration Database { get; set; }

        public ActionConfiguration()
        {
            Time = new TimeConfiguration();
            CPU = new CPUConfiguration();
            RAM = new RAMConfiguration();
            Database = new DatabaseConfiguration();
        }
    }

    public class ControllerActionConfiguration : ActionConfiguration
    {
        public FrontEndConfiguration FrontEnd { get; set; }

        public ControllerActionConfiguration():base()
        {
            FrontEnd = new FrontEndConfiguration();
        }
    }

    public class ExposedAPIConfiguration : ActionConfiguration { }

    public class DTO2ViewModelConfiguration : Enabled_Configuration
    {
        public SizeConfiguration Size { get; set; }

        public TimeConfiguration Time { get; set; }

        public DatabaseConfiguration Database { get; set; }

        public DTO2ViewModelConfiguration()
        {
            Size = new SizeConfiguration();
            Time = new TimeConfiguration();
            Database = new DatabaseConfiguration();
        }
    }

    public class Model2DTOConfiguration : Enabled_Configuration
    {
        public SizeConfiguration Size { get; set; }

        public TimeConfiguration Time { get; set; }

        public Model2DTOConfiguration()
        {
            Size = new SizeConfiguration();
            Time = new TimeConfiguration();
        }
    }

    public class ConversionsConfiguration : Enabled_Configuration
    {
        public DTO2ViewModelConfiguration DTO2ViewModel { get; set; }

        public Model2DTOConfiguration Model2DTO { get; set; }

        public ConversionsConfiguration()
        {
            DTO2ViewModel = new DTO2ViewModelConfiguration();
            Model2DTO = new Model2DTOConfiguration();
        }
    }

    public class DataItemConfiguration: Enabled_Configuration
    {
        public SizeConfiguration Size { get; set; }

        public TimeConfiguration Time { get; set; }

        public DatabaseConfiguration Database { get; set; }

        public DataItemConfiguration()
        {
            Size = new SizeConfiguration();
            Time = new TimeConfiguration();
            Database = new DatabaseConfiguration();
        }
    }

    public class DataConfiguration: Enabled_Configuration
    {
        public DataItemConfiguration ClientData { get; set; }
        public DataItemConfiguration DTO2ViewModel { get; set; }
        public DataItemConfiguration Model2DTO { get; set; }

        public DataConfiguration()
        {
            ClientData = new DataItemConfiguration();
            DTO2ViewModel = new DataItemConfiguration();
            Model2DTO = new DataItemConfiguration();
        }
    }

#endregion

    public class PerformanceMonitorConfiguration: Enabled_Configuration
    {
        public static string LoggerName = "PerformanceMonitorLogger";

        public ControllerActionConfiguration ControllerAction { get; set; }

        public DataConfiguration DataConfiguration { get; set; }

        public DataItemConfiguration DBFlush { get; set; }

        public ExposedAPIConfiguration ExposedAPI { get; set; }

        private List<string> SplitAndClear(string toSplit, string applicationName = null)
        {
            var result = new List<string>();
            if (!string.IsNullOrWhiteSpace(toSplit))
            {
                result = toSplit.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            var boNameSpace = "";
            if (!string.IsNullOrWhiteSpace(applicationName))
            {
                boNameSpace = $"{applicationName}.BO.";
            }
            return result.Select(x => $"{boNameSpace}{x.Trim()}").ToList();
        }

        public PerformanceMonitorConfiguration(PerformanceMonitorConfigurationReader configuration = null, string applicationName = null)
        {
            ControllerAction = new ControllerActionConfiguration();
            DBFlush = new DataItemConfiguration();
            DataConfiguration = new DataConfiguration();
            ExposedAPI = new ExposedAPIConfiguration();

            if (configuration != null && configuration.Enabled == true)
            {
                this.Enabled = true;
                ControllerAction.Enabled = configuration.ControllerAction?.Enabled ?? false;
                if (ControllerAction.Enabled)
                {
                    ControllerAction.FrontEnd.Enabled = configuration.ControllerAction?.FrontEnd.Enabled ?? false; 

                    ControllerAction.Time.Enabled = configuration.ControllerAction?.Time?.Enabled ?? false;
                    if (ControllerAction.Time.Enabled)
                    {
                        ControllerAction.Time.MinimumMilliseconds = configuration.ControllerAction?.Time?.MinimumMilliseconds ?? 0;
                    }//end ControllerAction.Time

                    ControllerAction.CPU.Enabled = configuration.ControllerAction?.CPU?.Enabled ?? false;
                    if (ControllerAction.CPU.Enabled)
                    {
                        ControllerAction.CPU.IntervalMilliseconds = configuration.ControllerAction?.CPU?.IntervalMilliseconds ?? 1000;
                        ControllerAction.CPU.MinimumPercentage = configuration.ControllerAction?.CPU?.MinimumPercentage ?? 0;
                    }//end ControllerAction.CPU

                    ControllerAction.RAM.Enabled = configuration.ControllerAction?.RAM?.Enabled ?? false;
                    if (ControllerAction.RAM.Enabled)
                    {
                        ControllerAction.RAM.IntervalMilliseconds = configuration.ControllerAction?.RAM?.IntervalMilliseconds ?? 1000;
                        ControllerAction.RAM.MinimumBytes = configuration.ControllerAction?.RAM?.MinimumBytes ?? 0;
                    }//end ControllerAction.RAM

                    ControllerAction.Database.Enabled = configuration.ControllerAction?.Database?.Enabled ?? false;
                    if (ControllerAction.Database.Enabled)
                    {
                        ControllerAction.Database.Session.Enabled = configuration.ControllerAction?.Database?.Session?.Enabled ?? false;
                        if (ControllerAction.Database.Session.Enabled)
                        {
                            ControllerAction.Database.Session.IgnoreNulls = configuration.ControllerAction?.Database?.Session?.IgnoreNulls ?? true;

                            ControllerAction.Database.Session.MonitoredStatistics = SplitAndClear(configuration.ControllerAction?.Database?.Session?.MonitoredStatistics, null);
                        }// end ControllerAction.Database.Session

                        ControllerAction.Database.Entities.Enabled = configuration.ControllerAction?.Database?.Entities?.Enabled ?? false;
                        if (ControllerAction.Database.Entities.Enabled)
                        {
                            ControllerAction.Database.Entities.IgnoreNulls = configuration.ControllerAction?.Database?.Entities?.IgnoreNulls ?? true;

                            ControllerAction.Database.Entities.MonitoredEntities = SplitAndClear(configuration.ControllerAction?.Database?.Entities?.MonitoredEntities, applicationName);
                            ControllerAction.Database.Entities.MonitoredStatistics = SplitAndClear(configuration.ControllerAction?.Database?.Entities?.MonitoredStatistics, null);
                        }// end ControllerAction.Database.Entities
                    }//end ControllerAction.Database
                }//end ControllerAction

                
                ExposedAPI.Enabled = configuration.ExposedAPI?.Enabled ?? false;
                if (ExposedAPI.Enabled)
                {
                    ExposedAPI.Time.Enabled = configuration.ExposedAPI?.Time?.Enabled ?? false;
                    if (ExposedAPI.Time.Enabled)
                    {
                        ExposedAPI.Time.MinimumMilliseconds = configuration.ExposedAPI?.Time?.MinimumMilliseconds ?? 0;
                    }//end ControllerAction.Time

                    ExposedAPI.CPU.Enabled = configuration.ExposedAPI?.CPU?.Enabled ?? false;
                    if (ExposedAPI.CPU.Enabled)
                    {
                        ExposedAPI.CPU.IntervalMilliseconds = configuration.ExposedAPI?.CPU?.IntervalMilliseconds ?? 1000;
                        ExposedAPI.CPU.MinimumPercentage = configuration.ExposedAPI?.CPU?.MinimumPercentage ?? 0;
                    }//end ControllerAction.CPU

                    ExposedAPI.RAM.Enabled = configuration.ExposedAPI?.RAM?.Enabled ?? false;
                    if (ExposedAPI.RAM.Enabled)
                    {
                        ExposedAPI.RAM.IntervalMilliseconds = configuration.ExposedAPI?.RAM?.IntervalMilliseconds ?? 1000;
                        ExposedAPI.RAM.MinimumBytes = configuration.ExposedAPI?.RAM?.MinimumBytes ?? 0;
                    }//end ControllerAction.RAM

                    ExposedAPI.Database.Enabled = configuration.ExposedAPI?.Database?.Enabled ?? false;
                    if (ExposedAPI.Database.Enabled)
                    {
                        ExposedAPI.Database.Session.Enabled = configuration.ExposedAPI?.Database?.Session?.Enabled ?? false;
                        if (ExposedAPI.Database.Session.Enabled)
                        {
                            ExposedAPI.Database.Session.IgnoreNulls = configuration.ExposedAPI?.Database?.Session?.IgnoreNulls ?? true;

                            ExposedAPI.Database.Session.MonitoredStatistics = SplitAndClear(configuration.ExposedAPI?.Database?.Session?.MonitoredStatistics, null);
                        }// end ControllerAction.Database.Session

                        ExposedAPI.Database.Entities.Enabled = configuration.ExposedAPI?.Database?.Entities?.Enabled ?? false;
                        if (ExposedAPI.Database.Entities.Enabled)
                        {
                            ExposedAPI.Database.Entities.IgnoreNulls = configuration.ExposedAPI?.Database?.Entities?.IgnoreNulls ?? true;

                            ExposedAPI.Database.Entities.MonitoredEntities = SplitAndClear(configuration.ExposedAPI?.Database?.Entities?.MonitoredEntities, applicationName);
                            ExposedAPI.Database.Entities.MonitoredStatistics = SplitAndClear(configuration.ExposedAPI?.Database?.Entities?.MonitoredStatistics, null);
                        }// end ExposedAPI.Database.Entities
                    }//end ExposedAPI.Database
                }//end ExposedAPI

                DataConfiguration.Enabled = configuration.Data?.Enabled ?? false;
                if (DataConfiguration.Enabled)
                {
                    DataConfiguration.ClientData.Enabled = configuration.Data.ClientData?.Enabled ?? false;
                    if (DataConfiguration.ClientData.Enabled)
                    {
                        DataConfiguration.ClientData.Size.Enabled = configuration.Data.ClientData?.Size?.Enabled ?? false;
                        if (DataConfiguration.ClientData.Size.Enabled)
                        {
                            DataConfiguration.ClientData.Size.MinimumBytes = configuration.Data.ClientData?.Size?.MinimumBytes ?? 0;
                        }//end DataConfiguration.ClientData.Size

                        DataConfiguration.ClientData.Time.Enabled = configuration.Data.ClientData?.Time?.Enabled ?? false;
                        if (DataConfiguration.ClientData.Time.Enabled)
                        {
                            DataConfiguration.ClientData.Time.MinimumMilliseconds = configuration.Data.ClientData?.Time?.MinimumMilliseconds ?? 0;
                        }//end ClientData.Time

                        DataConfiguration.ClientData.Database = null;
                    }//end DataConfiguration.ClientData

                    DataConfiguration.Model2DTO.Enabled = configuration.Data.Model2DTO?.Enabled ?? false;
                    if (DataConfiguration.Model2DTO.Enabled)
                    {
                        DataConfiguration.Model2DTO.Size.Enabled = configuration.Data.Model2DTO?.Size?.Enabled ?? false;
                        if (DataConfiguration.Model2DTO.Size.Enabled)
                        {
                            DataConfiguration.Model2DTO.Size.MinimumBytes = configuration.Data.Model2DTO?.Size?.MinimumBytes ?? 0;
                        }//end DataConfiguration.Model2DTO.Size

                        DataConfiguration.Model2DTO.Time.Enabled = configuration.Data.Model2DTO?.Time?.Enabled ?? false;
                        if (DataConfiguration.Model2DTO.Time.Enabled)
                        {
                            DataConfiguration.Model2DTO.Time.MinimumMilliseconds = configuration.Data.Model2DTO?.Time?.MinimumMilliseconds ?? 0;
                        }//end Model2DTO.Time

                        DataConfiguration.Model2DTO.Database = null;
                    }//end DataConfiguration.Model2DTO

                    DataConfiguration.DTO2ViewModel.Enabled = configuration.Data.DTO2ViewModel?.Enabled ?? false;
                    if (DataConfiguration.DTO2ViewModel.Enabled)
                    {
                        DataConfiguration.DTO2ViewModel.Size.Enabled = configuration.Data.DTO2ViewModel?.Size?.Enabled ?? false;
                        if (DataConfiguration.DTO2ViewModel.Size.Enabled)
                        {
                            DataConfiguration.DTO2ViewModel.Size.MinimumBytes = configuration.Data.DTO2ViewModel?.Size?.MinimumBytes ?? 0;
                        }//end DataConfiguration.DTO2ViewModel.Size

                        DataConfiguration.DTO2ViewModel.Time.Enabled = configuration.Data.DTO2ViewModel?.Time?.Enabled ?? false;
                        if (DataConfiguration.DTO2ViewModel.Time.Enabled)
                        {
                            DataConfiguration.DTO2ViewModel.Time.MinimumMilliseconds = configuration.Data.DTO2ViewModel?.Time?.MinimumMilliseconds ?? 0;
                        }//end DataConfiguration.DTO2ViewModel.Time

                        DataConfiguration.DTO2ViewModel.Database.Enabled = configuration.Data?.DTO2ViewModel?.Database?.Enabled ?? false;
                        if (DataConfiguration.DTO2ViewModel.Database.Enabled)
                        {
                            DataConfiguration.DTO2ViewModel.Database.Session.Enabled = configuration.Data?.DTO2ViewModel?.Database?.Session?.Enabled ?? false;
                            if (DataConfiguration.DTO2ViewModel.Database.Session.Enabled)
                            {
                                DataConfiguration.DTO2ViewModel.Database.Session.IgnoreNulls = configuration.Data?.DTO2ViewModel?.Database?.Session?.IgnoreNulls ?? true;
                                DataConfiguration.DTO2ViewModel.Database.Session.MonitoredStatistics = SplitAndClear(configuration.Data?.DTO2ViewModel?.Database?.Session?.MonitoredStatistics, null);
                            }// end Conversions.DTO2ViewModel.Database.Session

                            DataConfiguration.DTO2ViewModel.Database.Entities.Enabled = configuration.Data?.DTO2ViewModel?.Database?.Entities?.Enabled ?? false;
                            if (DataConfiguration.DTO2ViewModel.Database.Entities.Enabled)
                            {
                                DataConfiguration.DTO2ViewModel.Database.Entities.IgnoreNulls = configuration.Data?.DTO2ViewModel?.Database?.Entities?.IgnoreNulls ?? true;

                                DataConfiguration.DTO2ViewModel.Database.Entities.MonitoredEntities =   SplitAndClear(configuration.Data?.DTO2ViewModel?.Database?.Entities?.MonitoredEntities, applicationName);
                                DataConfiguration.DTO2ViewModel.Database.Entities.MonitoredStatistics = SplitAndClear(configuration.Data?.DTO2ViewModel?.Database?.Entities?.MonitoredStatistics, null);
                            }// end DataConfiguration.DTO2ViewModel.Database.Entities
                        }//end Conversions.DTO2ViewModel.Database
                    }//end DataConfiguration.DTO2ViewModel

                }//end DataConfiguration

                DBFlush.Enabled = configuration.DBFlush?.Enabled ?? false;
                if (DBFlush.Enabled)
                {
                    DBFlush.Time.Enabled = configuration.DBFlush?.Time?.Enabled ?? false;
                    if (DBFlush.Time.Enabled)
                    {
                        DBFlush.Time.MinimumMilliseconds = configuration.DBFlush?.Time?.MinimumMilliseconds ?? 0;
                    }//end DBFlush.Time

                    DBFlush.Database.Enabled = configuration.DBFlush?.Database?.Enabled ?? false;
                    if (DBFlush.Database.Enabled)
                    {
                        DBFlush.Database.Session.Enabled = configuration.DBFlush?.Database?.Session?.Enabled ?? false;
                        if (DBFlush.Database.Session.Enabled)
                        {
                            DBFlush.Database.Session.IgnoreNulls = configuration.DBFlush?.Database?.Session?.IgnoreNulls ?? true;

                            DBFlush.Database.Session.MonitoredStatistics = SplitAndClear(configuration.DBFlush?.Database?.Session?.MonitoredStatistics, null);

                        }// end DBFlush.Database.Session

                        DBFlush.Database.Entities.Enabled = configuration.DBFlush?.Database?.Entities?.Enabled ?? false;
                        if (DBFlush.Database.Entities.Enabled)
                        {
                            DBFlush.Database.Entities.IgnoreNulls = configuration.DBFlush?.Database?.Entities?.IgnoreNulls ?? true;

                            DBFlush.Database.Entities.MonitoredEntities = SplitAndClear(configuration.DBFlush?.Database?.Entities?.MonitoredEntities, applicationName);
                            DBFlush.Database.Entities.MonitoredStatistics = SplitAndClear(configuration.DBFlush?.Database?.Entities?.MonitoredStatistics, null);

                        }// end DBFlush.Database.Entities
                    }//end DBFlush.Database
                }//end DBFlush
            }
        }
    }
}
