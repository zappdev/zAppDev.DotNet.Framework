// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK

using System;
using System.Collections.Concurrent;
using System.Linq;

using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using NHibernate;
using zAppDev.DotNet.Framework.Data;
using zAppDev.DotNet.Framework.Identity.Model;

namespace zAppDev.DotNet.Framework.Identity
{
    public class UserManager : UserManager<IdentityUser>
    {
        private static ConcurrentDictionary<Guid, Model.ApplicationClient> _clients = new ConcurrentDictionary<Guid, Model.ApplicationClient>();

        public static PasswordPolicyConfig PasswordPolicyConfig = new PasswordPolicyConfig();

        public UserManager(MiniSessionManager sessionManager) : base(new UserStore(sessionManager))
        {
        }

        public IdentityUser FindById(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            return base.FindByIdAsync(userId).Result;
        }


        public IdentityUser FindByName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return null;
            }
            return base.FindByNameAsync(userName).Result;
        }

        public static UserManager Create(IdentityFactoryOptions<UserManager> options, IOwinContext context)
        {
            var sessionManager = context.Get<MiniSessionManager>();
            var manager = new UserManager(sessionManager);
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<IdentityUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = false
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = PasswordPolicyConfig.RequiredLength,
                RequireNonLetterOrDigit = PasswordPolicyConfig.RequireNonLetterOrDigit,
                RequireDigit = PasswordPolicyConfig.RequireDigit,
                RequireLowercase = PasswordPolicyConfig.RequireLowercase,
                RequireUppercase = PasswordPolicyConfig.RequireUppercase
            };
            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = false;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 10;
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<IdentityUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }

        public IdentityResult SignOutClient(IdentityUser user, string clientKey)
        {
            try
            {
                if (string.IsNullOrEmpty(clientKey))
                {
                    throw new ArgumentNullException("clientKey");
                }
                var clients = user.User.Clients.Where(c => c.ClientKey == clientKey).ToList();
                foreach(var client in clients)
                {
                    user.User.RemoveClients(client);
                }
                user.CurrentClientId = null;
                return this.Update(user);
            }
            catch (Exception x)
            {
                LogManager.GetLogger(this.GetType()).Error("Could not sign out user client.", x);
                return new IdentityResult(new [] { x.Message });
            }
        }


        public IdentityResult SignOutClientById(IdentityUser user, int clientId)
        {
            var client = user.User.Clients.FirstOrDefault(c => c.Id == clientId);
            if (client != null)
            {
                user.User.RemoveClients(client);
            }
            user.CurrentClientId = null;
            return this.Update(user);
        }

        public IdentityResult SignInClient(string username, string clientKey, string userHostAddress, string sessionID)
        {
            return SignInClient(this.FindByName(username), clientKey, userHostAddress, sessionID);
        }

        public IdentityResult SignInClient(IdentityUser user, string clientKey, string userHostAddress, string sessionID)
        {
            if (string.IsNullOrEmpty(clientKey))
            {
                throw new ArgumentNullException(nameof(clientKey));
            }
            try
            {
                var client = user.User.Clients.SingleOrDefault(c => c.ClientKey == clientKey && c.SessionId == sessionID);
                if (client == null)
                {
                    client = new Model.ApplicationClient
                    {
                        ClientKey = clientKey,
                        IPAddress = userHostAddress,
                        SessionId = sessionID,
                        ConnectedOn = DateTime.UtcNow
                    };
                    user.User.AddClients(client);
                }
                else
                {
                    client.ConnectedOn = DateTime.UtcNow;
                }
                var result = this.Update(user);
                //var repo = new Repository();
                //repo.SaveApplicationClient(client);
                user.CurrentClientId = client.Id.ToString();
                //var result = new IdentityResult();
                return result;
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(UserManager)).Error($"SignInClient for user: '{user?.Id}' failed", e);
                return null;
            }
        }

        #region External Account Functions
        public IdentityResult SignInClient(ExternalLoginInfo loginInfo, string clientKey, string userHostAddress, string sessionID)
        {
            if (string.IsNullOrEmpty(clientKey))
            {
                throw new ArgumentNullException(nameof(loginInfo));
            }
            if (loginInfo == null)
            {
                throw new ArgumentNullException(nameof(loginInfo));
            }
            var user = this.Find(new UserLoginInfo(loginInfo.Login.LoginProvider, loginInfo.Login.ProviderKey));
            var client = user.User.Clients.SingleOrDefault(c => c.ClientKey == clientKey && c.SessionId == sessionID);
            if (client == null)
            {
                client = new Model.ApplicationClient
                {
                    ClientKey = clientKey,
                    IPAddress = userHostAddress,
                    SessionId = sessionID,
                    ConnectedOn = DateTime.UtcNow
                };
                user.User.AddClients(client);
            }
            else
            {
                client.ConnectedOn = DateTime.UtcNow;
            }
            var result = this.Update(user);
            user.CurrentClientId = client.Id.ToString();
            return result;
        }

        public IdentityResult LinkExternalAccount(ExternalLoginInfo loginInfo)
        {
            if (loginInfo == null)
            {
                throw new ArgumentNullException(nameof(loginInfo));
            }
            var user = IdentityHelper.GetCurrentApplicationUser();
            var externalLogin = user.Logins.FirstOrDefault(x => x.ProviderKey == loginInfo.Login.ProviderKey && x.LoginProvider == loginInfo.Login.LoginProvider);
            if (externalLogin == null)
            {
                return this.AddLogin(user.UserName, new UserLoginInfo(loginInfo.Login.LoginProvider, loginInfo.Login.ProviderKey));
            }
            return null;
        }
        #endregion
    }
}

#endif