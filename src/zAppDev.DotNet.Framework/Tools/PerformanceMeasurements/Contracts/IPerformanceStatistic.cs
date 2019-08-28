#if NETFRAMEWORK
namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Contracts
{
    public interface IPerformanceStatistic<IPerformanceConfiguration>
    {
        bool IsInteresting(IPerformanceConfiguration configuration);
    }
}
#endif