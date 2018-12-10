using System;
using CLMS.Framework.Profiling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Profiling
{
    [TestClass]
    public class LogStatisticTest
    {  
#if NETFRAMEWORK
        [TestMethod]
        public void PropertyTest()
        {
            var info = new LogStatistic
            {
                ModelName = "WebForm",
                SymbolName = "WF",
                SymbolType = AppDevSymbolType.ClassOperation,
                Time = int.MinValue
            };

            Assert.AreEqual(int.MinValue, info.Time);
            Assert.AreEqual("WebForm", info.ModelName);
            Assert.AreEqual(AppDevSymbolType.ClassOperation, info.SymbolType);
            Assert.AreEqual("WF", info.SymbolName);
            Assert.IsNotNull(info.Id);
        }
#endif
    }
}