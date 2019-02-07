using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using CLMS.Framework.Configuration;
using log4net;
using S22.Imap;

namespace CLMS.Framework.Utilities
{
    public class Email
	{
        #region IMAP Settings structure
        public class ImapSettings
        {
#if NETFRAMEWORK
            public string Server { get; internal set; }
            public int Port { get; internal set; }
            public string Username { get; internal set; }
            public string Password { get; internal set; }
            public bool UseSSL { get; internal set; }
#else
            public ImapConfiguration ImapConfiguration { get; internal set; }
#endif


            public ImapSettings()
            {

            }

            public ImapSettings(MailSettings smtpSettings)
            {
                var log = LogManager.GetLogger(typeof(ImapSettings));

#if NETFRAMEWORK
                var prefix = "IMAP:";

                var server = ConfigurationManager.AppSettings[$"{prefix}host"];
                var port = ConfigurationManager.AppSettings[$"{prefix}port"];
                var username = ConfigurationManager.AppSettings[$"{prefix}username"];
                var password = ConfigurationManager.AppSettings[$"{prefix}password"];
                var useSSL = ConfigurationManager.AppSettings[$"{prefix}enableSSL"];

                if (string.IsNullOrWhiteSpace(server)) throw new ArgumentNullException(nameof(smtpSettings), $"Missing setting: [{prefix}host]");
                if (string.IsNullOrWhiteSpace(port)) throw new ArgumentNullException(nameof(smtpSettings), $"Missing setting: [{prefix}port]");

                Server = server;

                if (int.TryParse(port, out var p))
                {
                    Port = p;
                }
                else
                {
                    throw new ArgumentException("MailSettings", $"Invalid setting: [{prefix}port]");
                }

                Username = username;
                if (string.IsNullOrWhiteSpace(Username))
                {
                    Username = smtpSettings?.Smtp?.Network?.UserName;
                    if (string.IsNullOrWhiteSpace(Username))
                        throw new ArgumentNullException(nameof(smtpSettings), $"Missing setting: [{prefix}username]");

                    log.Warn($"Missing or incorrect setting: [{prefix}username]. Will try to use the username you set in your SMTP Settings.");
                }

                Password = password;
                if (string.IsNullOrWhiteSpace(Password))
                {
                    Password = smtpSettings?.Smtp?.Network?.Password;
                    if (string.IsNullOrWhiteSpace(Password))
                        throw new ArgumentNullException(nameof(smtpSettings), $"Missing setting: [{prefix}password]");

                    log.Warn($"Missing or incorrect setting: [{prefix}password]. Will try to use the password you set in your SMTP Settings.");
                }

                if (bool.TryParse(useSSL, out var s))
                {
                    UseSSL = s;
                }
                else
                {
                    log.Warn($"Missing or incorrect setting: [{prefix}enableSSL]. Will go with 'false'.");
                }

#else
                const string prefix = "IMAP:";

                ImapConfiguration = ConfigurationHandler.GetAppConfiguration().ImapConfiguration;

                if (string.IsNullOrWhiteSpace(ImapConfiguration.Host)) throw new ArgumentNullException(nameof(ImapSettings), $"Missing setting: [{prefix}host]");
                
                if (ImapConfiguration.Port == null) throw new ArgumentNullException(nameof(ImapSettings), $"Missing setting: [{prefix}port]");
               
                if (string.IsNullOrWhiteSpace(ImapConfiguration.Username))
                {
                    ImapConfiguration.Username = smtpSettings?.Smtp?.Network?.UserName;
                    if(string.IsNullOrWhiteSpace(ImapConfiguration.Username))
                        throw new ArgumentNullException(nameof(ImapSettings), $"Missing setting: [{prefix}username]");

                    log.Warn($"Missing or incorrect setting: [{prefix}username]. Will try to use the username you set in your SMTP Settings.");
                }

                if (string.IsNullOrWhiteSpace(ImapConfiguration.Password))
                {
                    ImapConfiguration.Password = smtpSettings?.Smtp?.Network?.Password;
                    if(string.IsNullOrWhiteSpace(ImapConfiguration.Password))
                        throw new ArgumentNullException(nameof(ImapSettings), $"Missing setting: [{prefix}password]");

                    log.Warn($"Missing or incorrect setting: [{prefix}password]. Will try to use the password you set in your SMTP Settings.");
                }

                if(ImapConfiguration.EnableSsl == null)
                {
                    log.Warn($"Missing or incorrect setting: [{prefix}enableSSL]. Will go with 'false'.");
                }
#endif
            }
        }
#endregion

