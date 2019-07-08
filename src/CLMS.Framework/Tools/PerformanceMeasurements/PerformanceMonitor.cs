#if NETFRAMEWORK
using CLMS.Framework.Tools.PerformanceMeasurements.Components;
using CLMS.Framework.Tools.PerformanceMeasurements.Configuration;
using log4net;
using NHibernate;
using System;

namespace CLMS.Framework.Tools.PerformanceMeasurements
{
    public class PerformanceMonitor
    {
        public readonly ControllerActionMonitor ControllerActionMonitor;
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

        public PerformanceMonitor(PerformanceMonitorConfiguration configuration, ISession session, ILog log = null)
        {
            _configuration = configuration;
            ContinueOnError = true;

            _monitorLog = log ?? _configuration.GetLogger();
            _genericLog = LogManager.GetLogger(_typeOfThis);

            try
            {
                if (_configuration?.Enabled == true)
                {
                    if (configuration.ControllerAction.Enabled)
                        ControllerActionMonitor = new ControllerActionMonitor(_configuration.ControllerAction, session);

                    if (configuration.DataConfiguration.Enabled)
                    {
                        if (configuration.DataConfiguration.ClientData.Enabled)
                        {
                            RequestMonitor = new DataMonitor("Request Data", configuration.DataConfiguration.ClientData);
                        }

                        if (configuration.DataConfiguration.Model2DTO.Enabled)
                        {
                            ResponseMonitorPreAction = new DataMonitor("Response Data Pre Action", _configuration.DataConfiguration.Model2DTO);
                            ResponseMonitorPostAction = new DataMonitor("Response Data Post Action", _configuration.DataConfiguration.Model2DTO);
                        }

                        if (configuration.DataConfiguration.DTO2ViewModel.Enabled)
                        {
                            ConversionMonitor = new DataMonitor("DTO to ViewModel Conversion", _configuration.DataConfiguration.DTO2ViewModel);
                        }
                    }

                    if (configuration.DBFlush.Enabled)
                    {
                        DatabaseFlushMonitor = new DataMonitor("Database Flush Monitor", configuration.DBFlush);
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
                _genericLog.Error($"{_nameOfThis} - Exception while Logging Statistics: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Exception while Logging Statistics: {e.StackTrace}");
                if (!ContinueOnError) throw;
            }
        }//end Log()
    }
}
#endif