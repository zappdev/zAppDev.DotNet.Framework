// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Data;
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Configuration;
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Contracts;
using log4net;
using NHibernate;
using System;

namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Components
{
    public class ControllerActionMonitor: IPerformanceMonitor<ControllerActionStatistics>
    {
        public MonitorStatus MonitorStatus { get; private set; }
        public bool ContinueOnError { get; set; }

        private readonly UsageMonitor _usageMonitor;
        private readonly NHMonitor _nhMonitor;

        private readonly ControllerActionConfiguration _configuration;

        private readonly ILog _genericLog;

        private string _nameOfThis => nameof(ControllerActionMonitor);
        private Type _typeOfThis => typeof(ControllerActionMonitor);

        private readonly bool _run;

        public ControllerActionMonitor(ControllerActionConfiguration configuration, ISession session, ILog log = null)
        {
            MonitorStatus = MonitorStatus.None;

            _configuration = configuration;

            _genericLog = log ?? LogManager.GetLogger(_typeOfThis);

            _run = _configuration.Enabled;

            try
            {
                var measureUsage = ((_run) && (_configuration.CPU.Enabled || _configuration.RAM.Enabled));
                var measureDatabase = ((_run) && (_configuration.Database.Enabled) && (_configuration.Database.Session.Enabled || _configuration.Database.Entities.Enabled));

                if (measureUsage)
                {
                    _usageMonitor = new UsageMonitor(configuration);
                }

                if (measureDatabase)
                {
                    _nhMonitor = new NHMonitor(_configuration.Database, session);
                }
            }
            catch (Exception e)
            {
                _genericLog.Error($"{_nameOfThis} - Failed to initialize {_nameOfThis}: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Failed to initialize {_nameOfThis}: {e.StackTrace}");
                if (!ContinueOnError) throw;
            }
        }//end ctor

        public void Start()
        {
            if (!_run) return;

            try
            {
                MonitorStatus = MonitorStatus.Running;

                _usageMonitor?.Start();

                _nhMonitor?.Start();
            }
            catch (Exception e)
            {
                _genericLog.Error($"{_nameOfThis} - Exception while Starting {_nameOfThis}: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Exception while Starting {_nameOfThis}: {e.StackTrace}");
                if (!ContinueOnError) throw;
            }
        }//end Start()

        public void Stop()
        {
            if (!_run) return;
            try
            {
                MonitorStatus = MonitorStatus.Stopped;
                _usageMonitor?.Stop();

                _nhMonitor?.Stop();
            }
            catch (Exception e)
            {
                _genericLog.Error($"{_nameOfThis} - Exception while Stopping {_nameOfThis}: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Exception while Stopping {_nameOfThis}: {e.StackTrace}");
                if (!ContinueOnError) throw;
            }
        }//end Stop()

        public ControllerActionStatistics Get()
        {
            if (!_run) return null;

            var result = new ControllerActionStatistics();
            try
            {
                if (MonitorStatus == MonitorStatus.None)
                {
                    _genericLog.Error($"{_nameOfThis} - Get() was called, but the Monitor was never Started. Start and then Stop the Monitor, before Getting the results.");
                    return null;
                }

                if (MonitorStatus == MonitorStatus.Running)
                {
                    _genericLog.Info($"{_nameOfThis} - Get() was called, but the Monitor is still Running. The results presented might be inaccurate. (Hint: Stop the Monitor, before Logging the results)");
                }

                result.NHStatistics = _nhMonitor?.Get();
                result.UsageStatistics = _usageMonitor?.Get();
                return result;
            }
            catch (Exception e)
            {
                _genericLog.Error($"{_nameOfThis} - Exception while Getting Statistics: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Exception while Getting Statistics: {e.StackTrace}");
                if (!ContinueOnError) throw;
                return null;
            }
        }//end Get()
    }
}
