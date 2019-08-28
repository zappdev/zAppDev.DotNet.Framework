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