using log4net;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Configuration;
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Contracts;

namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Components
{
    public class ExposedApiMonitor : IPerformanceMonitor<ExposedApiStatistics>
    {
        public MonitorStatus MonitorStatus { get; private set; }
        public bool ContinueOnError { get; set; }

        private readonly EndpointUsageMonitor _usageMonitor;
        private readonly NHMonitor _nhMonitor;

        private readonly ExposedApiConfiguration _configuration;

        private readonly ILog _genericLog;

        private string _nameOfThis => nameof(ExposedApiMonitor);
        private Type _typeOfThis => typeof(ExposedApiMonitor);

        private readonly bool _run;

        public ExposedApiMonitor(ExposedApiConfiguration configuration, ISession session, ILog log = null)
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
                    _usageMonitor = new EndpointUsageMonitor(configuration);
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
        }
        
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
        }

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
        }

        ExposedApiStatistics IPerformanceMonitor<ExposedApiStatistics>.Get()
        {
            if (!_run) return null;

            var result = new ExposedApiStatistics();
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
        }
    }
}
