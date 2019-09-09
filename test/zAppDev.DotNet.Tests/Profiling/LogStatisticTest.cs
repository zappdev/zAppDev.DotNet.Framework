// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using zAppDev.DotNet.Framework.Profiling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Profiling
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