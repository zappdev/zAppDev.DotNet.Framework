// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System.Net.Http;
using zAppDev.DotNet.Framework.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Services
{
    [TestClass]
    public class CacheKeyHasherTest
    {
        [TestMethod]
        public void PropertyTest() 
        {
            var hasher = new CacheKeyHasher{
                ApiName = "Api",
                UserName = "theofilis",
                Operation = "AddItem",
                OriginalKey = "1"
            };

            Assert.AreEqual("Api|theofilis|AddItem|c4ca4238a0b923820dcc509a6f75849b", hasher.GetHashedKey());

        }

        [TestMethod]
        public void ServiceConsumptionContainerTest()
        {
            var context = new ServiceConsumptionContainer
            {
                HttpResponseMessage = new HttpResponseMessage()
            };

            Assert.IsNotNull(context.HttpResponseMessage);
        }
    }
}
