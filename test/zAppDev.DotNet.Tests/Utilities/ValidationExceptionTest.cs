// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Utilities
{
    [TestClass]
    public class ValidationExceptionTest
    {
        [TestMethod]
        public void InitTest()
        {
            var exp = new ValidationException("Error");

            Assert.AreEqual("Error", exp.Message);
        }
    }
}