using CLMS.Framework.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CLMS.Framework.Tests.Collections
{
    [TestClass]
    public class IndexedCollectionTest
    {
        public IndexedCollection<string, int> Collection;

        [TestInitialize]
        public void SetUp()
        {
            Collection = new IndexedCollection<string, int>();

            Collection.SetKeyExpression((item) =>
            {
                return item.GetHashCode();
            });
        }

        [TestMethod]
        public void CreateTest()
        {
            Assert.AreEqual(Collection.Length, 0);
        }

        [TestMethod]
        public void AddTest()
        {
            Collection.Add("Name");
            Assert.AreEqual(Collection.Length, 1);

            Collection.Add("Name");
            Assert.AreEqual(Collection.Length, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddNullTest()
        {
            Collection.Add(null);
        }

        [TestMethod]
        public void GetItemsTest()
        {
            Collection.Add("Name");
            var items = Collection.GetItems();

            Assert.AreEqual(1, items.Count);
        }

        [TestMethod]
        public void GetKeysTest()
        {
            Collection.Add("Name");
            var items = Collection.GetKeys();

            Assert.AreEqual(1, items.Count);
        }

        [TestMethod]
        public void ClearTest()
        {
            Collection.Add("Name");
            Assert.AreEqual(Collection.Length, 1);
            Collection.Clear();
            Assert.AreEqual(Collection.Length, 0);
        }

        [TestMethod]
        public void RemoveTest()
        {
            Collection.Add("Name");
            Assert.AreEqual(Collection.Length, 1);
            Collection.Remove("Name");
            Assert.AreEqual(Collection.Length, 0);
        }

        [TestMethod]
        public void RemoveByKeyTest()
        {
            Collection.Add("Name");
            Assert.AreEqual(Collection.Length, 1);
            Collection.RemoveByKey("Name".GetHashCode());
            Assert.AreEqual(Collection.Length, 0);
        }

        [TestMethod]
        public void AddRangeTest()
        {
            Collection.AddRange(new List<string> { "Name", "Email" });
            Assert.AreEqual(Collection.Length, 2);
        }

        [TestMethod]
        public void ContainsTest()
        {
            Collection.AddRange(new List<string> { "Name", "Email" });
            Assert.AreEqual(Collection.Contains("Name"), true);
        }

        [TestMethod]
        public void GetTest()
        {
            Collection.AddRange(new List<string> { "Name", "Email" });
            Assert.AreEqual(Collection.Get("Name".GetHashCode()), "Name");
            Assert.AreEqual(Collection.Get("Name1".GetHashCode()), null);
        }
    }
}
