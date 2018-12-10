using CLMS.Framework.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Data
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

            manager.WillFlush = false;
            Assert.IsFalse(manager.WillFlush);

            Assert.IsNull(manager.SingleUseSession);
            manager.SingleUseSession = false;
            Assert.IsFalse(manager.SingleUseSession.Value);
#endif
        }

        [TestMethod]
        public void SessionFactoryTest()
        {
            Assert.IsNotNull(MiniSessionManager.SessionFactory);
        }

    }
}
