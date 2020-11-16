#if NETFRAMEWORK
#else
using System;
using System.Collections.Generic;
using System.Linq;
using CacheManager.Core;
using Microsoft.AspNetCore.Http;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Mvc.API
{
    public class CacheManagerApiCacheService : IApiCacheService
    {
        private readonly ICacheManager<ApiCacheOutput> _cacheManager;

        private readonly IApiCacheKeyGenerator _cacheKey;

        private static readonly ISet<string> Keys = new HashSet<string>();

        private ICacheKeyHasher _keyHasher = new Services.CacheKeyHasher();

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

                string itemKey = _cacheKey.MakeCacheKey(context);
                _cacheManager.Add(new CacheItem<ApiCacheOutput>(
                   itemKey,
                    response,
                    cacheMode,
                    cacheTimeout));
                if(!Keys.Contains(itemKey)) Keys.Add(itemKey);
            }
        }

        public bool TryGetValue(HttpContext context, out ApiCacheOutput response)
        {
            response = _cacheManager.Get(_cacheKey.MakeCacheKey(context)) as ApiCacheOutput;
            return response != null;
        }

        public void InvalidateApiCache(string api, string username = null)
        {
            var keysToRemove = Keys.Where(k =>
            {
                var keySemantics = ServiceLocator.Current.GetInstance<ICacheKeyHasher>().SplitToObject(k);

                return keySemantics.ApiName.Equals(api) &&
                       (username == null || keySemantics.UserName.Equals(username));
            }).ToList();

            foreach (var key in keysToRemove)
            {
                Remove(key);
            }
        }

        private void Remove(string key)
        {
            if(_cacheManager.Remove(key))
            {
                Keys.Remove(key);
            }
        }

        public void InvalidateOperationCache(string api, string operation, string username = null)
        {
            var keysToRemove = Keys.Where(k =>
            {
                var keySemantics = ServiceLocator.Current.GetInstance<ICacheKeyHasher>().SplitToObject(k);

                return keySemantics.ApiName.Equals(api) &&
                       keySemantics.Operation.Equals(operation) &&
                       (username == null || keySemantics.UserName.Equals(username));
            }).ToList();

            foreach (var key in keysToRemove)
            {
                Remove(key);
            }
        }
    }
}
#endif