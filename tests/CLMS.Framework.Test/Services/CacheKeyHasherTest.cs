using CLMS.Framework.Service;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Test.Services
{
    [TestClass]
    public class CacheWrapperServiceTest
    {
        private readonly CacheWrapperService _cacheWrapper;

        public CacheWrapperServiceTest()
        {
            var services = new ServiceCollection();

            services.AddDistributedMemoryCache();

            var provider = services.BuildServiceProvider();

            _cacheWrapper = new CacheWrapperService(provider.GetService<IDistributedCache>(), null);
        }
        
        [TestMethod]
        public void ContainsTest()
        {
            const string key = "Key";
            Assert.IsFalse(_cacheWrapper.Contains(key));
            
            _cacheWrapper.Set(key, 1);            
            Assert.IsTrue(_cacheWrapper.Contains(key));
            
            var result = _cacheWrapper.Get<int>(key);            
            Assert.AreEqual(1, result);
            
            result = _cacheWrapper.Get<int>("not");            
            Assert.AreEqual(0, result); // 0 is the default value of int type
        }        
    }
}
