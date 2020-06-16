// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.

using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Components;
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Configuration;
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Contracts;
using log4net;
using Newtonsoft.Json;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements
{
    public class PerformanceMonitor
    {
        public readonly ActionMonitor ControllerActionMonitor;
        public readonly ActionMonitor ExposedAPIMonitor;
        public readonly DataMonitor RequestMonitor;
        public readonly DataMonitor ResponseMonitorPreAction;
        public readonly DataMonitor ResponseMonitorPostAction;
        public readonly DataMonitor ConversionMonitor; //dto to view model
        public readonly DataMonitor DatabaseFlushMonitor;


        public bool ContinueOnError { get; set; }
        private readonly ILog _monitorLog;
        private readonly ILog _genericLog;

        private readonly PerformanceMonitorConfiguration _configuration;
        private string _nameOfThis => nameof(PerformanceMonitor);
        private Type _typeOfThis => typeof(PerformanceMonitor);

        public PerformanceMonitor(PerformanceMonitorConfiguration configuration, ISession session, ILog log = null, bool? forceEnabledState = null)
        {
            _configuration = configuration;
            ContinueOnError = true;

            _monitorLog = log ?? _configuration.GetLogger();
            _genericLog = LogManager.GetLogger(_typeOfThis);

            if(_configuration != null && forceEnabledState.HasValue)
            {
                _configuration.Enabled = forceEnabledState.Value;
            }

            try
            {
                if (_configuration?.Enabled == true)
                {
                    if (configuration.ControllerAction.Enabled)
                        ControllerActionMonitor = new ActionMonitor(_configuration.ControllerAction, session);

                    if (configuration.DataConfiguration.Enabled)
                    {
                        if (configuration.DataConfiguration.ClientData.Enabled)
                        {
                            RequestMonitor = new DataMonitor("Request Data", configuration.DataConfiguration.ClientData, session);
                        }

                        if (configuration.DataConfiguration.Model2DTO.Enabled)
                        {
                            ResponseMonitorPreAction = new DataMonitor("Response Data Pre Action", _configuration.DataConfiguration.Model2DTO, session);
                            ResponseMonitorPostAction = new DataMonitor("Response Data Post Action", _configuration.DataConfiguration.Model2DTO, session);
                        }

                        if (configuration.DataConfiguration.DTO2ViewModel.Enabled)
                        {
                            ConversionMonitor = new DataMonitor("DTO to ViewModel Conversion", _configuration.DataConfiguration.DTO2ViewModel, session);
                        }
                    }

                    if (configuration.DBFlush.Enabled)
                    {
                        DatabaseFlushMonitor = new DataMonitor("Database Flush Monitor", configuration.DBFlush, session);
                    }
                }
            }
            catch (Exception e)
            {
                _genericLog.Error($"{_nameOfThis} - Failed to initialize {_nameOfThis}: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Failed to initialize {_nameOfThis}: {e.StackTrace}");
                if (!ContinueOnError) throw;
            }
        }

        public PerformanceStatistics Get()
        {
            try
            {
                var statistics = new PerformanceStatistics(_configuration);

                statistics.ControllerActionStatistics = ControllerActionMonitor?.Get();
                statistics.RequestStatistics = RequestMonitor?.Get();
                statistics.ResponseStatisticsPreAction = ResponseMonitorPreAction?.Get();
                statistics.ResponseStatisticsPostAction = ResponseMonitorPostAction?.Get();
                statistics.ConversionStatistics = ConversionMonitor?.Get();
                statistics.DatabaseFlushStatistics = DatabaseFlushMonitor?.Get();
                statistics.ExposedAPIStatistics = ExposedAPIMonitor?.Get();

                return statistics;
            }
            catch (Exception e)
            {
                _genericLog.Error($"{_nameOfThis} - Exception while Getting Statistics: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Exception while Getting Statistics: {e.StackTrace}");
                if (!ContinueOnError) throw;
                return null;
            }
        }//end Get()

        public void Log(string controller, string action, string userHostAddress)
        {
            try
            {
                var statistics = Get();
                if (!statistics.IsInteresting()) return;

                statistics.Controller = controller;
                statistics.Action = action;
                statistics.UserHostAddress = userHostAddress;
                statistics.DateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff");

                var logText = statistics.GetLog(true);
                _monitorLog.Info(logText);
            }
            catch (Exception e)
            {
                _genericLog.Error($"{_nameOfThis} - Exception while Logging Back-End Statistics: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Exception while Logging Back-End Statistics: {e.StackTrace}");
                if (!ContinueOnError) throw;
            }
        }//end Log()

        public void Log(List<FrontEndPerformance> frontEndPerformanceRecords)
        {
            try
            {
                if (frontEndPerformanceRecords?.Any() != true) return;

                foreach(var frontEndPerformanceRecord in frontEndPerformanceRecords)
                {
                    var logText = JsonConvert.SerializeObject(frontEndPerformanceRecord, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented
                    });

                    _monitorLog.Info(logText);
                }

            }
            catch (Exception e)
            {
                _genericLog.Error($"{_nameOfThis} - Exception while Logging Front-End Statistics: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Exception while Logging Front-End Statistics: {e.StackTrace}");
                if (!ContinueOnError) throw;
            }
        }//end Log()
    }
}
