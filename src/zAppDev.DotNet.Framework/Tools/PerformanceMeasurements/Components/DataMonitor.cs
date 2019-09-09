// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
using zAppDev.DotNet.Framework.Data;
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Configuration;
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Contracts;
using log4net;
using System;
using System.Diagnostics;

namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Components
{
    public class DataMonitor: IPerformanceMonitor<DataStatistics>
    {
        public MonitorStatus MonitorStatus { get; private set; }
        public bool ContinueOnError { get; set; }

        private readonly ILog _genericLog;

        private readonly Stopwatch _stopwatch;

        private string Data { get; set; }

        private readonly NHMonitor _nhMonitor;

        private readonly string _element;

        private string _nameOfThis => nameof(DataStatistics);
        private Type _typeOfThis => typeof(DataStatistics);

        public void SetData(string data)
        {
            Data = data;
        }

        public DataMonitor(string element, DataItemConfiguration configuration,  ILog log = null)
        {

            MonitorStatus = MonitorStatus.None;
            ContinueOnError = true;

            _genericLog = log ?? LogManager.GetLogger(_typeOfThis);

            _element = element;

            try
            {
                if (configuration.Time.Enabled)
                {
                    _stopwatch = new Stopwatch();
                }

                if((configuration.Database?.Enabled == true) && ((configuration.Database?.Entities?.Enabled == true) || (configuration.Database?.Session?.Enabled == true)) )
                {
                    _nhMonitor = new NHMonitor(configuration.Database, MiniSessionManager.Instance.Session);
                }
            }
            catch (Exception e)
            {
                _genericLog.Error($"{_nameOfThis} - Failed to initialize {_nameOfThis} regarding {_element}: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Failed to initialize {_nameOfThis} regarding {_element}: {e.StackTrace}");
                if (!ContinueOnError) throw;
            }
        }//end ctor()

        public void Start()
        {
            MonitorStatus = MonitorStatus.Running;
            try
            {
                _stopwatch?.Start();
                _nhMonitor?.Start();
            }
            catch (Exception e)
            {
                _genericLog.Error($"{_nameOfThis} - Exception while Starting {_nameOfThis} regarding {_element}: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Exception while Starting {_nameOfThis} regarding {_element}: {e.StackTrace}");
                if (!ContinueOnError) throw;
            }
        }//end Start()

        public void Stop()
        {
            try
            {
                MonitorStatus = MonitorStatus.Stopped;
                _stopwatch?.Stop();
                _nhMonitor?.Stop();
            }
            catch (Exception e)
            {
                _genericLog.Error($"{_nameOfThis} - Exception while Stopping {_nameOfThis} regarding {_element}: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Exception while Stopping {_nameOfThis} regarding {_element}: {e.StackTrace}");
                if (!ContinueOnError) throw;
            }
        }//end Stop()

        public DataStatistics Get()
        {
            try
            {
                var result = new DataStatistics();

                result.DataSize = string.IsNullOrEmpty(Data) ? 0 : System.Text.ASCIIEncoding.Unicode.GetByteCount(Data);
                result.ElapsedMilliseconds = _stopwatch?.ElapsedMilliseconds ?? 0;
                result.NHStatistics = _nhMonitor?.Get();
                return result;
            }
            catch (Exception e)
            {
                _genericLog.Error($"{_nameOfThis} - Exception while Getting Statistics regarding {_element}: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Exception while Getting Statistics regarding {_element}: {e.StackTrace}");
                if (!ContinueOnError) throw;
                return null;
            }
        }//end Get()
    }
}
#endif