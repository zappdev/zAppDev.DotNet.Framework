using Microsoft.VisualStudio.TestTools.UnitTesting;

#if NETSTANDARD
using CLMS.Framework.Middleware;
#endif

namespace CLMS.Framework.Tests.Middleware
{
    [TestClass]
    public class ApiMiddlewareTest
    {
#if NETSTANDARD
        private ApiMiddleware _middleware;

        [TestInitialize]
        public void Init()
        {
            _middleware = new ApiMiddleware((next) => null);
        }

        [TestMethod]
        public void NextTest()
        {
            Assert.IsNotNull(_middleware.Invoke(null));
        }
#endif
    }
}
