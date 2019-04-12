#if NETFRAMEWORK
using CLMS.Framework.Tools.PerformanceMeasurements.Configuration;
using CLMS.Framework.Tools.PerformanceMeasurements.Contracts;
using Newtonsoft.Json;

namespace CLMS.Framework.Tools.PerformanceMeasurements
{
    public class PerformanceStatistics: IPerformanceStatistic<PerformanceMonitorConfiguration>
    {
        public string Controller;
        public string Action;

        public ControllerActionStatistics ControllerActionStatistics { get; set; }
        public DataStatistics RequestStatistics { get; set; }
        public DataStatistics ResponseStatisticsPreAction { get; set; }
        public DataStatistics ResponseStatisticsPostAction { get; set; }
        public DataStatistics ConversionStatistics { get; set; }
        public DataStatistics DatabaseFlushStatistics { get; set; }

        private readonly PerformanceMonitorConfiguration _configuration;
        public PerformanceStatistics(PerformanceMonitorConfiguration configuration)
        {
            _configuration = configuration;
            ControllerActionStatistics = new ControllerActionStatistics();
            RequestStatistics = new DataStatistics();
            ResponseStatisticsPreAction = new DataStatistics();
            ResponseStatisticsPostAction = new DataStatistics();
            ConversionStatistics = new DataStatistics();
            DatabaseFlushStatistics = new DataStatistics();
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

            if(ControllerActionStatistics == null && RequestStatistics == null && ResponseStatisticsPreAction == null && ResponseStatisticsPostAction == null && ConversionStatistics == null && DatabaseFlushStatistics == null)
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
#endif