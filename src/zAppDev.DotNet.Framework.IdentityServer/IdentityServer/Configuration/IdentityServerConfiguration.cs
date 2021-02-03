using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Identity.Model;

namespace zAppDev.DotNet.Framework.IdentityServer.Configuration
{
    public class IdentityServerConfiguration : IIdentityServerConfiguration
    {
        public string Authority { get; set; } = "";

        public string ClientId { get; set; } = "";

        public string ClientSecret { get; set; } = "";

        public string AuthenticationCookieName { get; set; } = "";

        public IList<string> Scopes { get; set; } = new List<string>();

        public byte[] JWTKey { get; set; } = System.Array.Empty<byte>();

        public string NameClaim { get; set; } = "";

        public string EmailClaim { get; set; } = "";

        public string UsernameClaim { get; set; } = "";

        public string UserClass { get; set; } = "ApplicationUser";

        public Func<ApplicationUser, Task> ExternalUserCreating { get; set; } = (user) => Task.CompletedTask;
    }
}
