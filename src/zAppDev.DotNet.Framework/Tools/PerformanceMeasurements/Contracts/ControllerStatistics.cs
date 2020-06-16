// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Configuration;

namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Contracts
{
    /// <summary>
    /// To be used with any action, e.g. Controller Action, Exposed API Action 
    /// </summary>
    public class ActionStatistics: IPerformanceStatistic<ActionConfiguration>
    {
        /// <summary>
        /// For Exposed API Statistics
        /// </summary>
        public string API { get; set; }

        /// <summary>
        /// For Exposed API Statistics 
        /// </summary>
        public string Operation { get; set; }

        public string DateTime { get; set; }

        public UsageStatistics UsageStatistics { get; set; }
        public NHStatistics NHStatistics { get; set; }

        public ActionStatistics()
        {
            UsageStatistics = new UsageStatistics();
            NHStatistics = new NHStatistics();
        }

        public bool IsInteresting(ActionConfiguration configuration = null)
        {
            if (configuration == null) return true;
            if (!configuration.Enabled) return false;

            if (UsageStatistics?.IsInteresting(configuration) == false) UsageStatistics = null;
            if (NHStatistics?.IsInteresting(configuration.Database) == false) NHStatistics = null; 

            return (UsageStatistics?.IsInteresting(configuration) == true || NHStatistics?.IsInteresting(configuration.Database) == true);
        }        
    }
}