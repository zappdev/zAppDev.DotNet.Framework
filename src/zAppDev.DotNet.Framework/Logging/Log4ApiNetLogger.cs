#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Services;
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

        public async Task LogExposedAPIMetadata(ExposedServiceMetadataStruct metadata)
        {
            await Task.Run( () => {
                _logger.LogInformation(Newtonsoft.Json.JsonConvert.SerializeObject(metadata));
                });
        }

        public void LogExternalAPIAccess(Guid requestId, string service, string operation, ServiceConsumptionOptions options, object response, HttpStatusCode status, TimeSpan processingTime, bool throwOnError = false, bool cachedResponse = false)
        {
            _logger.LogInformation($"{requestId},{service},{operation},{processingTime},{status}");
        }
    }
#endif
}
