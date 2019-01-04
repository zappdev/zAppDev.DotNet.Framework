using System.ServiceModel.Channels;

using CLMS.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

#if NETFRAMEWORK
using System.Web;
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
            var context = new Mock<HttpContext>();
            HttpContext.Current = context.Object;

            settings = Email.FetchSmtpSettings();

            Assert.AreEqual("", settings.Smtp.From);
#endif

        }
    }
}
