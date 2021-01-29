// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using zAppDev.DotNet.Framework.Utilities;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace zAppDev.DotNet.Framework.Identity
{
    public static class ServiceCollectionExtensions
    {
        private static PasswordPolicyConfig ReadPasswordPolicyFromConfiguration(IConfiguration configuration)
        {
            var passwordPolicy = new PasswordPolicyConfig
            {
                RequireDigit = true,
                RequiredLength = 6,
                RequireLowercase = true,
                RequireNonLetterOrDigit = true,
                RequireUppercase = true
            };

            //configuration.GetValue("configuration:appSettings:add:JWTKey:value", "MIksRlTn0KG6nmjW*fzq*FYTY0RifkNQE%QTqdfS81CgNEGtUmMCY5XEgPTSL&28"

            if (bool.TryParse(configuration.GetValue("configuration:appSettings:add:PasswordPolicy:RequireDigit:value", "true"), out bool requireDigit))
                passwordPolicy.RequireDigit = requireDigit;
            if (int.TryParse(configuration.GetValue("configuration:appSettings:add:PasswordPolicy:RequiredLength:value", "6"), out int requiredLength))
                passwordPolicy.RequiredLength = requiredLength;
            if (bool.TryParse(configuration.GetValue("configuration:appSettings:add:PasswordPolicy:RequireLowercase:value", "true"), out bool requireLowercase))
                passwordPolicy.RequireLowercase = requireLowercase;

            if (bool.TryParse(configuration.GetValue("configuration:appSettings:add:PasswordPolicy:RequireNonLetterOrDigit:value", "true"), out bool requireNonLetterOrDigit))
                passwordPolicy.RequireNonLetterOrDigit = requireNonLetterOrDigit;
            if (bool.TryParse(configuration.GetValue("configuration:appSettings:add:PasswordPolicy:RequireUppercase:value", "true"), out bool requireUppercase))
                passwordPolicy.RequireUppercase = requireUppercase;

            return passwordPolicy;
        }
        public static void AddIdentityManager(this IServiceCollection services, IConfiguration configuration, ExternalLoginConfig externalLoginConfig = null)
        {
            var passwordPolicy = ReadPasswordPolicyFromConfiguration(configuration);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;

            });

            services.AddCustomIdentity<Model.IdentityUser, IdentityRole>(options =>
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
                options.Lockout.AllowedForNewUsers = false;

                // User settings.
                options.User.RequireUniqueEmail = false;
                options.User.AllowedUserNameCharacters = @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -._@+";
            })
                .AddRoles<IdentityRole>()
                .AddRoleStore<RoleStore<IdentityRole>>()
                .AddUserStore<UserStore>()
                .AddUserManager<ZappDevUserManager>()
                .AddDefaultTokenProviders();

            var key = EncodingUtilities.StringToByteArray(configuration.GetValue("configuration:appSettings:add:JWTKey:value", "MIksRlTn0KG6nmjW*fzq*FYTY0RifkNQE%QTqdfS81CgNEGtUmMCY5XEgPTSL&28"), "ascii");

            var authenticationBuilder = services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
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

            services.AddTransient<ZappDevUserManager, CustomUserManager>();
            services.AddTransient<SignInManager<Model.IdentityUser>>();

            services.AddSingleton<OperationAuthorizationService>();
            services.AddSingleton<IAuthorizationPolicyProvider, OperationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, OperationAuthorizationHandler>();

            //services.AddAuthentication(IISDefaults.AuthenticationScheme);
        }
    }

    public static class CustomIdentityServiceCollectionExtensions
    {
        public static IdentityBuilder AddCustomIdentity<TUser, TRole>(
            this IServiceCollection services,
            Action<IdentityOptions> setupAction)
            where TUser : class
            where TRole : class
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddCookie(IdentityConstants.ApplicationScheme, o =>
            {
                // Cookie authentication settings
                o.Cookie.Name = IdentityConstants.ApplicationScheme;
                o.LoginPath = new PathString("/SignInPage/Load");
                o.ReturnUrlParameter = "returnUrl";
                o.LogoutPath = new PathString("/Login/Logout");
                o.AccessDeniedPath = new PathString("/Unauthorized/Render");
                o.ExpireTimeSpan = TimeSpan.FromDays(1);
                o.SlidingExpiration = true;
                o.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
                };
            })
            .AddCookie(IdentityConstants.ExternalScheme, o =>
            {
                o.Cookie.Name = IdentityConstants.ExternalScheme;
                o.SlidingExpiration = true;
                o.ExpireTimeSpan = TimeSpan.FromDays(1);
            })
            .AddCookie(IdentityConstants.TwoFactorRememberMeScheme, o =>
            {
                o.Cookie.Name = IdentityConstants.TwoFactorRememberMeScheme;
                o.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = SecurityStampValidator.ValidateAsync<ITwoFactorSecurityStampValidator>
                };
            })
            .AddCookie(IdentityConstants.TwoFactorUserIdScheme, o =>
            {
                o.Cookie.Name = IdentityConstants.TwoFactorUserIdScheme;
                o.ExpireTimeSpan = TimeSpan.FromDays(1);
                o.SlidingExpiration = true;
            });

            // Hosting doesn't add IHttpContextAccessor by default
            services.AddHttpContextAccessor();
            // Identity services
            services.TryAddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
            services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
            services.TryAddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
            services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            services.TryAddScoped<IRoleValidator<TRole>, RoleValidator<TRole>>();
            // No interface for the error describer so we can add errors without rev'ing the interface
            services.TryAddScoped<IdentityErrorDescriber>();
            services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<TUser>>();
            services.TryAddScoped<ITwoFactorSecurityStampValidator, TwoFactorSecurityStampValidator<TUser>>();
            services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser, TRole>>();

#if NETCOREAPP3_1 
            services.TryAddScoped<IUserConfirmation<TUser>, DefaultUserConfirmation<TUser>>();

#elif NET5_0
            services.TryAddScoped<IUserConfirmation<TUser>, DefaultUserConfirmation<TUser>>(); 
#endif



            services.TryAddScoped<UserManager<TUser>>();
            services.TryAddScoped<SignInManager<TUser>>();
            services.TryAddScoped<RoleManager<TRole>>();

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return new IdentityBuilder(typeof(TUser), typeof(TRole), services);
        }
    }

}
#endif