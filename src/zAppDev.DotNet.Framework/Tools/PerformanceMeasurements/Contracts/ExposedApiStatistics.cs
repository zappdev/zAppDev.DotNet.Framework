using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Contracts
{
    public class ExposedApiStatistics : IPerformanceStatistic<ExposedApiStatistics>
    {
        public EndpointUsageStatistics UsageStatistics { get; set; }
        public NHStatistics NHStatistics { get; set; }

        public ExposedApiStatistics()
        {
            NHStatistics = new NHStatistics();
        }
        public bool IsInteresting(ExposedApiStatistics configuration)
        {
            throw new NotImplementedException();
        }
    }
}
