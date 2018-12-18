﻿using System.Threading.Tasks;
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
            _middleware = new ApiMiddleware(async (next) => { });
        }

        [TestMethod]
        public async Task NextTest()
        {
            var task = _middleware.Invoke(null);
            await task;
            Assert.IsNotNull(task);
        }
#endif
    }
}
