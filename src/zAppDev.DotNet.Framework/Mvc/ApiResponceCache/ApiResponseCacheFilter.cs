#if NETFRAMEWORK
#else
using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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

            var cache = ServiceLocator.Current.GetInstance<IApiCacheService>();

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
                    ContentType = value.Headers["Content-Type"]
                };

                httpContext.Items[ApiManagerItemsKeys.HitCache] = true;
            }
            else
            {
                httpContext.Items[ApiManagerItemsKeys.HitCache] = false;
            }
        }
    }
}
#endif