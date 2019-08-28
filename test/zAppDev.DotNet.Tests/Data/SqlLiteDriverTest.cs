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
