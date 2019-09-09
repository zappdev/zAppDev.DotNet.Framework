// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Helpers
{
    public static class Convertors
    {
        public static List<FrontEndPerformance> ToFrontEndPerformanceList (this FrontEndStatisticsDTO frontEndStatisticsDTO, string userHostAddress)
        {
            if(frontEndStatisticsDTO.FrontEndMetricsDTOList?.Any() != true)
            {
                return null; 
            }

            var frontEndPerformanceList = new List<FrontEndPerformance>();

            var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff");

            foreach (var frontEndMetricsDTO in frontEndStatisticsDTO.FrontEndMetricsDTOList)
            {
                var idParts = frontEndMetricsDTO.ID.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                var controller = idParts[0];

                string action = null;
                string infromation = null;

                if (idParts.Length > 1) action = idParts[1];
                if (idParts.Length > 2) infromation = idParts[2];

                var frontEndPerformanceStatistics = new FrontEndPerformanceStatistics();

                frontEndPerformanceStatistics.Action = action;
                frontEndPerformanceStatistics.Information = infromation;
                frontEndPerformanceStatistics.ElapsedMilliseconds = frontEndMetricsDTO.ElapsedMilliseconds;

                bool mustAdd = false;
                var frontEndPerformanceController = frontEndPerformanceList.FirstOrDefault(x => x.Controller == controller);
                if(frontEndPerformanceController == null)
                {
                    mustAdd = true;
                    frontEndPerformanceController = new FrontEndPerformance();
                    frontEndPerformanceController.Controller = controller;
                    frontEndPerformanceController.DateTime = dateTime;
                    frontEndPerformanceController.UserHostAddress = userHostAddress;
                }

                frontEndPerformanceController.FrontEndStatistics.Add(frontEndPerformanceStatistics);

                if (mustAdd) frontEndPerformanceList.Add(frontEndPerformanceController);
            }

            return frontEndPerformanceList;

        }//end ToFrontEndPerformanceStatisticsList();
    }
}
