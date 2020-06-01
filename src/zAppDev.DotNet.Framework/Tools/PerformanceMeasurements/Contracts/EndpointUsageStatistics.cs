using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Configuration;

namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Contracts
{
    public class EndpointUsageStatistics : IPerformanceStatistic<ExposedApiConfiguration>
    {
        public long? ElapsedMilliseconds { get; set; }

        public float? AverageCPU { get; set; }

        public float? AverageRAM { get; set; }

        public bool IsInteresting(ExposedApiConfiguration configuration)
        {
            throw new NotImplementedException();
        }
    }
}
