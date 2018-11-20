using CLMS.Framework.Tools.PerformanceMeasurements.Configuration;

namespace CLMS.Framework.Tools.PerformanceMeasurements.Contracts
{
    public class UsageStatistics : IPerformanceStatistic<ControllerActionConfiguration>
    {
        public long? ElapsedMilliseconds { get; set; }

        public float? AverageCPU { get; set; }

        public float? AverageRAM { get; set; }

        public bool IsInteresting(ControllerActionConfiguration configuration = null)
        {
            if (configuration == null) return true;
            if (!configuration.Enabled) return false;

            if (!configuration.Time.Enabled || ElapsedMilliseconds.GetValueOrDefault(-1) < configuration.Time.MinimumMilliseconds)
            {
                ElapsedMilliseconds = null;
            }

            if (!configuration.CPU.Enabled || AverageCPU.GetValueOrDefault(-1) < configuration.CPU.MinimumPercentage)
            {
                ElapsedMilliseconds = null;
            }

            if (!configuration.RAM.Enabled || AverageRAM.GetValueOrDefault(-1) < configuration.RAM.MinimumBytes)
            {
                AverageRAM = null;
            }

            return (ElapsedMilliseconds.HasValue || AverageCPU.HasValue || AverageRAM.HasValue);
        }//end IsInteresting()
    }
}
