// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.

using zAppDev.DotNet.Framework.Utilities;
#if NETSTANDARD
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
#endif
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Utilities
{

    [TestClass]
    public class ServiceLocatorTest
    {
#if NETSTANDARD
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
#endif
    }
}