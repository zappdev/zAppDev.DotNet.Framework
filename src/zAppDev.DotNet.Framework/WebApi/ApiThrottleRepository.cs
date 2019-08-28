#if NETFRAMEWORK
using CacheManager.Core;
using System;
using System.Web.Http;
using WebApiThrottle;

namespace zAppDev.DotNet.Framework.WebApi
{
    public class ApiThrottleRepository : IThrottleRepository
    {
        private readonly ICacheManager<object> _cache;
        private readonly string _regionName = "_ThrottleRepositoy";

        public ApiThrottleRepository()
        {
            _cache = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ICacheManager<object>)) as ICacheManager<object>;
        }

        public bool Any(string id)
        {
            return _cache.Exists(id, _regionName);
        }

        public void Clear()
        {
            _cache.ClearRegion(_regionName);
        }

        public ThrottleCounter? FirstOrDefault(string id)
        {
            return _cache.Get<ThrottleCounter?>(id, _regionName);
        }

        public void Remove(string id)
        {
            _cache.Remove(id, _regionName);
        }

        public void Save(string id, ThrottleCounter throttleCounter, TimeSpan expirationTime)
        {
            var a = new CacheItem<object>(id, _regionName, throttleCounter, ExpirationMode.Absolute, expirationTime);
            _cache.AddOrUpdate(a, (item) =>
            {
                var counter = (ThrottleCounter)item;
                counter.Timestamp = throttleCounter.Timestamp;
                counter.TotalRequests = throttleCounter.TotalRequests;
                return counter;
            });
        }
    }
}
#endif