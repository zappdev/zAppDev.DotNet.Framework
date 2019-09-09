// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Utilities
{
    [TestClass]
    public class DataAccessContextTest
    {
        [TestMethod]
        public void InitTest()
        {
            var context = new DataAccessContext<object>
            {
                PageIndex = 0, PageSize = 10, Filter = null, SortByColumnName = null
            };
            
            Assert.AreEqual(0, context.PageIndex);
            Assert.AreEqual(10, context.PageSize);
            Assert.AreEqual(null, context.SortByColumnName);
            Assert.AreEqual(null, context.Filter);
        }
        
    }
}