using System;
using System.IO;
using CLMS.AppDev.Cache;
using zAppDev.DotNet.Framework.Utilities;
#if NETFRAMEWORK

#else
using Moq;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

#endif

namespace zAppDev.DotNet.Framework.Tests.Utilities
{
    public class Helper
    {
#if NETFRAMEWORK
#else
        public static void HttpCoreSimulate(Func<DefaultHttpContext> getContext)
        {

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockEnvironment = new Mock<IHostingEnvironment>();
            var context = getContext();

            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);
            mockEnvironment.Setup(_ => _.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var services = new ServiceCollection();
            services.AddSingleton(ins => mockHttpContextAccessor.Object);
            services.AddSingleton(ins => mockEnvironment.Object);
            services.AddSingleton<ICache<string>>(ins => new InMemoryCache());
            ServiceLocator.SetLocatorProvider(services.BuildServiceProvider());
        }
#endif
    }
}
