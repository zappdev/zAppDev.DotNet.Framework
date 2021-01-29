// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace zAppDev.DotNet.Framework.Identity
{
    public abstract class ZappDevUserManager :  UserManager<Model.IdentityUser>
    {
        public ZappDevUserManager(
            IUserStore<Model.IdentityUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<Model.IdentityUser> passwordHasher,
            IEnumerable<IUserValidator<Model.IdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<Model.IdentityUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<ZappDevUserManager> logger) :
            base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {

        }

        public abstract Model.IdentityUser Find(ExternalLoginInfo loginInfo);
        public abstract Model.IdentityUser Find(string username, string password);
        public abstract Model.IdentityUser FindById(string userId);
        public abstract Model.IdentityUser FindByName(string userName);
        public abstract Task<string> GeneratePasswordResetTokenAsync(string userId);
        public abstract IdentityResult LinkExternalAccount(ExternalLoginInfo loginInfo);
        public abstract Task<IdentityResult> ResetPasswordAsync(string userName, string key, string password);
        public abstract IdentityResult SignInClient(ExternalLoginInfo loginInfo, string clientKey, string userHostAddress, string sessionID);
        public abstract IdentityResult SignInClient(Model.IdentityUser user, string clientKey, string userHostAddress, string sessionId);
        public abstract IdentityResult SignInClient(string username, string clientKey, string userHostAddress, string sessionId);
        public abstract IdentityResult SignOutClient(Model.IdentityUser user, string clientKey);
        public abstract IdentityResult SignOutClientById(Model.IdentityUser user, int clientId);
    }
}
#endif