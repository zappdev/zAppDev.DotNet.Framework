using CLMS.Framework.Data;
using CLMS.Framework.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Logging
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
