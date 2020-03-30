#if NETFRAMEWORK
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using zAppDev.DotNet.Framework.Services;

namespace zAppDev.DotNet.Framework.Logging
{
    public class Log4NetAPIFileLogger : IAPILogger
    {
        private readonly ILog _logger;
        
        public Log4NetAPIFileLogger(string loggerName)
        {
            _logger = LogManager.GetLogger(loggerName);
        }

        public void Log(string apiType, string apiTitle, LogMessage message, bool throwOnError)
        {
            throw new NotImplementedException();
        }

        public void LogExposedAPIAccess(Guid requestId, HttpActionContext actionContext, TimeSpan processingTime, bool cacheHit)
        {
            throw new NotImplementedException();
        }

        public void LogExposedAPIAccess(Guid requestId, string service, HttpRequestMessage request, HttpResponseMessage response, TimeSpan processingTime, bool throwOnError, bool cacheHit)
        {
            throw new NotImplementedException();
        }

        public void LogExposedAPIInfo(HttpActionExecutedContext filterContext, TimeSpan elapsed)
        {
            var url = filterContext.Request.RequestUri;
            var method = filterContext.Request.Method;

            var requestBody = string.Empty;
            if (filterContext.Request.Content != null)
            {
                using (var reader = new StreamReader(filterContext.Request?.Content.ReadAsStreamAsync().Result))
                {
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                    requestBody = reader.ReadToEnd();
                }
            }

            var responseBody = string.Empty;
            if (filterContext.Response.Content != null)
            {
                responseBody = filterContext.Response.Content.ReadAsStringAsync().Result;
            }
            
            _logger.Info($@"
                {{
                    url : ""{url}"",
                    method : ""{method}"",
                    request : ""{requestBody}"",
                    response : ""{responseBody}"",
                    time : ""{elapsed.TotalMilliseconds}"" ms
                }}
            ");
        }

        public void LogExternalAPIAccess(Guid requestId, string service, string operation, ServiceConsumptionOptions options, object response, HttpStatusCode status, TimeSpan processingTime, bool throwOnError = false, bool cachedResponse = false)
        {
            throw new NotImplementedException();
        }
    }
}
#endif