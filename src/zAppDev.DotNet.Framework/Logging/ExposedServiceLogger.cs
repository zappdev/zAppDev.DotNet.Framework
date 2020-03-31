#if NETFRAMEWORK
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zAppDev.DotNet.Framework.Logging
{
    public class ExposedServiceLogger : IExposedServiceLogger
    {
        public bool IsFileLoggingEnabled { get; set; }
        public bool IsRabitEnabled { get; set; }
        public bool IsDatabaseEnabled { get; set; }

        public ILog _logger { get; set; }

        public ExposedServiceLogger(bool fileEnabled, string loggerName, bool rabitEnabled, bool databaseEnabled)
        {
            IsFileLoggingEnabled = fileEnabled;
            _logger = log4net.LogManager.GetLogger(loggerName);
            IsRabitEnabled = rabitEnabled;
            IsDatabaseEnabled = databaseEnabled;
        }
        public static IExposedServiceLogger FromConfiguration()
        {
            bool.TryParse(ConfigurationManager.AppSettings["FileLoggingAPIMetada"], out var fileEnabled);
            var loggerName = ConfigurationManager.AppSettings["FileLoggerName"];
            bool.TryParse(ConfigurationManager.AppSettings["RabitMQLoggingAPIMetadata"], out var rabitEnabled);
            bool.TryParse(ConfigurationManager.AppSettings["DatabaseLogginAPIMetada"], out var databaseEnabled); 

            return new ExposedServiceLogger(fileEnabled, loggerName, rabitEnabled, databaseEnabled);
        }
        public void LogExposedAPIMetadata(ExposedServiceMetadataStruct metadata)
        {
            if (IsFileLoggingEnabled) LogInFile(metadata);
        }

        private void LogInFile(ExposedServiceMetadataStruct metadata)
        {
            _logger.Info(Newtonsoft.Json.JsonConvert.SerializeObject(metadata));
        }
    }
}
#else
#endif