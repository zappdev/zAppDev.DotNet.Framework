using System;
using System.Text;

using Newtonsoft.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;

namespace CLMS.Framework.Service
{
    public interface ICacheWrapperService
    {
        T Get<T>(string key);

        void Set<T>(string key, T value);
        
        bool Contains(string key);
    }

    public class CacheWrapperService : ICacheWrapperService
    {
        private static readonly JsonSerializerSettings config = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
        };

        private readonly IDistributedCache _cache;
        private readonly ILogger _logger;

        public CacheWrapperService(
            IDistributedCache cache,
            ILogger<CacheWrapperService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public T Get<T>(string key)
        {
            // Look for cache key.
            var cacheEntry = _cache?.Get(key);

            if (cacheEntry == null) return default(T);
            
            var raw = Encoding.UTF8.GetString(cacheEntry);
            _logger?.LogInformation(1001, "Getting item {ID}", raw);
            return JsonConvert.DeserializeObject<T>(raw, config);

        }

        public void Set<T>(string key, T value)
        {
            var serializedObject = JsonConvert.SerializeObject(value, config);

            var options = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(20));
            var encodedCurrentTimeUtc = Encoding.UTF8.GetBytes(serializedObject);
            _cache?.Set(key, encodedCurrentTimeUtc, options);
        }

        public bool Contains(string key)
        {
            var cacheEntry = _cache?.Get(key);
            return cacheEntry != null;
        }
    }
}