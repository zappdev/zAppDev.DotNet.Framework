using System.Collections.Generic;
using System.IO;
using System.Net.Mail;

namespace CLMS.Framework.Utilities
{
    public class EMailMessage
    {
        public EMailMessage()
        {
            To = new List<string>();
            CC = new List<string>();
            Bcc = new List<string>();
            Attachments = new List<EmailAttachment>();
        }

        public string From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }                      
        public bool IsBodyHtml { get; set; }
        public List<string> To { get; set; }
        public List<string> CC { get; set; }
        public List<string> Bcc { get; set; }
        public List<EmailAttachment> Attachments { get; set; }
    }

    public class EmailAttachment
    {
        public string Name
        {
            get => Attachment?.Name;
            set
            {
                if (Attachment == null)
                {
                    Attachment = new Attachment(value);
                }
                else
                {
                    Attachment.Name = value;
                }
            }
        }

        public Attachment Attachment { get; set; }

        public EmailAttachment()
        {

        }

        public EmailAttachment(Attachment attachment)
        {
            Attachment = attachment;
        }

        public static EmailAttachment CreateFromFile(string name, string path, string mediaType)
        {
			//using (var fs = new FileStream(path, FileMode.Open))
            //{
            //    var attachment = new Attachment(fs, name, mediaType);
            //    return new EmailAttachment(attachment);
            //}
            return new EmailAttachment(new Attachment(new FileStream(path, FileMode.Open), name, mediaType));
        }

        public static EmailAttachment CreateFromString(string name, string contents, string mediaType)
        {
            return new EmailAttachment(Attachment.CreateAttachmentFromString(contents, name, System.Text.Encoding.Default, mediaType));
        }

        public static EmailAttachment CreateFromBytes(string name, byte[] contents, string mediaType)
        {
            //using (var ms = new MemoryStream(contents))
            //{
            //    var attachment = new Attachment(ms, name, mediaType);
            //    return new EmailAttachment(attachment);
            //}			
            return new EmailAttachment(new Attachment(new MemoryStream(contents), name, mediaType));
        }
    }
}
