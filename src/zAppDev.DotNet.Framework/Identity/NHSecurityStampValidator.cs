#if NETFRAMEWORK

using System;
using log4net;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.Cookies;
using zAppDev.DotNet.Framework.Identity.Model;

namespace zAppDev.DotNet.Framework.Identity
{
    /// <summary>
    ///     Static helper class used to configure a CookieAuthenticationProvider to validate a cookie against a user's security
    ///     stamp
    /// </summary>
    public static class ApplicationCookieIdentityValidator
    {
        private static async Task<bool> VerifySecurityStampAsync<TManager, TUser>(TManager manager, TUser user, CookieValidateIdentityContext context)
        where TManager : UserManager
        where TUser : IdentityUser
        {
            var stamp = context.Identity.FindFirstValue(Constants.DefaultSecurityStampClaimType);
            return (stamp == await manager.GetSecurityStampAsync(user.User.UserName));
        }

        private static Task<bool> VerifyClientIdAsync<TUser>(TUser user, CookieValidateIdentityContext context)
        where TUser : IdentityUser
        {
            string clientId = context.Identity.FindFirstValue("AspNet.Identity.ClientId");
            if (!string.IsNullOrEmpty(clientId) && user.User.Clients.Any(c => c.Id.GetValueOrDefault().ToString() == clientId))
            {
                user.CurrentClientId = clientId;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        /// <summary>
        ///     Can be used as the ValidateIdentity method for a CookieAuthenticationProvider which will check a user's security
        ///     stamp after validateInterval
        ///     Rejects the identity if the stamp changes, and otherwise will call regenerateIdentity to sign in a new
        ///     ClaimsIdentity
        /// </summary>
        /// <param name="validateInterval"></param>
        /// <returns></returns>
        public static Func<CookieValidateIdentityContext, Task> OnValidateIdentity(TimeSpan validateInterval)
        {
            return async context =>
            {
                await ProcessIdentity(validateInterval, context);
            };
        }

        private static async Task ProcessIdentity(TimeSpan validateInterval, CookieValidateIdentityContext context)
        {
            if (context.Request.Path.HasValue
                    && (context.Request.Path.Value.EndsWith(".css")
                        || context.Request.Path.Value.EndsWith(".js")
                        || context.Request.Path.Value.EndsWith(".map")
                        || context.Request.Path.Value.EndsWith(".woff")
                        || context.Request.Path.Value.EndsWith(".woff2")
                        || context.Request.Path.Value.EndsWith(".png")
                        || context.Request.Path.Value.EndsWith(".gif")
                        || context.Request.Path.Value.EndsWith(".jpg")
                        || context.Request.Path.Value.EndsWith(".jpeg")
                       )
               )
            {
                return;
            }
            try
            {
                var currentUtc = DateTimeOffset.UtcNow;
                if (context.Options?.SystemClock != null)
                {
                    currentUtc = context.Options.SystemClock.UtcNow;
                }
                var issuedUtc = context.Properties.IssuedUtc;
                // Only validate if enough time has elapsed
                var validate = (issuedUtc == null);
                if (issuedUtc != null)
                {
                    var timeElapsed = currentUtc.Subtract(issuedUtc.Value);
                    validate = timeElapsed > validateInterval;
                }
                if (validate)
                {
                    var manager = context.OwinContext.GetUserManager<UserManager>();
                    var userId = context.Identity.GetUserId();
                    if (manager != null && !string.IsNullOrWhiteSpace(userId))
                    {
                        var user = await manager.FindByIdAsync(userId);
                        var reject = true;
                        // Refresh the identity if the stamp matches, otherwise reject
                        if (user != null
                                && await VerifySecurityStampAsync(manager, user, context)
                                //&& await VerifyClientIdAsync(user, context)
                           )
                        {
                            reject = false;
                            // Regenerate fresh claims if possible and resign in
                            var identity = await user.GenerateUserIdentityAsync(manager, context.Identity.GetIsPersistent());
                            if (identity != null)
                            {
                                context.OwinContext.Authentication.SignIn(identity);
                                var newResponseGrant = context.OwinContext.Authentication.AuthenticationResponseGrant;
                                if (newResponseGrant != null)
                                {
                                    newResponseGrant.Properties.IsPersistent = context.Identity.GetIsPersistent();
                                }
                            }
                        }
                        if (reject)
                        {
                            if (user != null)
                            {
                                int clientId;
                                if (int.TryParse(context.Identity.FindFirstValue("AspNet.Identity.ClientId"), out clientId))
                                    manager.SignOutClientById(user, clientId);
                            }
                            context.RejectIdentity();
                            context.OwinContext.Authentication.SignOut(context.Options.AuthenticationType);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ApplicationCookieIdentityValidator)).Error("Error in validate cookie.", e);
                throw;
            }
        }
    }
}
#endif