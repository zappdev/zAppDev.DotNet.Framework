#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
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

        public void LogExposedAPIInfo(HttpContext context, TimeSpan _elapsed)
        {
            var url = context.Request.Path.Value;
            var method = context.Request.Method;

            string request = string.Empty;
            if (context.Request.ContentLength != null)
            {
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                request = new StreamReader(context.Request.Body).ReadToEnd();
                context.Request.Body.Seek(0, SeekOrigin.Begin);
            }

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            string response = new StreamReader(context.Response.Body).ReadToEnd();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            var timeInms = _elapsed.TotalMilliseconds;

            _logger.LogInformation($@"
            {{
                url: ""{url}"",
                method: ""{method}"",
                reqBody: ""{request}"",
                resp: ""{response}"",
                time: ""{timeInms} ms""
            }}
            ");
        }

        public void LogExternalAPIAccess(Guid requestId, string service, string operation, ServiceConsumptionOptions options, object response, HttpStatusCode status, TimeSpan processingTime, bool throwOnError = false, bool cachedResponse = false)
        {
            _logger.LogInformation($"{requestId},{service},{operation},{processingTime},{status}");
        }
    }
#endif
}
