namespace zAppDev.DotNet.Framework.Configuration
{
    public class MailSettings
    {
        public SmtpSettings Smtp { get; set; }
    }

    public class SmtpSettings
    {
        public string From { get; set; }
        public SmtpNetworkSettings Network { get; set; }
    }

    public class SmtpNetworkSettings
    {
        public string Password { get; set; }
        public string UserName { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }

    public class ImapConfiguration
    {
        public string Host { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public int? Port { get; set; }

        public bool? EnableSsl { get; set; }
    }
}
