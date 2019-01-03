using CLMS.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Utilities
{
    [TestClass]
    public class EmailTest
    {
        [TestMethod]
        public void FetchSmtpSettingsTest()
        {
            var settings = Email.FetchSmtpSettings();

            Assert.AreEqual("", settings.Smtp.From);
        }
    }
}
