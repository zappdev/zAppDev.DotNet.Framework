using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using zAppDev.DotNet.Framework.Services;
#if NETFRAMEWORK
using Microsoft.Owin;
#endif
using Newtonsoft.Json.Linq;

namespace zAppDev.DotNet.Framework.Logging
{
    public class LogMessage
    {
#if NETFRAMEWORK
        public LogMessage()
        {

        }

        public LogMessage(Guid requestId, HttpRequestMessage request, HttpResponseMessage response, TimeSpan processingTime, bool cacheHit)
        {
            var requestMessage = GetObjectFromContent(request.Content);
            var responseMessage = GetObjectFromContent(response?.Content);
            var httpRequestContext = request.GetRequestContext();

            Service = request.GetActionDescriptor().ControllerDescriptor.ControllerName;
            Operation = request.GetActionDescriptor().ActionName;
            StatusCode = response != null ? (int)response?.StatusCode : -1;
            RequestMethod = request.Method.Method;
            RequestPath = request.RequestUri.LocalPath;
            RequestUri = request.RequestUri.PathAndQuery;
            Message = response?.StatusCode.ToString();
            ElapsedMsecs = processingTime.TotalMilliseconds;
            CacheHit = cacheHit;
            RequestId = requestId;
            IP = GetClientIpAddress(request);
            Timestamp = DateTime.UtcNow;

            Username = httpRequestContext.Principal.Identity.IsAuthenticated
                ? httpRequestContext.Principal.Identity.Name
                : "Anonymous";
            Request = requestMessage;
            Response = responseMessage;
        }

        public string Service
        {
            get;
            set;
        }
        public string Operation
        {
            get;
            set;
        }
        public int StatusCode
        {
            get;
            set;
        }
        public string RequestMethod
        {
            get;
            set;
        }
        public string RequestPath
        {
            get;
            set;
        }
        public string RequestUri
        {
            get;
            set;
        }
        public string Message
        {
            get;
            set;
        }
        public double ElapsedMsecs
        {
            get;
            set;
        }

        public Guid RequestId
        {
            get;
            set;
        }

        public Guid ExposedApiCorrelationId
        {
            get;
            set;
        }

        public string Username
        {
            get;
            set;
        }
        public string IP
        {
            get;
            set;
        }
        public DateTime Timestamp
        {
            get;
            set;
        }
        public object Request
        {
            get;
            set;
        }
        public object Response
        {
            get;
            set;
        }

        public bool CacheHit
        {
            get;
            set;
        }

        public override string ToString()
        {
            return $"{RequestMethod} {RequestUri} - {StatusCode} {Message} - {ElapsedMsecs} msecs, user: {Username}, ip: {IP}";
        }

        public string ToFullString()
        {
            return $"{this}\r\n{Timestamp.ToString("s", System.Globalization.CultureInfo.InvariantCulture)} {RequestId}\r\nreq Uri: {RequestUri}, req Body: {Request}\r\nresp: {Response}";
        }

        public static LogMessage CreateMessage(Guid requestId, HttpRequestMessage request, HttpResponseMessage response, TimeSpan processingTime, bool cacheHit)
        {
            return new LogMessage(requestId, request, response, processingTime, cacheHit);
        }

        private static bool IsSimple(Type type)
        {
            if (type == null)
            {
                return false;
            }

            while (true)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // nullable type, check if the nested type is simple.
                    type = type.GetGenericArguments()[0];
                    continue;
                }

                return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal);
            }
        }

        public static LogMessage CreateMessage(Guid requestId,
            ServiceConsumptionOptions options,
            string service, string operation,
            object responseMessage,
            HttpStatusCode statusCode,
            TimeSpan processingTime,
            bool cachedResponse)
        {
            var logMessage = new LogMessage
            {
                Service = service,
                Operation = operation,
                StatusCode = (int)statusCode,
                RequestMethod = options.Verb.ToString(),
                RequestPath = options.Url,
                RequestUri = options.Url,
                Message = statusCode.ToString(),
                ElapsedMsecs = processingTime.TotalMilliseconds,
                CacheHit = cachedResponse,
                RequestId = requestId,
                //IP = GetClientIpAddress(request),
                Timestamp = DateTime.UtcNow,
                Username = HttpContext.Current?.User?.Identity.IsAuthenticated == true
                    ? HttpContext.Current.User.Identity.Name
                    : "Anonymous",
                Request = new JObject { ["ConsumerRequest"] = JToken.FromObject(options) },
                Response = responseMessage
            };

            if (responseMessage != null && IsSimple(responseMessage.GetType()))
            {
                logMessage.Response = new JObject { ["Data"] = JToken.FromObject(responseMessage) };
            }

            var requestMessage = HttpContext.Current?.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
            if (requestMessage?.Properties["exposed-service-requestId"] != null)
            {
                logMessage.ExposedApiCorrelationId = (Guid)requestMessage?.Properties["exposed-service-requestId"];
            }

            return logMessage;
        }

        private static string GetClientIpAddress(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextBase)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            return request.Properties.ContainsKey("MS_OwinContext")
                ? IPAddress.Parse(((OwinContext)request.Properties["MS_OwinContext"]).Request.RemoteIpAddress).ToString()
                : null;
        }

        private static object GetObjectFromContent(HttpContent content)
        {
            if (content == null)
            {
                return null;
            }

            var objectContent = content as ObjectContent;
            if (objectContent != null) return objectContent.Value;

            using (var stream = content.ReadAsStreamAsync().Result)
            {
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    var readToEnd = reader.ReadToEnd();
                    try
                    {
                        if (string.IsNullOrEmpty(readToEnd)) return null;

                        if (readToEnd.Trim().StartsWith("["))
                        {
                            return JArray.Parse(readToEnd);
                        }

                        return JObject.Parse(readToEnd);
                    }
                    catch (Exception)
                    {
                        return readToEnd;
                    }
                }
            }
        }
#endif
    }
}