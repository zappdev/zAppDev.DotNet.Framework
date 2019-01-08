using CLMS.Framework.Data;
using CLMS.Framework.Tests.Data.POCO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Data
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
