using CLMS.Framework.Tools.PerformanceMeasurements.Configuration;
using CLMS.Framework.Tools.PerformanceMeasurements.Contracts;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace CLMS.Framework.Tools.PerformanceMeasurements.Components
{



    public class UsageMonitor : IPerformanceMonitor<UsageStatistics>
    {
        public MonitorStatus MonitorStatus { get; private set; }
        public bool ContinueOnError { get; set; }

        private readonly List<float> _cpuMetrics;
        private readonly List<float> _ramMetrics;

        private readonly Stopwatch _stopwatch;

        private readonly Object _cpuLock;
        private readonly Object _ramLock;

        private readonly ControllerActionConfiguration _configuration;

        private readonly ILog _genericLog;

        private string _nameOfThis => nameof(UsageMonitor);
        private Type _typeOfThis => typeof(UsageMonitor);

        private DateTime lastCPUTime;
        private TimeSpan lastTotalProcessorTime;
        private DateTime curCPUTime;
        private TimeSpan curTotalProcessorTime;

        private readonly Process _currentProcess;

        public UsageMonitor(ControllerActionConfiguration configuration, ILog log = null)
        {
            MonitorStatus = MonitorStatus.None;
            ContinueOnError = true;

            _configuration = configuration;

            _genericLog = log ?? LogManager.GetLogger(_typeOfThis);

            try
            {
                _currentProcess = Process.GetCurrentProcess();
                if (_configuration.RAM.Enabled)
                {
                    _ramLock = new Object();
                    _ramMetrics = new List<float>();
                }

                if (_configuration.CPU.Enabled)
                {
                    _cpuLock = new Object();
                    _cpuMetrics = new List<float>();
                }

                if (_configuration.Time.Enabled)
                {
                    _stopwatch = new Stopwatch();
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
            try
            {
                MonitorStatus = MonitorStatus.Running;

                if (_configuration.CPU.Enabled)
                {
                    //_cpuCounter.NextValue();

                    new Thread(() =>
                    {
                        GetCPUSample();
                    }).Start();
                }

                if (_configuration.RAM.Enabled)
                {
                    new Thread(() =>
                    {
                        GetRAMSample();
                    }).Start();
                }

                if (_configuration.Time.Enabled)
                {
                    _stopwatch.Start();
                }
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
            try
            {
                _stopwatch?.Stop();

                MonitorStatus = MonitorStatus.Stopped;
            }
            catch (Exception e)
            {
                _genericLog.Error($"{_nameOfThis} - Exception while Stopping {_nameOfThis}: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Exception while Stopping {_nameOfThis}: {e.StackTrace}");
                if (!ContinueOnError) throw;
            }

        }//end Stop()

        public UsageStatistics Get()
        {
            var result = new UsageStatistics();

            try
            {
                result.ElapsedMilliseconds = _stopwatch?.ElapsedMilliseconds ?? 0;
                result.AverageCPU = _cpuMetrics?.Any() == true ? _cpuMetrics?.Average() ?? 0 : 0;
                result.AverageRAM = _ramMetrics?.Any() == true ? _ramMetrics?.Average() ?? 0 : 0;

            }
            catch (Exception e)
            {
                _genericLog.Error($"{_nameOfThis} - Exception while Getting Statistics: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Exception while Getting Statistics: {e.StackTrace}");
                if (!ContinueOnError) throw;
            }
            return result;
        }//end Get()

        private void GetCPUSample()
        {
            if (MonitorStatus == MonitorStatus.Stopped) return;

            while (MonitorStatus == MonitorStatus.Running)
            {
                lock (_cpuLock)
                {
                    if (lastCPUTime == null || lastCPUTime == new DateTime())
                    {
                        lastCPUTime = DateTime.Now;
                        lastTotalProcessorTime = _currentProcess.TotalProcessorTime;
                    }
                    else
                    {
                        curCPUTime = DateTime.Now;
                        curTotalProcessorTime = _currentProcess.TotalProcessorTime;

                        var CPUUsage = (curTotalProcessorTime.TotalMilliseconds - lastTotalProcessorTime.TotalMilliseconds) / curCPUTime.Subtract(lastCPUTime).TotalMilliseconds / Convert.ToDouble(Environment.ProcessorCount);
                        _cpuMetrics.Add(Convert.ToSingle(CPUUsage * 100));

                        lastCPUTime = curCPUTime;
                        lastTotalProcessorTime = curTotalProcessorTime;
                    }
                }

                Thread.Sleep(_configuration.CPU.IntervalMilliseconds);
            }

        }//GetCPUSample()

        private void GetRAMSample()
        {
            if (MonitorStatus == MonitorStatus.Stopped) return;

            while (MonitorStatus == MonitorStatus.Running)
            {
                lock (_ramLock)
                {
                    _ramMetrics.Add(_currentProcess.VirtualMemorySize64);
                }

                Thread.Sleep(_configuration.RAM.IntervalMilliseconds);
            }
        }//GetRAMSample()
    }
}
