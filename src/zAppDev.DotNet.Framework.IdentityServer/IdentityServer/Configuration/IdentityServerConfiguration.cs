namespace zAppDev.DotNet.Framework.IdentityServer.Configuration
{
    public class IdentityServerConfiguration : IIdentityServerConfiguration
    {
        public string Authority { get; internal set; } = "";

        public string ClientId { get; internal set; } = "";

        public string ClientSecret { get; internal set; } = "";

        public string AuthenticationCookieName { get; internal set; } = "";

        public byte[] JWTKey { get; internal set; }
    }
}