        public static MailSettings FetchSmtpSettings()
        {
            return ConfigurationHandler.GetSmtpSettings();
        }

#region SMTP: Sending E-Mails
        public static void SendMail(EMailMessage message, bool sendAsync = false)
        {
            var appConfiguration = ConfigurationHandler.GetAppConfiguration();

            if (string.IsNullOrEmpty(message.From))
            {
                var settings = FetchSmtpSettings();

                if (settings == null)
                    throw new ArgumentNullException(nameof(message), "No valid mail settings were found in the configuration.");

                message.From = settings.Smtp.From;
            }

            bool.TryParse(appConfiguration.AppSettings["TestMode"], out var testMode);

            if (testMode) {
                var originalRecipients = "";
                foreach (var i in message.To) {
                    originalRecipients += $"<br/>{i}";
                }

                message.To.Clear();
                message.To.Add(appConfiguration.AppSettings["TestModeEmail"]);
                message.Body = "<b>TEST MODE</b> Original Recipients:" + originalRecipients + "<br/><br/>" + message.Body;
            }

            var mailSeparatorCharacter = appConfiguration.AppSettings["EmailSeparatorCharacter"];
            mailSeparatorCharacter = !string.IsNullOrWhiteSpace(mailSeparatorCharacter) ? mailSeparatorCharacter : ",";

            SendMail(message.Subject,
                message.Body,
                message.To == null ? "" : string.Join(mailSeparatorCharacter, message.To.Where(a=> !string.IsNullOrWhiteSpace(a))),
                message.CC == null ? "" : string.Join(mailSeparatorCharacter, message.CC.Where(a => !string.IsNullOrWhiteSpace(a))),
                message.Bcc == null ? "" : string.Join(mailSeparatorCharacter, message.Bcc.Where(a => !string.IsNullOrWhiteSpace(a))),
                message.From,
                message.Attachments?.Select(a => a.Attachment).ToList(),
                sendAsync);
        }

	    /// <summary>
	    /// Sends an email
	    /// </summary>
	    /// <param name="subject">The Subject of the email</param>
	    /// <param name="body">The Body of the email (html)</param>
	    /// <param name="to">The addresses to send the email to (',' separated).</param>
	    /// <param name="cc">The addresses to CC the email to (',' separated)</param>
	    /// <param name="bcc">The addresses to BCC the email to (',' separated)</param>
	    /// <param name="fromAddress">The address to send the email from (Optional. Defaults to the From address in the configuration).</param>
	    /// <param name="attachments">Any attachments for the email</param>
	    /// <param name="sendAsync">Send the mail asynchronously</param>
	    public static void SendMail(string subject, string body, string to, string cc = "", string bcc = "", string fromAddress = "", List<Attachment> attachments = null, bool sendAsync = false)
        {
            var log = LogManager.GetLogger(typeof(Email));

            log.Debug("Start");

            if (string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentNullException(nameof(to), "No valid recipient address.");
            }

            log.Debug("To address Ok");


            var settings = FetchSmtpSettings();

            log.Debug("settings Ok");

            if (settings == null)
                throw new ArgumentNullException(nameof(settings), "No valid mail settings were found in the configuration.");

            var fromMailAddress = string.IsNullOrWhiteSpace(fromAddress) ? settings.Smtp.From : fromAddress;

            log.Debug("from address Ok");

            if (string.IsNullOrWhiteSpace(fromMailAddress))
                throw new ArgumentNullException(nameof(fromAddress), "No valid sender mail address was found in the configuration or in the arguments.");
            
            log.Debug("Creating Mail Message");

            var message = new MailMessage
            {
                From = new MailAddress(fromMailAddress),
                IsBodyHtml = true,
                Subject = subject,
                Body = body,
            };

            log.Debug("Created Class Instance OK");

            if (attachments == null) attachments = new List<Attachment>();            

            foreach(var attachment in attachments) 
            {
                message.Attachments.Add(attachment);
            }

            log.Debug("Attachments OK");

            log.Debug($"Sending email TO: {to}");
            log.Debug($"Sending email CC: {cc}");
            log.Debug($"Sending email BCC: {bcc}");

	        try
	        {
                message.To.Add(to);
            }
            catch (Exception x)
	        {
	            log.Error($"Could not add TO address: {to}", x);
	            throw;
	        }

	        if (!string.IsNullOrWhiteSpace(cc))
	        {
                try
                {
                    message.CC.Add(cc);
                }
                catch (Exception x)
                {
                    log.Error($"Could not add CC address: {cc}", x);
                    throw;
                }
	        }

            if (!string.IsNullOrWhiteSpace(bcc))
            {
                try
                {
                    message.Bcc.Add(bcc);
                }
                catch (Exception x)
                {
                    log.Error($"Could not add BCC address: {bcc}", x);
                    throw;
                }
            }

            SendMail(message, sendAsync);

            log.Debug("ALL OK");
        }

