using CLMS.Framework.Data;
using CLMS.Framework.Tests.Data.POCO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Data
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
