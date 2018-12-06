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
    }

    public class CacheWrapperService : ICacheWrapperService
    {
        internal static readonly JsonSerializerSettings config = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
        };

        private IDistributedCache _cache;
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
            var cacheEntry = _cache.Get(key);

            if (cacheEntry != null)
            {
                var raw = Encoding.UTF8.GetString(cacheEntry);
                _logger.LogInformation(1001, "Getting item {ID}", raw);
                return JsonConvert.DeserializeObject<T>(raw, config);
            }

            return default(T);
        }

        public void Set<T>(string key, T value)
        {
            var serializedObject = JsonConvert.SerializeObject(value, config);

            var options = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(20));
            byte[] encodedCurrentTimeUTC = Encoding.UTF8.GetBytes(serializedObject);
            _cache.Set(key, encodedCurrentTimeUTC, options);
        }
    }
}