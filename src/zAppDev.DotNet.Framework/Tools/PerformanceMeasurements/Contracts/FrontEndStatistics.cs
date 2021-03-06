﻿// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Contracts
{
    public class FrontEndStatisticsDTO
    {
        public List<FrontEndMetricsDTO> FrontEndMetricsDTOList; 
    }

    public class FrontEndMetricsDTO
    {
        public string ID;
        public float ElapsedMilliseconds;
    }

    public class FrontEndPerformanceStatistics
    {
        public string Action;
        public string Information;
        public float ElapsedMilliseconds;
    }

    public class FrontEndPerformance
    {
        public string Controller;
        public string UserHostAddress;
        public string DateTime;
        public List<FrontEndPerformanceStatistics> FrontEndStatistics;

        public FrontEndPerformance()
        {
            FrontEndStatistics = new List<FrontEndPerformanceStatistics>();
        }
    }
}
