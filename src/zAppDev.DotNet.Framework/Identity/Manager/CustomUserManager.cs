// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;

using log4net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using zAppDev.DotNet.Framework.Identity.Model;

namespace zAppDev.DotNet.Framework.Identity
{

    public class CustomUserManager : ZappDevUserManager
    {
        private static ConcurrentDictionary<Guid, ApplicationClient> _clients = new ConcurrentDictionary<Guid, ApplicationClient>();

        public CustomUserManager(
            IUserStore<Model.IdentityUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<Model.IdentityUser> passwordHasher,
            IEnumerable<IUserValidator<Model.IdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<Model.IdentityUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<CustomUserManager> logger) :
            base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {

        }

        public override Model.IdentityUser Find(ExternalLoginInfo loginInfo)
        {
            if (!SupportsUserLogin)
                throw new ApplicationException("UserManager don't support user login");

            if (Store is UserStore storeImp)
            {
                return storeImp.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey).GetAwaiter().GetResult();
            }
            return null;
        }

        public override Model.IdentityUser Find(string username, string password)
        {
            var user = FindById(username);
            if (user == null) return null;

            var pass = CheckPasswordAsync(user, password).GetAwaiter().GetResult();

            return (pass) ? user : null;
        }

        public override Model.IdentityUser FindById(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            return base.FindByIdAsync(userId).GetAwaiter().GetResult();
        }

        public override Model.IdentityUser FindByName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return null;
            }
            return base.FindByNameAsync(userName).GetAwaiter().GetResult();
        }

        public override async Task<string> GeneratePasswordResetTokenAsync(string userId)
        {
            var user = await FindByIdAsync(userId);
            return await GeneratePasswordResetTokenAsync(user);
        }

        public override async Task<IdentityResult> ResetPasswordAsync(string userName, string key, string password)
        {
            var user = await FindByNameAsync(userName);

            return await ResetPasswordAsync(user, key, password);
        }

        public override IdentityResult SignOutClient(Model.IdentityUser user, string clientKey)
        {
            try
            {
                if (string.IsNullOrEmpty(clientKey))
                {
                    throw new ArgumentNullException(nameof(clientKey));
                }
                var clients = user.User.Clients.Where(c => c.ClientKey == clientKey).ToList();
                foreach (var client in clients)
                {
                    user.User.RemoveClients(client);
                }
                user.CurrentClientId = null;
                return UpdateAsync(user).GetAwaiter().GetResult();
            }
            catch (Exception x)
            {
                LogManager.GetLogger(GetType()).Error("Could not sign out user client.", x);
                return IdentityResult.Failed(new IdentityError()
                {
                    Description = x.Message
                });
            }
        }

        public override IdentityResult SignOutClientById(Model.IdentityUser user, int clientId)
        {
            var client = user.User.Clients.FirstOrDefault(c => c.Id == clientId);
            if (client != null)
            {
                user.User.RemoveClients(client);
            }
            user.CurrentClientId = null;
            return UpdateAsync(user).GetAwaiter().GetResult();
        }

        public override IdentityResult SignInClient(string username, string clientKey, string userHostAddress, string sessionId)
        {
            return SignInClient(FindByName(username), clientKey, userHostAddress, sessionId);
        }

        public override IdentityResult SignInClient(Model.IdentityUser user, string clientKey, string userHostAddress, string sessionId)
        {
            if (string.IsNullOrEmpty(clientKey))
            {
                throw new ArgumentNullException(nameof(clientKey));
            }
            try
            {
                var client = user.User.Clients.SingleOrDefault(c => c.ClientKey == clientKey && c.SessionId == sessionId);
                if (client == null)
                {
                    client = new ApplicationClient
                    {
                        ClientKey = clientKey,
                        IPAddress = userHostAddress,
                        SessionId = sessionId,
                        ConnectedOn = DateTime.UtcNow
                    };
                    user.User.AddClients(client);
                }
                else
                {
                    client.ConnectedOn = DateTime.UtcNow;
                }
                var result = UpdateAsync(user).GetAwaiter().GetResult();
                //var repo = new Repository();
                //repo.SaveApplicationClient(client);
                user.CurrentClientId = client.Id.ToString();
                //var result = new IdentityResult();
                return result;
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(CustomUserManager)).Error($"SignInClient for user: '{user?.Id}' failed", e);
                return null;
            }
        }

        #region External Account Functions

        public override IdentityResult SignInClient(ExternalLoginInfo loginInfo, string clientKey, string userHostAddress, string sessionID)
        {
            if (string.IsNullOrEmpty(clientKey))
            {
                throw new ArgumentNullException(nameof(loginInfo));
            }
            if (loginInfo == null)
            {
                throw new ArgumentNullException(nameof(loginInfo));
            }
            var user = Find(loginInfo);
            var client = user.User.Clients.SingleOrDefault(c => c.ClientKey == clientKey && c.SessionId == sessionID);
            if (client == null)
            {
                client = new ApplicationClient
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
            var result = this.UpdateAsync(user).GetAwaiter().GetResult();
            user.CurrentClientId = client.Id.ToString();
            return result;
        }

        public override IdentityResult LinkExternalAccount(ExternalLoginInfo loginInfo)
        {
            if (loginInfo == null)
            {
                throw new ArgumentNullException(nameof(loginInfo));
            }

            var user = IdentityHelper.GetCurrentIdentityUser();
            var externalLogin = user.User.Logins.FirstOrDefault(x => x.ProviderKey == loginInfo.ProviderKey && x.LoginProvider == loginInfo.LoginProvider);
            if (externalLogin == null)
            {
                return this.AddLoginAsync(user, new UserLoginInfo(loginInfo.LoginProvider, loginInfo.ProviderKey, loginInfo.ProviderDisplayName)).GetAwaiter().GetResult();
            }
            return null;
        }
        #endregion
    }

}
#endif