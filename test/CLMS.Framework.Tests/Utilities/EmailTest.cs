using System;
using System.Collections.Generic;
using System.IO;
using CLMS.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if NETFRAMEWORK
using Http.TestLibrary;
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

            Assert.AreEqual("unit@clms.framework.com", settings.Smtp.From);

#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory()).SimulateRequest())
            {               
                settings = Email.FetchSmtpSettings();

                Assert.AreEqual("unit@clms.framework.com", settings.Smtp.From);
            }
#endif
        }

        [TestMethod]
        public void SendMailTest()
        {
            var obj = new EMailMessage
            {
                From = "info@clmsuk.com",
                To = new List<string>
                {
                    "to@example.com"
                },
                Subject = "Test",
                Body = "<h1>Hi</h1>",
                IsBodyHtml = true
            };
            Email.SendMail(obj, true);

            obj = new EMailMessage
            {
                To = new List<string>
                {
                    "to@example.com"
                },
                Subject = "Test",
                Body = "<h1>Hi</h1>",
                IsBodyHtml = true
            };
            Email.SendMail(obj, true);

            Email.SendMail("Test", "Test", "to@example.com");

            Assert.ThrowsException<ArgumentNullException>(() => Email.SendMail("Test", "Test", ""));
        }
    }
}
