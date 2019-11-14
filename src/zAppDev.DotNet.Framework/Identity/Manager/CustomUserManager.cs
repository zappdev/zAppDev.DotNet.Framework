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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using zAppDev.DotNet.Framework.Identity.Model;
using zAppDev.DotNet.Framework.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace zAppDev.DotNet.Framework.Identity
{

    public class CustomUserManager : UserManager<Model.IdentityUser>
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

        public Model.IdentityUser Find(ExternalLoginInfo loginInfo)
        {
            if (!SupportsUserLogin)
                throw new ApplicationException("UserManager don't support user login");

            if (Store is UserStore storeImp)
            {
                return storeImp.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey).Result;
            }
            return null;
        }

        public Model.IdentityUser Find(string username, string password)
        {
            var user = FindById(username);
            if (user == null) return null;

            var pass = ValidatePasswordAsync(user, password).Result;

            return (pass.Succeeded) ? user : null;
        }

        public Model.IdentityUser FindById(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            return base.FindByIdAsync(userId).Result;
        }

        public Model.IdentityUser FindByName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return null;
            }
            return base.FindByNameAsync(userName).Result;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string userId)
        {
            var user = await FindByIdAsync(userId);
            return await GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetPasswordAsync(string userName, string key, string password)
        {
            var user = await FindByNameAsync(userName);

            return await ResetPasswordAsync(user, key, password);
        }

        public IdentityResult SignOutClient(Model.IdentityUser user, string clientKey)
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
                return UpdateAsync(user).Result;
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

        public IdentityResult SignOutClientById(Model.IdentityUser user, int clientId)
        {
            var client = user.User.Clients.FirstOrDefault(c => c.Id == clientId);
            if (client != null)
            {
                user.User.RemoveClients(client);
            }
            user.CurrentClientId = null;
            return UpdateAsync(user).Result;
        }

        public IdentityResult SignInClient(string username, string clientKey, string userHostAddress, string sessionId)
        {
            return SignInClient(FindByName(username), clientKey, userHostAddress, sessionId);
        }

        public IdentityResult SignInClient(Model.IdentityUser user, string clientKey, string userHostAddress, string sessionId)
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
                var result = UpdateAsync(user).Result;
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
            var result = this.UpdateAsync(user).Result;
            user.CurrentClientId = client.Id.ToString();
            return result;
        }

        public IdentityResult LinkExternalAccount(ExternalLoginInfo loginInfo)
        {
            if (loginInfo == null)
            {
                throw new ArgumentNullException(nameof(loginInfo));
            }

            var user = IdentityHelper.GetCurrentIdentityUser();
            var externalLogin = user.User.Logins.FirstOrDefault(x => x.ProviderKey == loginInfo.ProviderKey && x.LoginProvider == loginInfo.LoginProvider);
            if (externalLogin == null)
            {
                return this.AddLoginAsync(user, new UserLoginInfo(loginInfo.LoginProvider, loginInfo.ProviderKey, loginInfo.ProviderDisplayName)).Result;
            }
            return null;
        }
        #endregion
    }

    public static class ServiceCollectionExtensions
    {
        public static void AddIdentityManager(this IServiceCollection services, IConfiguration configuration, PasswordPolicyConfig passwordPolicy = null, ExternalLoginConfig externalLoginConfig = null)
        {
            if (passwordPolicy == null)
            {
                passwordPolicy = new PasswordPolicyConfig();
            }

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = passwordPolicy.RequireDigit;
                options.Password.RequireLowercase = passwordPolicy.RequireLowercase;
                options.Password.RequireNonAlphanumeric = passwordPolicy.RequireNonLetterOrDigit;
                options.Password.RequireUppercase = passwordPolicy.RequireUppercase;
                options.Password.RequiredLength = passwordPolicy.RequiredLength;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.RequireUniqueEmail = false;
                options.User.AllowedUserNameCharacters = @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -._@+";
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddIdentity<Model.IdentityUser, IdentityRole>()
                .AddRoles<IdentityRole>()
                .AddRoleStore<RoleStore<IdentityRole>>()
                .AddUserStore<UserStore>()
                .AddUserManager<CustomUserManager>()
                .AddDefaultTokenProviders();

            var key = EncodingUtilities.StringToByteArray(configuration.GetValue("configuration:appSettings:add:JWTKey:value", "MIksRlTn0KG6nmjW*fzq*FYTY0RifkNQE%QTqdfS81CgNEGtUmMCY5XEgPTSL&28"), "ascii");

            var authenticationBuilder = services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = "Identity.Application";
                });

            authenticationBuilder.AddCookie(options =>
            {
                // Cookie authentication settings
                options.LoginPath = "/SignInPage/Load";
                options.ReturnUrlParameter = "returnUrl";
                options.LogoutPath = "/Login/Logout";
                options.AccessDeniedPath = "/Unauthorized/Render";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.SlidingExpiration = true;
            });

            authenticationBuilder.AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };                
            });

            if (externalLoginConfig != null && externalLoginConfig.IsGoogleEnabled)
            {
                authenticationBuilder.AddGoogle(options =>
                {
                    options.ClientId = externalLoginConfig.GoogleClientId;
                    options.ClientSecret = externalLoginConfig.GoogleClientSecret;
                });
            }

            if (externalLoginConfig != null && externalLoginConfig.IsFacebookEnabled)
            {
                authenticationBuilder.AddFacebook(options =>
                {
                    options.AppId = externalLoginConfig.FacebookClientId;
                    options.AppSecret = externalLoginConfig.FacebookClientSecret;
                });
            }

            services.AddTransient<CustomUserManager>();
            services.AddTransient<SignInManager<Model.IdentityUser>>();

            services.AddSingleton<OperationAuthorizationService>();
            services.AddSingleton<IAuthorizationPolicyProvider, OperationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, OperationAuthorizationHandler>();

            //services.AddAuthentication(IISDefaults.AuthenticationScheme);
        }
    }

}
#endif