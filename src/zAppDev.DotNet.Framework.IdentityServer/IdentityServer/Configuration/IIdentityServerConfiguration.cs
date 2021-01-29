namespace zAppDev.DotNet.Framework.IdentityServer.Configuration
{
    public interface IIdentityServerConfiguration
    {
        string Authority { get; }
        string ClientId { get; }
        string ClientSecret { get; }
        string AuthenticationCookieName { get; }
        byte[] JWTKey { get; }
    }
}