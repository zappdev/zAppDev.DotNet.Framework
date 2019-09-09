// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
using zAppDev.DotNet.Framework.Logging;
using System;
using System.Net.Http;
using System.Web.Http;
using WebApiThrottle;

namespace zAppDev.DotNet.Framework.WebApi
{
    public class ApiThrottleLogger : IThrottleLogger
    {
        private IAPILogger _apiLogger;

        public ApiThrottleLogger()
        {
            _apiLogger = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IAPILogger)) as IAPILogger;
        }

        public void Log(ThrottleLogEntry entry)
        {
            if (entry.Request == null) return;
            var message = string.Format("{0} Request {1} from {2} has been throttled (blocked), quota {3}/{4} exceeded by {5}",
                                        entry.LogDate, entry.RequestId, entry.ClientIp, entry.RateLimit, entry.RateLimitPeriod, entry.TotalRequests);
            var response
                = entry.Request.CreateErrorResponse((System.Net.HttpStatusCode)429, message);
            //todo: better logic to get controller and action....
            var service = "";
            var url = entry.Request.RequestUri.LocalPath;
            var indexOfApi = url.IndexOf("api");
            if (indexOfApi > -1)
            {
                var part = url.Substring(indexOfApi + 3);
                var pieces = part.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (pieces.Length > 1)
                {
                    service = pieces[0] + "." + pieces[1];
                }
            }
            _apiLogger.LogExposedAPIAccess(Guid.NewGuid(), service, entry.Request, response, entry.Duration, false, false);
        }
    }
}
#endif