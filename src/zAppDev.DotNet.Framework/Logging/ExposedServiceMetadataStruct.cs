using System;
using System.IO;
#if NETFRAMEWORK
using System.Web.Http.Filters;
#else
using Microsoft.AspNetCore.Http;
using zAppDev.DotNet.Framework.Mvc.API;
#endif

namespace zAppDev.DotNet.Framework.Logging
{
    public class ExposedServiceMetadataStruct
    {
        public string URL { get; set; }
        public string Method { get; set; }
        public string RequestBody { get; set; }
        public DateTime RequestTimestamp { get; set; }
        public string ResponseBody { get; set; }
        public int ResponseCode { get; set; }
        public double ResponseTime { get; set; }

#if NETFRAMEWORK
        public ExposedServiceMetadataStruct(HttpActionExecutedContext filterContext, TimeSpan elapsed)
        {
            URL = filterContext.Request.RequestUri.ToString();
            Method = filterContext.Request.Method.ToString();

            var httpContext = (System.Web.HttpContextWrapper)filterContext.Request.Properties["MS_HttpContext"];
            RequestTimestamp = httpContext.Timestamp;

            var requestBody = string.Empty;
            if (filterContext.Request.Content != null)
            {
                using (var reader = new StreamReader(filterContext.Request?.Content.ReadAsStreamAsync().Result))
                {
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                    requestBody = reader.ReadToEnd();
                }
            }
            RequestBody = requestBody;

            var responseBody = string.Empty;
            if (filterContext.Response.Content != null)
            {
                responseBody = filterContext.Response.Content.ReadAsStringAsync().Result;
            }
            ResponseBody = responseBody;
            ResponseCode = (int)filterContext.Response.StatusCode;

            ResponseTime = elapsed.TotalMilliseconds;
        }
#else
        public ExposedServiceMetadataStruct()
        {

        }
        public ExposedServiceMetadataStruct(HttpContext context, TimeSpan _elapsed, string response)
        {
            URL = context.Request.Path.Value;
            Method = context.Request.Method;

            string request = string.Empty;
            if (context.Request.ContentLength != null)
            {
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                request = new StreamReader(context.Request.Body).ReadToEnd();
                context.Request.Body.Seek(0, SeekOrigin.Begin);
            }
            RequestBody = request;
            
            if(context.Items.ContainsKey(HttpContextItemKeys.RequestStartedOn)) RequestTimestamp = (DateTime) context.Items["RequestStartedOn"];

            //context.Response.Body.Seek(0, SeekOrigin.Begin);
            //string response = new StreamReader(context.Response.Body).ReadToEnd();
            //context.Response.Body.Seek(0, SeekOrigin.Begin);
            ResponseBody = response;
            ResponseCode = context.Response.StatusCode;
            ResponseTime = _elapsed.TotalMilliseconds;
        }

        public static async System.Threading.Tasks.Task<ExposedServiceMetadataStruct> GetMetadata(HttpContext context, TimeSpan _elapsed, string responseBody)
        {
            var response = new ExposedServiceMetadataStruct();
            response.URL = context.Request.Path.Value;
            response.Method = context.Request.Method;

            string request = string.Empty;
            if (context.Request.ContentLength != null)
            {
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                request = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Seek(0, SeekOrigin.Begin);
            }
            response.RequestBody = request;

            if (context.Items.ContainsKey(HttpContextItemKeys.RequestStartedOn)) response.RequestTimestamp = (DateTime)context.Items["RequestStartedOn"];
            
            response.ResponseBody = responseBody;
            response.ResponseCode = context.Response.StatusCode;
            response.ResponseTime = _elapsed.TotalMilliseconds;

            return response;
        }
#endif
    }
}
