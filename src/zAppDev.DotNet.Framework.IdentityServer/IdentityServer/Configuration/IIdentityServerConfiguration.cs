using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Identity.Model;

namespace zAppDev.DotNet.Framework.IdentityServer.Configuration
{
    public interface IIdentityServerConfiguration
    {
        string Authority { get; set; }
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string AuthenticationCookieName { get; set; }
        byte[] JWTKey { get; set; }
        IList<string> Scopes { get; set; }
        string EmailClaim { get; set; }
        string NameClaim { get; set; }
        string UsernameClaim { get; set; }
        string UserClass { get; set; }
        Func<ApplicationUser, Task> ExternalUserCreating { get; set; }
    }
}