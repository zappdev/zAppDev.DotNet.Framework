#if NETFRAMEWORK
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Configuration;

namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Contracts
{
    public class DataStatistics : IPerformanceStatistic<DataItemConfiguration>
    {
        public long? ElapsedMilliseconds { get; set; }

        public float? DataSize { get; set; }

        public NHStatistics NHStatistics { get; set; }

        public bool IsInteresting(DataItemConfiguration configuration = null)
        {
            if (configuration == null) return true;
            if(!configuration.Enabled) return false;

            if(!configuration.Time.Enabled || ElapsedMilliseconds.GetValueOrDefault(-1) < configuration.Time.MinimumMilliseconds)
            {
                ElapsedMilliseconds = null;
            }

            if (!configuration.Size.Enabled || DataSize.GetValueOrDefault(-1) < configuration.Size.MinimumBytes)
            {
                DataSize = null;
            }

            if(configuration.Database == null || NHStatistics?.IsInteresting(configuration.Database) != true)
            {
                NHStatistics = null;
            }

            var isInteresting =
            (
                ElapsedMilliseconds.HasValue || DataSize.HasValue || NHStatistics != null
            );
            return isInteresting;
        }
    }
}
#endif