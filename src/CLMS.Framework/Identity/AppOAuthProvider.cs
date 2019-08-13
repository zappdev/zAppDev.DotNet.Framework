#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace CLMS.Framework.Identity
{
    public class AppOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;

        public AppOAuthProvider(string publicClientId)
        {
            _publicClientId = publicClientId ?? throw new ArgumentNullException(nameof(publicClientId));
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            string usernameVal = context.UserName;
            string passwordVal = context.Password;

            if (!IdentityHelper.ValidateUser(usernameVal, passwordVal))
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }

            // Fill claims
            var userInfo = IdentityHelper.GetUserManager().FindByName(usernameVal);
            var claims = userInfo.User.Permissions.Select(p => new Claim(Model.ClaimTypes.Permission, p.Name)).ToList();
            claims.Add(new Claim(ClaimTypes.Name, userInfo.UserName));
            if (!string.IsNullOrWhiteSpace(userInfo.User.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, userInfo.User.Email));
            }
            var userRoles = userInfo.User.Roles.Select(r => new Claim(ClaimTypes.Role, r.Name));
            claims.AddRange(userRoles);

            // Setting Claim Identities for OAUTH 2 protocol.  
            ClaimsIdentity oAuthClaimIdentity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);

            // Setting user authentication.  
            AuthenticationProperties properties = CreateProperties(userInfo.UserName);
            AuthenticationTicket ticket = new AuthenticationTicket(oAuthClaimIdentity, properties);

            context.Validated(ticket);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID.  
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(string userName)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
                                               {
                                                   { "userName", userName }
                                               };

            return new AuthenticationProperties(data);
        }
    }
}
#endif
