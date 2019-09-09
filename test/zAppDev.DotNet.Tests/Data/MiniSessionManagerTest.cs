// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using zAppDev.DotNet.Framework.Data;
using zAppDev.DotNet.Framework.Data.DAL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Data
{
    [TestClass]
    public class MiniSessionManagerTest
    {     
        [TestInitialize]
        public void Initialize()
        {
        }

        [TestMethod]
        public void PropertiesTest()
        {
#if NETFRAMEWORK
            Assert.IsNotNull(MiniSessionManager.OwinKey);
            Assert.IsFalse(MiniSessionManager.DatabaseUpdateFailed);
#endif
        }

        [TestMethod]
        public void InitTest()
        {
#if NETFRAMEWORK
            var manager = new MiniSessionManager();
            Assert.IsNotNull(manager.InstanceId);

            manager = MiniSessionManager.Create();
            Assert.IsNotNull(manager.InstanceId);

            manager.WillFlush = false;
            Assert.IsFalse(manager.WillFlush);

            Assert.IsNull(manager.SingleUseSession);
            manager.SingleUseSession = false;
            Assert.IsFalse(manager.SingleUseSession.Value);
#endif
        }

        [TestMethod]
        public void FlowTest()
        {
            var manager = new MiniSessionManager();

            var session = manager.OpenSession();

            var sessionNew = manager.OpenSession();

            Assert.AreSame(session, sessionNew);

            manager.LastAction = RepositoryAction.INSERT;
            Assert.AreEqual(RepositoryAction.INSERT, manager.LastAction);
            manager.BeginTransaction();
            manager.WillFlush = true;
            manager.CommitChanges();
        }

        [TestMethod]
        public void ExecuteInTransactionTest()
        {
            var manager = new MiniSessionManager();

            manager.ExecuteInTransaction(() => { });

            Assert.ThrowsException<Exception>(() =>
            {
                manager.ExecuteInTransaction(() => { throw new Exception(); });
            });
        }

        [TestMethod]
        public void FlowExceptionTest()
        {
            var manager = new MiniSessionManager
            {
                Session = null
            };

            Assert.ThrowsException<ApplicationException>(() => { manager.CommitChanges(); });
            manager.CommitChanges(new Exception());

            var session = manager.OpenSession();

            var sessionNew = manager.OpenSession();

            Assert.AreSame(session, sessionNew);

            manager.BeginTransaction();

            manager.CommitChanges(new Exception());
        }


        [TestMethod]
        public void SessionFactoryTest()
        {
            Assert.IsNotNull(MiniSessionManager.SessionFactory);
        }

        [TestMethod]
        public void ExecuteInUoWTest()
        {
            var manager = new MiniSessionManager();

            MiniSessionManager.ExecuteInUoW((mgr) => { });

            Assert.ThrowsException<Exception>(() =>
            {
                MiniSessionManager.ExecuteInUoW((mgr) => { throw new Exception(); });
            });
        }
    }
}
