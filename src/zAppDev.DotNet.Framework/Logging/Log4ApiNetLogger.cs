#if NETFRAMEWORK
#else
using Microsoft.Extensions.Logging;
using System;
#endif

namespace zAppDev.DotNet.Framework.Logging
{
#if NETFRAMEWORK
#else
    public class Log4ApiNetLogger : IAPILogger
    {
        private readonly ILogger _logger;

        public Log4ApiNetLogger (ILogger<Log4ApiNetLogger> logger)
        {
            _logger = logger;
        }

        public void LogExposedAPIAccess(string controller, string action, Guid requestId, TimeSpan processingTime, bool cacheHit)
        {
            _logger.LogInformation($"{controller},{action},{requestId},{processingTime},{cacheHit}");
        }
    }
#endif
}
