// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Data
{
    [TestClass]
    public class LocalTest
    {
        [TestMethod]
        public void GetDataTest()
        {
            Assert.IsNotNull(Local.Data);
        }
    }

    [TestClass]
    public class LocalDataTest
    {
        [TestMethod]
        public void GetDataTest()
        {
#if NETFRAMEWORK
            Assert.IsFalse(LocalData.RunningInWeb);
#endif
        }

        [TestMethod]
        public void ClearTest()
        {
#if NETFRAMEWORK
            Local.Data.Clear();
            Assert.AreEqual(0, Local.Data.Count);
#endif
        }

        [TestMethod]
        public void AccessTest()
        {
#if NETFRAMEWORK
            Local.Data["Key"] = 10;
            Assert.AreEqual(10, Local.Data["Key"]);
#endif
        }
    }
}
