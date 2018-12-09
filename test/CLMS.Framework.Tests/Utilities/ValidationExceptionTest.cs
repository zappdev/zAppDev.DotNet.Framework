using CLMS.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Utilities
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