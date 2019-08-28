using zAppDev.DotNet.Framework.Data;
using zAppDev.DotNet.Framework.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Logging
{
    [TestClass]
    public class NHStatisticsLoggerTest
    {
        [TestMethod]
        public void PrintGlobalStatisticsTest()
        {
            var manager = new MiniSessionManager();

            var session = manager.OpenSession();

            session.PrintGlobalStatistics();
        }

        [TestMethod]
        public void PrintSessionStatisticsTest()
        {
            var manager = new MiniSessionManager();

            var session = manager.OpenSession();

            session.PrintSessionStatistics();
        }
    }
}
