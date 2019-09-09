// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Data;
using zAppDev.DotNet.Framework.Tests.Data.POCO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Data
{
    [TestClass]
    public class NhExtensionsTest
    {
        [TestMethod]
        public void IsDirtyEntityTest()
        {
            var manager = new MiniSessionManager();
            var session = manager.OpenSession();

            Assert.IsTrue(session.IsDirtyEntity(new User()));
        }

        [TestMethod]
        public void IsDirtyPropertyTest()
        {
            var manager = new MiniSessionManager();
            var session = manager.OpenSession();

            Assert.IsFalse(session.IsDirtyProperty(new User(), "Name"));
        }

        [TestMethod]
        public void GetOriginalEntityPropertyTest()
        {
            var manager = new MiniSessionManager();
            var session = manager.OpenSession();

            Assert.AreEqual(null, session.GetOriginalEntityProperty(new User(), "Name"));
        }


        
    }
}
