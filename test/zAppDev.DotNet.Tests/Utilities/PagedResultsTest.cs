using System.Collections.Generic;
using zAppDev.DotNet.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Utilities
{
    [TestClass]
    public class PagedResultsTest
    {
        [TestMethod]
        public void PropertyTest()
        {
            var items = new PagedResults<string>
            {
                TotalResults = 1,
                Results = new List<string> { "item" }
            };

            Assert.AreEqual(1, items.TotalResults);
            Assert.AreEqual("item", items.Results[0]);
        }
    }
}