		private static void SendMail(MailMessage message, bool sendAsync = false)
		{
			var log = LogManager.GetLogger(typeof(Email));
		    var settings = FetchSmtpSettings();
            SmtpClient client = null;
			try
			{
				if (message.To.Count == 0)
				{
					log.Warn("No message recipients!");
					return;
				}

			    client = new SmtpClient(settings.Smtp.Network.Host, settings.Smtp.Network.Port)
			    {
			        Credentials = new NetworkCredential(settings.Smtp.Network.UserName,
			            settings.Smtp.Network.Password)
			    };

			    log.Debug("host: " + client.Host);
                log.Debug("port: " + client.Port);

				if (bool.TryParse(ConfigurationManager.AppSettings["EnableSSL"], out var ssl))
				{
					client.EnableSsl = ssl;
				}

                log.Debug("SSL: " + client.EnableSsl);

                var emailSecureConnectionType = ConfigurationManager.AppSettings["EmailSecureConnectionType"];
			    if (string.IsNullOrWhiteSpace(emailSecureConnectionType)) {
			        emailSecureConnectionType = "TLS";
			    }

			    if (!string.IsNullOrWhiteSpace(message.Body)) {
                    // this is to avoid BARE LFs that some email servers reject
			        message.Body = message.Body.Replace("\n", "\r\n").Replace("\r\r", "\r");
			    }

			    if (emailSecureConnectionType != "TLS") {
                    log.Debug("Sending email using SSL");
                    throw new NotImplementedException();
                    //SendMailOverSSL(message);
                    //return;
			    }

			    if (sendAsync)
			    {
                    log.Debug("Sending email using TLS asynchronously");

                    new Thread(() => {

                        try
                        {
                            client.Send(message);
                        }
                        catch (Exception e)
                        {
                            LogManager.GetLogger(typeof(Email)).Error("Error sending Email", e);
                            return;
                        }
		                finally
		                {
                            client?.Dispose();
                        }						

                        foreach (var att in message.Attachments)
                        {
                            att.ContentStream?.Dispose();
                        }

                        LogManager.GetLogger(typeof(Email)).Debug("Sent Email successfully");

                    }).Start();

                    //client.SendCompleted += (sender, args) =>
			                 //               {
			                 //                   if (args.Error != null)
			                 //                   {
			                 //                       log.Error(args.Error);
			                 //                   }
			                 //               };
                    //client.SendAsync(message, CancellationToken.None);

			    }
			    else
			    {
                    log.Debug("Sending email using TLS synchronously");
                    client.Send(message);

                    foreach (var att in message.Attachments)
                    {
                        att.ContentStream?.Dispose();
                    }
			    }
			}
			catch (Exception e)
			{
				var exc = e;
				
				while (exc != null)
				{
					log.Error("Problem with sending email", exc);
					exc = exc.InnerException;
				}
			}
		    finally
		    {
		        if (!sendAsync)
		        {
		            client?.Dispose();
		        }
		    }			
		}

