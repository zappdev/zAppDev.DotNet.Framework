namespace CLMS.Framework.Configuration
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
    }
}
