// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Services
{
    [TestClass]
    public class OAuth2Test
    {
        [TestMethod]
        public void InitTest()
        {
            var token = new OAuth2TokenData();
            
            token.Parse("{'access_token': 'ALM'}");
            
            Assert.AreEqual("ALM", token.Token);

            var auth2ReturnUrl = new OAuth2ReturnUrl("www.clms.gr");
            Assert.AreEqual("www.clms.gr", auth2ReturnUrl.ReturnUrl);

            var code = new OAuth2Code("ABS");
            Assert.AreEqual("ABS", code.Code);
        }
    }
}