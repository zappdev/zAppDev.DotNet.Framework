using log4net;
using System.Configuration;

namespace zAppDev.DotNet.Framework.Data.DatabaseManagers.AccessLogManager
{
    public class AccessLogManagerUtilities
    {
        public static bool? GetDisableAccessLogValue()
        {
            var logger = LogManager.GetLogger(typeof(AccessLogManagerUtilities));

            var setting = ConfigurationManager.AppSettings["DisableAccessLog"];
            if (string.IsNullOrWhiteSpace(setting))
            {
                logger.Warn("No {DisableAccessLog} found in configuration. Will not enable/disable the Access Log");
                return null;
            }

            if (!bool.TryParse(setting, out var doDisable))
            {
                logger.Warn("The value of {DisableAccessLog} is incorrect. The Access Log will be Enabled, by default");
            }

            return doDisable;
        }
    }
}
