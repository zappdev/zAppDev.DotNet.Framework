using CLMS.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Utilities
{
    [TestClass]
    public class TypeRegistryHelperTest
    {
        [TestMethod]
        public void GetSystemDomainModelTypesTest()
        {
            Assert.AreEqual(22, TypeRegistryHelper.GetSystemDomainModelTypes().Count);
        }

        [TestMethod]
        public void GetAuditableSystemDomainModelTypesTest()
        {
            Assert.AreEqual(17, TypeRegistryHelper.GetAuditableSystemDomainModelTypes().Count);
        }
    }
}
