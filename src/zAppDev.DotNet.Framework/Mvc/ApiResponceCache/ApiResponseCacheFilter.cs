// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Mvc.API
{
    public class ApiResponseCacheFilter : Attribute, IResourceFilter
    {
        public bool NoCache { get; set; }
        public int SharedTimeSpan { get; set; }
        public int ClientTimeSpan { get; set; }
        public int ServerTimeSpan { get; set; }
        public bool ExcludeQueryStringFromCacheKey { get; set; }
        public bool MustRevalidate { get; set; }
        public bool AnonymousOnly { get; set; }
        public bool Private { get; set; }
        public bool CachePerUser { get; set; }
        public bool LogEnabled { get; set; }
        public Type CacheKeyGenerator { get; set; }
        public Type CustomTimeSpanMethodClassType { get; set; }
        public string CustomTimeSpanMethodName { get; set; }
        public ExpirationMode ExpirationMode { get; set; }
        public string ApiName { get; set; }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("actionContext");

            EnableOutputCaching(context);
            ServeFromCache(context);
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }

        public void EnableOutputCaching(ResourceExecutingContext context)
        {
            var action = (string)context.RouteData.Values["Action"];
            var controller = (string)context.RouteData.Values["Controller"];

            context.HttpContext.EnableOutputCaching(
                NoCache,
                SharedTimeSpan,
                ClientTimeSpan,
                ServerTimeSpan,
                ExcludeQueryStringFromCacheKey,
                MustRevalidate,
                AnonymousOnly,
                CacheKeyGenerator,
                Private,
                CustomTimeSpanMethodClassType,
                CustomTimeSpanMethodName,
                CachePerUser,
                ExpirationMode,
                ApiName,
                LogEnabled,
                action,
                controller);
        }

        private void ServeFromCache(ResourceExecutingContext context)
        {
            var httpContext = context.HttpContext;

            var cache = context.HttpContext.RequestServices.GetRequiredService<IApiCacheService>();  //ServiceLocator.Current.GetInstance<IApiCacheService>();

            if (cache.TryGetValue(httpContext, out var value))
            {
                foreach (string name in value.Headers.Keys)
                {
                    if (!httpContext.Response.Headers.ContainsKey(name))
                    {
                        httpContext.Response.Headers[name] = value.Headers[name];
                    }
                }

                context.Result = new ContentResult()
                {
                    Content = Encoding.UTF8.GetString(value.Body, 0, value.Body.Length),
                    ContentType =  value.Headers.ContainsKey("Content-Type") ?  value.Headers["Content-Type"] : null
                };

                httpContext.Items[HttpContextItemKeys.HitCache] = true;
            }
            else
            {
                httpContext.Items[HttpContextItemKeys.HitCache] = false;
            }
        }
    }
}
#endif