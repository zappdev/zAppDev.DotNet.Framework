#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Http;
using System;

namespace zAppDev.DotNet.Framework.Mvc.API
{
    public static class ApiResponseCacheFeatureExtensions
    {
        public static void EnableOutputCaching(this HttpContext context, ApiResponseCacheConfig config)
        {
            context.EnableOutputCaching(
                config.NoCache,
                config.SharedTimeSpan,
                config.ClientTimeSpan,
                config.ServerTimeSpan,
                config.ExcludeQueryStringFromCacheKey,
                config.MustRevalidate,
                config.AnonymousOnly,
                config.CacheKeyGenerator,
                config.Private,
                config.CustomTimeSpanMethodClassType,
                config.CustomTimeSpanMethodName,
                config.CachePerUser,
                config.ExpirationMode,
                config.ApiName,
                config.LogEnabled,
                null,
                null);
        }

        public static void EnableOutputCaching(this HttpContext context,
            bool noCache,
            int sharedTimeSpan,
            int clientTimeSpan,
            int serverTimeSpan,
            bool excludeQueryStringFromCacheKey,
            bool mustRevalidate,
            bool anonymousOnly,
            Type cacheKeyGenerator,
            bool @private,
            Type customTimeSpanMethodClassType,
            string customTimeSpanMethodName,
            bool cachePerUser,
            ExpirationMode expirationMode,
            string apiName,
            bool logEnabled,
            string actionName,
            string controllerName)
        {
            var feature = context.Features.Get<ApiResponseCacheConfig>();

            if (feature == null)
            {
                feature = new ApiResponseCacheConfig();
                context.Features.Set(feature);
            }

            feature.AnonymousOnly = anonymousOnly;
            feature.NoCache = noCache;
            feature.SharedTimeSpan = sharedTimeSpan;
            feature.ClientTimeSpan = clientTimeSpan;
            feature.ServerTimeSpan = serverTimeSpan;
            feature.ExcludeQueryStringFromCacheKey = excludeQueryStringFromCacheKey;
            feature.MustRevalidate = mustRevalidate;
            feature.CacheKeyGenerator = cacheKeyGenerator;
            feature.Private = @private;
            feature.CustomTimeSpanMethodClassType = customTimeSpanMethodClassType;
            feature.CustomTimeSpanMethodName = customTimeSpanMethodName;
            feature.CachePerUser = cachePerUser;
            feature.ExpirationMode = expirationMode;
            feature.ApiName = apiName;
            feature.LogEnabled = logEnabled;
            feature.ActionName = actionName;
            feature.ControllerName = controllerName;
        }

        internal static bool IsOutputCachingEnabled(this HttpContext context)
        {
            return context.IsOutputCachingEnabled(out var _);
        }

        internal static bool IsOutputCachingEnabled(this HttpContext context, out ApiResponseCacheConfig config)
        {
            config = context.Features.Get<ApiResponseCacheConfig>();

            return config != null;
        }
    }
}
#endif