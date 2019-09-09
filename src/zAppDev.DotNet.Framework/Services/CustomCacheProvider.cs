// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Linq;
using CacheManager.Core;
using WebApi.OutputCache.Core.Cache;
using System.Web.Http;
using CacheManager.Core.Internal;

namespace zAppDev.DotNet.Framework.Services
{
    public class CustomCacheProvider : IApiOutputCache
    {
        protected readonly ICacheManager<object> CacheManagerCache;

        private static readonly IList<string> Keys = new List<string>();

        private ICacheKeyHasher _keyHasher = new CacheKeyHasher();

        public ExpirationMode ExpirationMode { get; set; }
        public TimeSpan ExpirationTimeSpan { get; set; }   //hack, needed 'cause Add expects DateTimeOffset, however CacheManagerCache.Add expects a TimeSpan

        private readonly string _nullCacheObject;

        /// <summary>
        /// Initializes a new CacheProvider that uses CacheManager.
        /// See <see href="http://cachemanager.michaco.net/d"/> for more details
        /// </summary>
        /// <param name="nullCacheObject">Allows null objects to be saved into the cache, as "CLMS_NULL" strings. (Since CacheManager does not allow null cache objects)</param>
        public CustomCacheProvider(string nullCacheObject = "CLMS_NULL")
        {
            _nullCacheObject = nullCacheObject;

            CacheManagerCache = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ICacheManager<object>)) as ICacheManager<object>;
        }

        public void RemoveStartsWith(string key)
        {
            IList<string> keys = Keys.Where(k => k.StartsWith(key)).ToList();
            foreach (var k in keys)
            {
                Remove(k);
            }
        }

        public T Get<T>(string key) where T : class
        {
            return CacheManagerCache.Get<T>(key);
        }

        public T GetGeneric<T>(string key, out bool found)
        {
            found = false;
            try
            {
                var result = CacheManagerCache.Get<T>(key);
                found = result != null;
                return result;
            }
            catch (InvalidCastException)
            {
                var value = CacheManagerCache.Get<string>(key);
                if (value == _nullCacheObject)
                {
                    found = true;
                    return default(T);
                }
                return default(T); //Just return a default object, leaving found as "false"
            }
        }

        public object Get(string key)
        {
            return CacheManagerCache.Get<object>(key);
        }

        public void Remove(string key)
        {
            if(CacheManagerCache.Remove(key))
            {
                Keys.Remove(key);
            }
        }

        public bool Contains(string key)
        {
            return Keys.Contains(key);
            //return CacheManagerCache.Exists(key);
        }

        public void Add(string key, object o, DateTimeOffset expiration, string dependsOnKey = null)
        {
            if (o == null) o = _nullCacheObject;
            var item = new CacheItem<object>(key, o, ExpirationMode, ExpirationTimeSpan);

            CacheManagerCache.Put(item);
            if (!Keys.Contains(key)) Keys.Add(key);
        }



        public IEnumerable<string> AllKeys => Keys;

        public void InvalidateApiCache(string api, string username = null)
        {
            var keysToRemove = Keys.Where(k =>
                {
                    var keySemantics = _keyHasher.SplitToObject(k);

                    return keySemantics.ApiName.Equals(api) &&
                           (username == null || keySemantics.UserName.Equals(username));
                }).ToList();

            foreach (var key in keysToRemove)
            {
                Remove(key);
            }
        }

        public void InvalidateOperationCache(string api, string operation, string username = null)
        {
            var keysToRemove = Keys.Where(k =>
            {
                var keySemantics = _keyHasher.SplitToObject(k);

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