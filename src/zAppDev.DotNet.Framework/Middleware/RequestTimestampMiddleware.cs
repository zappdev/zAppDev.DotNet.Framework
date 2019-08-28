#if NETFRAMEWORK
#else
using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace zAppDev.DotNet.Framework.Middleware
{
    public class RequestTimestampMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestTimestampMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            context.Items.Add("RequestStartedOn", DateTime.UtcNow);

            // Call the next delegate/middleware in the pipeline
            return this._next(context);
        }
    }

    public static class RequestTimestampMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestTimestamp(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestTimestampMiddleware>();
        }
    }
}
#endif