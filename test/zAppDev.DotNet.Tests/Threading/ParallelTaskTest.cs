// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using zAppDev.DotNet.Framework.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Tests.Threading
{
    internal class Item
    {
        public string Name;

        public string Last;
    }

    [TestClass]
    public class ParallelTaskTest
    {
        [TestMethod]
        public void ForEachTest()
        {
            var items = new List<Item> { new Item { Name = "" } };

            ParallelTask.ForEach(items, (item, index) => {

            });

            Assert.ThrowsException<AggregateException>(() =>
            {
                ParallelTask.ForEach(items, (item, index) => throw new Exception());
            });

            Assert.AreEqual(1, 1);
        }
    }
}
