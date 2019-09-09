// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Configuration;

namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Contracts
{
    public class ControllerActionStatistics: IPerformanceStatistic<ControllerActionConfiguration>
    {
        public UsageStatistics UsageStatistics { get; set; }
        public NHStatistics NHStatistics { get; set; }

        public ControllerActionStatistics()
        {
            UsageStatistics = new UsageStatistics();
            NHStatistics = new NHStatistics();
        }

        public bool IsInteresting(ControllerActionConfiguration configuration = null)
        {
            if (configuration == null) return true;
            if (!configuration.Enabled) return false;

            if (UsageStatistics?.IsInteresting(configuration) == false) UsageStatistics = null;
            if (NHStatistics?.IsInteresting(configuration.Database) == false) NHStatistics = null; 

            return (UsageStatistics?.IsInteresting(configuration) == true || NHStatistics?.IsInteresting(configuration.Database) == true);
        }        
    }
}

#endif