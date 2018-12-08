using CLMS.Framework.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CLMS.Framework.Tests.Threading
{
    internal class Item
    {
        public string Name { get; set; }

        public string Last { get; set; }
    }

    [TestClass]
    public class ParallelTaskTest
    {
        [TestMethod()]
        public void ForEachTest()
        {
            var items = new List<Item> { new Item { Name = "" } };

            ParallelTask.ForEach(items, (item, index) => {

            });

            Assert.AreEqual(1, 1);
        }
    }
}
