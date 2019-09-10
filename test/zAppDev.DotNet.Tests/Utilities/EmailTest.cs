// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using zAppDev.DotNet.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if NETFRAMEWORK
using Http.TestLibrary;

#endif

namespace zAppDev.DotNet.Framework.Tests.Utilities
{
    [TestClass]
    public class EmailTest
    {
        [TestMethod]
        public void FetchSmtpSettingsTest()
        {
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory()).SimulateRequest())
            {
                var settings = Email.FetchSmtpSettings();

                Assert.AreEqual("unit@clms.framework.com", settings.Smtp.From);
            }
#else
            var settings = Email.FetchSmtpSettings();

            Assert.AreEqual("unit@clms.framework.com", settings.Smtp.From);
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

        [TestMethod]
        public void ImapTest()
        {
            var emailHandler = new Email();

            Assert.AreEqual(true, emailHandler.TestConnection());

            var mailCount = emailHandler.GetMailCount();
            var status = emailHandler.HasUnreadMessages();

            if (status)
            {
                Debug.WriteLine($"Unread: {mailCount}");
            }

            Debug.WriteLine($"IDs: {string.Join(", ", emailHandler.GetIDs())}");

            emailHandler.GetMails(true, false).ForEach(message =>
                Debug.WriteLine($"From: {message.From} {Environment.NewLine} Subject: {message.Subject}"));

            var allEmails = emailHandler.GetAllIDs();
            
            var email = emailHandler.GetMail(allEmails.First(), true, true);
            Debug.WriteLine($"From: {email.From} {Environment.NewLine} Subject: {email.Subject}");
            
            emailHandler.MarkAsRead(allEmails.First(), true);
        }
    }
}