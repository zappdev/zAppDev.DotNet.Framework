using CLMS.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Utilities
{
    [TestClass]
    public class MailMessageTest
    {
        [TestMethod]
        public void EMailMessageTest()
        {
            var obj = new EMailMessage
            {
                From = "info@clmsuk.com",
                Subject = "Test",
                Body = "Hi",
                IsBodyHtml = true
            };
            Assert.AreEqual("info@clmsuk.com", obj.From);
            Assert.AreEqual("Test", obj.Subject);
            Assert.AreEqual("Hi", obj.Body);
            Assert.AreEqual(true, obj.IsBodyHtml);

            Assert.IsNotNull(obj.To);
            Assert.IsNotNull(obj.Attachments);
            Assert.IsNotNull(obj.Bcc);
            Assert.IsNotNull(obj.CC);

            var attachment = new EmailAttachment
            {
                Name = "./Assets/EmptyScript.ps1"
            };
            Assert.AreEqual("EmptyScript.ps1", attachment.Name);

            attachment = EmailAttachment.CreateFromBytes("test", new byte[]{}, "text/plain");

            Assert.AreEqual("test", attachment.Name);

            attachment = EmailAttachment.CreateFromString("test", "test", "text/plain");
            Assert.AreEqual("test", attachment.Name);

            attachment = EmailAttachment.CreateFromFile("test", "./Assets/TestScript.ps1", "text/plain");
            Assert.AreEqual("test", attachment.Name);

            attachment.Name = "newName";
            Assert.AreEqual("newName", attachment.Name);
        }
    }
}
