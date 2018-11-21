namespace System.Net.Configuration
{
    public class SmtpNetworkElement
    {
        //
        // Summary:
        //     Determines whether or not default user credentials are used to access an SMTP
        //     server. The default value is false.
        //
        // Returns:
        //     true indicates that default user credentials will be used to access the SMTP
        //     server; otherwise, false.
        public bool DefaultCredentials { get; set; }
        //
        // Summary:
        //     Gets or sets the name of the SMTP server.
        //
        // Returns:
        //     A string that represents the name of the SMTP server to connect to.
        public string Host { get; set; }
        //
        // Summary:
        //     Gets or sets the Service Provider Name (SPN) to use for authentication when using
        //     extended protection to connect to an SMTP mail server.
        //
        // Returns:
        //     A string that represents the SPN to use for authentication when using extended
        //     protection to connect to an SMTP mail server.
        public string TargetName { get; set; }
        //
        // Summary:
        //     Gets or sets the client domain name used in the initial SMTP protocol request
        //     to connect to an SMTP mail server.
        //
        // Returns:
        //     A string that represents the client domain name used in the initial SMTP protocol
        //     request to connect to an SMTP mail server.
        public string ClientDomain { get; set; }
        //
        // Summary:
        //     Gets or sets the user password to use to connect to an SMTP mail server.
        //
        // Returns:
        //     A string that represents the password to use to connect to an SMTP mail server.
        public string Password { get; set; }
        //
        // Summary:
        //     Gets or sets the port that SMTP clients use to connect to an SMTP mail server.
        //     The default value is 25.
        //
        // Returns:
        //     A string that represents the port to connect to an SMTP mail server.
        public int Port { get; set; }
        //
        // Summary:
        //     Gets or sets the user name to connect to an SMTP mail server.
        //
        // Returns:
        //     A string that represents the user name to connect to an SMTP mail server.
        public string UserName { get; set; }
        //
        // Summary:
        //     Gets or sets whether SSL is used to access an SMTP mail server. The default value
        //     is false.
        //
        // Returns:
        //     true indicates that SSL will be used to access the SMTP mail server; otherwise,
        //     false.
        public bool EnableSsl { get; set; }
    }
}