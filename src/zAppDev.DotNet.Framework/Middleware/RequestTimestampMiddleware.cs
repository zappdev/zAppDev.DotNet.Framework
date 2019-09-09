// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
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