#if NETFRAMEWORK
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Configuration;
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Contracts;
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Helpers;
using log4net;
using NHibernate;
using NHibernate.Stat;
using System;

namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Components
{
    public class NHMonitor: IPerformanceMonitor<NHStatistics>
    {
        public MonitorStatus MonitorStatus { get; private set; }
        public bool ContinueOnError { get; set; }

        private readonly ISession _session;
        DatabaseConfiguration _configuration;

        private readonly ILog _genericLog;

        private readonly bool _run;

        private readonly bool _deleteStatisticsAtStart = false;
        private readonly bool _deleteStaatisticsAtEnd = false;

        private readonly NHSessionStatisticsHelper _nhSessionStatisticsHelper;
        private readonly NHEntityStatisticsHelper _nhEntityStatisticsHelper;

        private StatisticsImpl _previousNHStatistics { get; set; }
        private StatisticsImpl _currentNHStatistics { get; set; }

        private string _nameOfThis => nameof(NHMonitor);
        private Type _typeOfThis => typeof(NHMonitor);

        public NHMonitor(DatabaseConfiguration configuration, ISession session, ILog log = null)
        {
            MonitorStatus = MonitorStatus.None;
            ContinueOnError = true;

            _session = session;
            _configuration = configuration;

            _genericLog = log ?? LogManager.GetLogger(_typeOfThis);

            if (session == null)
            {
                _run = false;
                return;
            }

            try
            {
                if (_configuration.Session.Enabled)
                {
                    _nhSessionStatisticsHelper = new NHSessionStatisticsHelper(_configuration);
                    _run = true;
                }

                if (_configuration.Entities.Enabled)
                {
                    _nhEntityStatisticsHelper = new NHEntityStatisticsHelper(_configuration);
                    _run = true;
                }
            }
            catch (Exception e)
            {
                _genericLog.Error($"{_nameOfThis} - Failed to initialize {_nameOfThis}: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Failed to initialize {_nameOfThis}: {e.StackTrace}");
                if (!ContinueOnError) throw;
            }
        }//end ctor()

        public void Start()
        {
            if (!_run) return;
            try
            {
                if (_deleteStatisticsAtStart) _session.SessionFactory.Statistics.Clear();
                MonitorStatus = MonitorStatus.Running;
                _nhEntityStatisticsHelper.Capture(_session.SessionFactory.Statistics, MonitorStatus);
                _nhSessionStatisticsHelper.Capture(_session.SessionFactory.Statistics, MonitorStatus);
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
                if (_deleteStaatisticsAtEnd) _session.SessionFactory.Statistics.Clear();
                MonitorStatus = MonitorStatus.Stopped;
                _nhEntityStatisticsHelper.Capture(_session.SessionFactory.Statistics, MonitorStatus);
                _nhSessionStatisticsHelper.Capture(_session.SessionFactory.Statistics, MonitorStatus);
            }
            catch (Exception e)
            {
                _genericLog.Error($"{_nameOfThis} - Exception while Stopping {_nameOfThis}: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Exception while Stopping {_nameOfThis}: {e.StackTrace}");
                if (!ContinueOnError) throw;
            }
        }//end Stop()

        public NHStatistics Get()
        {
            if(!_run) return null; 
            var result = new NHStatistics();

            try
            {
                if (_configuration.Session.Enabled)
                {
                    result.NHSessionStatistics = _nhSessionStatisticsHelper?.Get();
                }

                if (_configuration.Entities.Enabled)
                {
                    result.NHEntityStatistics = _nhEntityStatisticsHelper.Get();
                }
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
#endif