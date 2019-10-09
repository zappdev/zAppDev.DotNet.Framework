#if NETFRAMEWORK
#else
using System;
using CacheManager.Core;
using Microsoft.AspNetCore.Http;

namespace zAppDev.DotNet.Framework.Mvc.API
{
    public class CacheManagerApiCacheService : IApiCacheService
    {
        private readonly ICacheManager<ApiCacheOutput> _cacheManager;

        private readonly IApiCacheKeyGenerator _cacheKey;

        public CacheManagerApiCacheService(
            ICacheManager<ApiCacheOutput> cacheManager,
            IApiCacheKeyGenerator cacheKey)
        {
            _cacheManager = cacheManager;
            _cacheKey = cacheKey;
        }

        public void Clear()
        {
            _cacheManager.Clear();
        }

        public void Set(HttpContext context, ApiCacheOutput response)
        {
            if (context.IsOutputCachingEnabled(out var options))
            {
                var cacheMode = (CacheManager.Core.ExpirationMode)options.ExpirationMode;
                var cacheTimeout = TimeSpan.FromSeconds(options.ServerTimeSpan);

                _cacheManager.Add(new CacheItem<ApiCacheOutput>(
                   _cacheKey.MakeCacheKey(context),
                    response,
                    cacheMode,
                    cacheTimeout));
            }
        }

        public bool TryGetValue(HttpContext context, out ApiCacheOutput response)
        {
            response = _cacheManager.Get(_cacheKey.MakeCacheKey(context)) as ApiCacheOutput;
            return response != null;
        }
    }
}
#endif