        /*private void SendMailOverSSL(MailMessage message)
	    {
            if (HttpContext.Current == null) {
                throw new ApplicationException("Cannot send email over SSL. No Http context exists!");
            }

            var config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
            var settings = (System.Net.Configuration.MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");

            var cdoMessage = new CDO.Message
            {
                From = message.From.Address,
                To = string.Join(";", message.To.ToList().Select(a=>a.Address)),
                CC = string.Join(";", message.CC.ToList().Select(a=>a.Address)),
                BCC = string.Join(";", message.Bcc.ToList().Select(a=>a.Address)),
                Subject = message.Subject,
                HTMLBody = message.Body
            };

            var configuration = cdoMessage.Configuration;
            var fields = configuration.Fields;
            var field = fields["http://schemas.microsoft.com/cdo/configuration/smtpserver"];
            if (settings != null) {
                field.Value = settings.Smtp.Network.Host;
                field = fields["http://schemas.microsoft.com/cdo/configuration/smtpserverport"];
                field.Value = settings.Smtp.Network.Port;
                field = fields["http://schemas.microsoft.com/cdo/configuration/sendusing"];
                field.Value = CDO.CdoSendUsing.cdoSendUsingPort;
                field = fields["http://schemas.microsoft.com/cdo/configuration/smtpauthenticate"];
                field.Value = CDO.CdoProtocolsAuthentication.cdoBasic;
                field = fields["http://schemas.microsoft.com/cdo/configuration/sendusername"];
                field.Value = settings.Smtp.Network.UserName;
                field = fields["http://schemas.microsoft.com/cdo/configuration/sendpassword"];
                field.Value = settings.Smtp.Network.Password;
            }
            field = fields["http://schemas.microsoft.com/cdo/configuration/smtpusessl"];
            field.Value = "true";
            fields.Update();

            cdoMessage.Send();
	    }*/

#endregion


#region IMAP: Reading E-Mails

        private readonly bool _suppressExceptions;

        private ImapSettings _imapSettings { get; set; }

        public Email(bool suppressExceptions = false)
        {
            try
            {
                _suppressExceptions = suppressExceptions;
                _imapSettings = new ImapSettings(FetchSmtpSettings());
            }
            catch (Exception)
            {
                if (!_suppressExceptions) throw;
            }
        }

        public Email(string server, int port, string username, string password, bool useSSL, bool suppressExceptions = false)
        {
            try
            {
                _suppressExceptions = suppressExceptions;
#if NETFRAMEWORK
                _imapSettings = new ImapSettings
                {
                    Server = server,
                    Port = port,
                    Username = username,
                    Password = password,
                    UseSSL = useSSL
                };

#else
                _imapSettings = new ImapSettings
                {
                    ImapConfiguration = new ImapConfiguration
                    {
                        Host = server,
                        Port = port,
                        Username = username,
                        Password = password,
                        EnableSsl = useSSL
                    }
                };
#endif
            }
            catch (Exception)
            {
                if (!_suppressExceptions) throw;
            }
        }

