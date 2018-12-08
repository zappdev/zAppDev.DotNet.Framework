#if NETFRAMEWORK
#else
using CLMS.Framework.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Utilities
{

    [TestClass]
    public class ServiceLocatorTest
    {
        private readonly ServiceProvider _serviceProvider;

        public ServiceLocatorTest()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            _serviceProvider = services.BuildServiceProvider();
        }

        [TestMethod]
        public void InitTest()
        {
            var locator = new ServiceLocator(_serviceProvider);

            var logger = locator.GetInstance<IHttpContextAccessor>();

            Assert.IsInstanceOfType(logger, typeof(IHttpContextAccessor));
        }

        [TestMethod]
        public void GetCurrentTest()
        {
            ServiceLocator.SetLocatorProvider(_serviceProvider);

            var logger = ServiceLocator.Current.GetInstance<IHttpContextAccessor>();

            Assert.IsInstanceOfType(logger, typeof(IHttpContextAccessor));

            logger = ServiceLocator.Current.GetInstance(typeof(IHttpContextAccessor)) as IHttpContextAccessor;

            Assert.IsInstanceOfType(logger, typeof(IHttpContextAccessor));
        }
    }
}
#endif