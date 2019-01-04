using System.IO;
using Http.TestLibrary;
using CLMS.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if NETFRAMEWORK

#endif

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

#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory()).SimulateRequest())
            {               
                settings = Email.FetchSmtpSettings();

                Assert.AreEqual("", settings.Smtp.From);
            }
#endif

        }
    }
}
