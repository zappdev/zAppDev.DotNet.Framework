#if NETFRAMEWORK
namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Contracts
{
    public enum MonitorStatus
    {
        None,
        Stopped,
        Running
    }

    public interface IPerformanceMonitor<IPerformanceStatistic>
    {
        MonitorStatus MonitorStatus { get; }
        bool ContinueOnError { get; set; }

        void Start();
        void Stop();
        IPerformanceStatistic Get();
    }

}
#endif