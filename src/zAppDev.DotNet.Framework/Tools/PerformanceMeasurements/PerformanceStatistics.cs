// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Configuration;
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Contracts;
using Newtonsoft.Json;

namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements
{
    public class PerformanceStatistics : IPerformanceStatistic<PerformanceMonitorConfiguration>
    {
        public string Controller;
        public string Action;
        public string UserHostAddress;
        public string DateTime;

        public ActionStatistics ControllerActionStatistics { get; set; }
        public DataStatistics RequestStatistics { get; set; }
        public DataStatistics ResponseStatisticsPreAction { get; set; }
        public DataStatistics ResponseStatisticsPostAction { get; set; }
        public DataStatistics ConversionStatistics { get; set; }
        public DataStatistics DatabaseFlushStatistics { get; set; }
        public ActionStatistics ExposedAPIStatistics { get; set; }

        private readonly PerformanceMonitorConfiguration _configuration;
        public PerformanceStatistics(PerformanceMonitorConfiguration configuration)
        {
            _configuration = configuration;
            ControllerActionStatistics = new ActionStatistics();
            RequestStatistics = new DataStatistics();
            ResponseStatisticsPreAction = new DataStatistics();
            ResponseStatisticsPostAction = new DataStatistics();
            ConversionStatistics = new DataStatistics();
            DatabaseFlushStatistics = new DataStatistics();
            ExposedAPIStatistics = new ActionStatistics();
        }

        public bool IsInteresting(PerformanceMonitorConfiguration configuration = null)
        {
            var isInteresting =
                (
                    (ControllerActionStatistics != null)
                    ||
                    (RequestStatistics != null)
                    ||
                    (ResponseStatisticsPreAction != null)
                    ||
                    (ResponseStatisticsPostAction != null)
                    ||
                    (ConversionStatistics != null)
                    ||
                    (DatabaseFlushStatistics != null)
                    ||
                    (ExposedAPIStatistics != null)
                );
                
            return isInteresting;
        }

        public void ClearNotInteresting()
        {
            if (ControllerActionStatistics?.IsInteresting(_configuration.ControllerAction) != true)
            {
                ControllerActionStatistics = null;
            }

            if (RequestStatistics?.IsInteresting(_configuration.DataConfiguration.ClientData) != true)
            {
                RequestStatistics = null;
            }

            if (ResponseStatisticsPreAction?.IsInteresting(_configuration.DataConfiguration.Model2DTO) != true)
            {
                ResponseStatisticsPreAction = null;
            }

            if (ResponseStatisticsPostAction?.IsInteresting(_configuration.DataConfiguration.Model2DTO) != true)
            {
                ResponseStatisticsPostAction = null;
            }

            if (ConversionStatistics?.IsInteresting(_configuration.DataConfiguration.DTO2ViewModel) != true)
            {
                ConversionStatistics = null;
            }

            if(DatabaseFlushStatistics?.IsInteresting(_configuration.DBFlush) != true)
            {
                DatabaseFlushStatistics = null;
            }

            if (ExposedAPIStatistics?.IsInteresting(_configuration.ExposedAPI) != true)
            {
                ExposedAPIStatistics = null;
            }

            if (ControllerActionStatistics == null && RequestStatistics == null && ResponseStatisticsPreAction == null && ResponseStatisticsPostAction == null && ConversionStatistics == null && DatabaseFlushStatistics == null && ExposedAPIStatistics == null)
            {
                Controller = null;
                Action = null;
            }
        }

        public string GetLog(bool clearNotInteresting = true)
        {
            if (clearNotInteresting)
            {
                ClearNotInteresting();
            }

            return JsonConvert.SerializeObject(this, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            });
        }
    }
}