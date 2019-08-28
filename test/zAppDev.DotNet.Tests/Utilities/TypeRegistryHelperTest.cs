using zAppDev.DotNet.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Utilities
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
