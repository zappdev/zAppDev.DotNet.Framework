#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Caching;
using log4net;

namespace zAppDev.DotNet.Framework.Identity
{
    public class AuthorizationCacheManager
    {
        private readonly MemoryCache _memoryCache = MemoryCache.Default;
        private readonly CacheExpiration _cacheExpiration = new CacheExpiration();
        private readonly ILog _log;

        public AuthorizationCacheManager()
        {
            _log = LogManager.GetLogger(this.GetType());
            this._cacheExpiration = new CacheExpiration();
        }

        public CacheExpiration CacheExpiration => _cacheExpiration;

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }

        public T GetFromCache<T>(string key)
        {
            if (_memoryCache.Contains(key))
                return (T)Convert.ChangeType(_memoryCache[key], typeof(T));
            return default(T);
        }

        public T PutToCache<T>(string key, T item, IList<string> filePaths)
        {
            var policy = new CacheItemPolicy();
            if (this._cacheExpiration.ExpirationType == CacheExpirationType.Absolute)
                policy.AbsoluteExpiration = this._cacheExpiration.AbsoluteExpiration;
            else
                policy.SlidingExpiration = this._cacheExpiration.SlidingExpiration;
            policy.RemovedCallback = arguments => _log.DebugFormat("Removed from Identity Cache - Reason: {0}, Key-Name: {1} | Value-Object: {2}",
                                     arguments.RemovedReason, arguments.CacheItem.Key, arguments.CacheItem.Value);
            _memoryCache.Set(key, item, policy);
            return item;
        }

        public T PutToCache<T>(string key, T item)
        {
            return PutToCache(key, item, new List<string>());
        }
    }

    public enum CacheExpirationType
    {
        Absolute,
        Sliding
    }

    public class CacheExpiration
    {
        public CacheExpirationType ExpirationType
        {
            get;
            set;
        }
        public int ExpirationTime
        {
            get;
            set;
        }

        public CacheExpiration()
        {
            //Default Values
            ExpirationType = CacheExpirationType.Absolute;
            ExpirationTime = 60;
            string appSettingMetaCacheExpirationType = ConfigurationManager.AppSettings["CacheExpirationType"];
            string appSettingMetaCacheExpirationTime = ConfigurationManager.AppSettings["CacheExpirationTime"];
            //Expiration Type
            if (!string.IsNullOrEmpty(appSettingMetaCacheExpirationType))
                if (!Enum.IsDefined(typeof(CacheExpirationType), appSettingMetaCacheExpirationType))
                    ExpirationType = CacheExpirationType.Absolute;
                else
                    ExpirationType = (CacheExpirationType)Enum.Parse(typeof(CacheExpirationType), appSettingMetaCacheExpirationType);
            //Expiration in seconds
            if (!string.IsNullOrEmpty(appSettingMetaCacheExpirationTime))
            {
                int tryExpirationTime;
                if (int.TryParse(appSettingMetaCacheExpirationTime, out tryExpirationTime))
                    ExpirationTime = tryExpirationTime;
            }
        }

        public DateTimeOffset AbsoluteExpiration
        {
            get
            {
                return DateTime.UtcNow.AddSeconds(ExpirationTime);
            }
        }

        public TimeSpan SlidingExpiration
        {
            get
            {
                return TimeSpan.FromSeconds(ExpirationTime);
            }
        }
    }

}

#endif