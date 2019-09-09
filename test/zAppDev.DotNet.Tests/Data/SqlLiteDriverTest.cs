// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Data
{
    [TestClass]
    public class SqlLiteDriverTest
    {
        [TestMethod]
        public void PropertiesTest()
        {
            var driver = new SqlLiteDriver();

            Assert.AreEqual(true, driver.UseNamedPrefixInSql);
            Assert.AreEqual(true, driver.UseNamedPrefixInParameter);
            Assert.AreEqual("@", driver.NamedPrefix);
            Assert.AreEqual(false, driver.SupportsMultipleOpenReaders);
            Assert.AreEqual(true, driver.SupportsMultipleQueries);
            Assert.AreEqual(false, driver.SupportsNullEnlistment);
            Assert.AreEqual(true, driver.HasDelayedDistributedTransactionCompletion);
        }
    }
}