        public bool TestConnection()
        {
            try
            {
                using (var client = GetClient())
                {
                    return client.Authed;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int GetMailCount()
        {
            try
            {
                using (var client = GetClient())
                {
                    return client.Search(SearchCondition.Unseen()).Count();
                }
            }
            catch (Exception)
            {
                if (!_suppressExceptions) throw;
                return -1;
            }
        }
        
        public bool HasUnreadMessages()
        {
            return GetMailCount() > 0;
        }

        private EMailMessage Convert(MailMessage message, uint id, bool readAll)
        {
            var emailMessage = new EMailMessage
            {
                From = message.From.Address,
                Subject = message.Subject
            };


            if (!readAll) return emailMessage;

            emailMessage.Body = message.Body;
            emailMessage.IsBodyHtml = message.IsBodyHtml;
            emailMessage.To = message.To.Select(x => x.Address).ToList();
            emailMessage.CC = message.CC.Select(x => x.Address).ToList();
            emailMessage.Bcc = message.Bcc.Select(x => x.Address).ToList();

            foreach (var attachment in message.Attachments)
            {
                if (attachment.ContentStream?.CanRead == true)
                {

                    using (var stream = new MemoryStream())
                    {
                        var attachmentBytes = new List<byte>();
                        while (true)
                        {
                            var nextByte = stream.ReadByte();
                            if (nextByte == -1) break;
                            attachmentBytes.Add((byte)nextByte);
                        }
                        emailMessage.Attachments.Add(EmailAttachment.CreateFromBytes(attachment.Name, attachmentBytes.ToArray(), attachment.ContentType.MediaType));
                    }
                }
            }
            return emailMessage;
        }

        public EMailMessage GetMail(int? id, bool readAll, bool markAsRead = false)
        {
            try
            {
                using (var client = GetClient())
                {
                    var message = client.GetMessage((uint)id, readAll ? FetchOptions.Normal : FetchOptions.NoAttachments);
                    if (message != null)
                    {
                        MarkAsRead(client, (uint)id, markAsRead);
                        return Convert(message, (uint)id, readAll);
                    }
                }
            }
            catch (Exception)
            {
                if (!_suppressExceptions) throw;
            }
            return new EMailMessage();
        }

        public List<int?> GetIDs()
        {
            var ids = new List<int>();

            try
            {
                using (var client = GetClient())
                {
                    ids = client.Search(SearchCondition.Unseen()).Select(x => (int)x).ToList();
                }
            }
            catch (Exception)
            {
                if (!_suppressExceptions) throw;
            }

            return ids.Select(x => (int?)x).ToList();
        }

        public List<int?> GetAllIDs()
        {
            var ids = new List<int>();

            try
            {
                using (var client = GetClient())
                {
                    ids = client.Search(SearchCondition.All()).Select(x => (int)x).ToList();
                }
            }
            catch (Exception)
            {
                if (!_suppressExceptions) throw;
            }

            return ids.Select(x => (int?)x).ToList();
        }
        
        public List<EMailMessage> GetMails(bool readAll, bool markAsRead = false, List<int?> ids = null)
        {
            var emailMessages = new List<EMailMessage>();

            try
            {
                using (var client = GetClient())
                {
                    var uids = ids == null ? client.Search(SearchCondition.Unseen()) : ids.Select(x => (uint)x);
                    var messages = client.GetMessages(uids, readAll ? FetchOptions.Normal : FetchOptions.NoAttachments);

                    var index = 0;
                    foreach (var message in messages)
                    {
                        var uid = uids.ElementAt(index);
                        emailMessages.Add(Convert(message, uid, readAll));
                        MarkAsRead(client, uid, markAsRead);

                        index++;
                    }
                }
            }
            catch (Exception)
            {
                if (!_suppressExceptions) throw;
            }

            return emailMessages;
        }

        private void MarkAsRead(ImapClient Client, uint id, bool isRead = true)
        {
            if (isRead)
                Client.AddMessageFlags(id, null, MessageFlag.Seen);
            else
                Client.RemoveMessageFlags(id, null, MessageFlag.Seen);
        }

        public void MarkAsRead(int? id, bool isRead = true)
        {
            try
            {
                using (var client = GetClient())
                {
                    var message = client.GetMessage((uint)id);
                    if (message != null)
                    {
                        MarkAsRead(client, (uint)id, isRead);
                    }
                }
            }
            catch (Exception)
            {
                if (!_suppressExceptions) throw;
            }
        }

	    private ImapClient GetClient()
        {
#if NETFRAMEWORK
            return new ImapClient(
                _imapSettings.Server,
                _imapSettings.Port,
                _imapSettings.Username,
                _imapSettings.Password,
                AuthMethod.Login,
                _imapSettings.UseSSL);
#else

            Debug.Assert(_imapSettings.ImapConfiguration.Port != null, 
                "_imapSettings.ImapConfiguration.Port != null");
            Debug.Assert(_imapSettings.ImapConfiguration.EnableSsl != null, 
                "_imapSettings.ImapConfiguration.EnableSsl != null");

            return new ImapClient(
	            _imapSettings.ImapConfiguration.Host,
	            _imapSettings.ImapConfiguration.Port.Value,
	            _imapSettings.ImapConfiguration.Username,
	            _imapSettings.ImapConfiguration.Password,
	            AuthMethod.Login,
	            _imapSettings.ImapConfiguration.EnableSsl.Value);
#endif
        }

#endregion
    }    
}
