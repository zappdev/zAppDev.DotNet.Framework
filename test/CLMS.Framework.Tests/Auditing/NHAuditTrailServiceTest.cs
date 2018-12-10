using System;
using CLMS.Framework.Auditing;
using CLMS.Framework.Utilities;
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
#endif

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CLMS.Framework.Tests.Auditing
{
    [TestClass]
    public class NHAuditTrailServiceTest
    {
        [TestMethod]
        public void CommonUseCaseTest()
        {
            var mock = new Mock<INHAuditTrailManager>();
            mock.SetupProperty(m => m.IsTemporarilyDisabled, false);

#if NETFRAMEWORK
            var instance = NHAuditTrailService.GetInstance();
            Assert.IsNull(instance);
#endif
#if NETSTANDARD
            Assert.ThrowsException<ArgumentNullException>(NHAuditTrailService.GetInstance);

            var services = new ServiceCollection();
            services.AddSingleton<INHAuditTrailManager>((ins) => mock.Object);
            var buildServiceProvider = services.BuildServiceProvider();
            ServiceLocator.SetLocatorProvider(buildServiceProvider);

            var instance = NHAuditTrailService.GetInstance();
            Assert.IsInstanceOfType(instance, typeof(INHAuditTrailManager));

            NHAuditTrailService.ExecuteWithoutAuditTrail(o =>
            {
                var audit = o as INHAuditTrailManager;

                Assert.AreEqual(true, audit?.IsTemporarilyDisabled);

                return null;
            });

            Assert.ThrowsException<Exception>((() =>
                NHAuditTrailService.ExecuteWithoutAuditTrail(o => throw new Exception())));
#endif
        }
    }
}
