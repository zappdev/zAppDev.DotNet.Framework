// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Data;
using zAppDev.DotNet.Framework.Tests.Data.POCO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Data
{
    [TestClass]
    public class ObjectGraphWalkerTest
    {
        [TestMethod]
        public void IsPersistedTest()
        {
            var obj = new ObjectGraphWalker();

            var manager = new MiniSessionManager();
            var session = manager.OpenSession();

            Assert.AreEqual(null, obj.IsPersisted(new User(), session));
        }

        [TestMethod]
        public void AssociateGraphWithSessionTest()
        {
            var obj = new ObjectGraphWalker();

            var manager = new MiniSessionManager();
            var session = manager.OpenSession();

            Assert.AreEqual(null, obj.IsPersisted(new User(), session));
            var user = obj.AssociateGraphWithSession(new User(), manager);
            //Assert.AreEqual("", user.Name);
        }
    }
}
