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
using Microsoft.Extensions.DependencyInjection.Extensions;
using zAppDev.DotNet.Framework.IdentityServer.Configuration;
using zAppDev.DotNet.Framework.IdentityServer.Services;
using zAppDev.DotNet.Framework.Identity;

namespace zAppDev.DotNet.Framework.IdentityServer.IdentityServer
{
    public static class ServiceCollectionExtensions
    {
        private static IdentityServerConfiguration ReadIdentityServerConfigurationFromConfiguration(IConfiguration configuration)
        {
            var key = EncodingUtilities.StringToByteArray(configuration.GetValue("configuration:appSettings:add:JWTKey:value", "MIksRlTn0KG6nmjW*fzq*FYTY0RifkNQE%QTqdfS81CgNEGtUmMCY5XEgPTSL&28"), "ascii");

            var identityServerConfiguration = new IdentityServerConfiguration
            {
                Authority = configuration.GetValue("configuration:appSettings:add:IdentityServer:Authority:value", ""),
                AuthenticationCookieName = configuration.GetValue("configuration:appSettings:add:AuthenticationCookieName:value", ""),
                ClientId = configuration.GetValue("configuration:appSettings:add:IdentityServer:ClientId:value", ""),
                ClientSecret = configuration.GetValue("configuration:appSettings:add:IdentityServer:ClientSecret:value", ""),
                JWTKey = key,
            };

            return identityServerConfiguration;
        }
        public static void AddIdentityServerManager(this IServiceCollection services, IConfiguration configuration)
        {
            var identityServerConfiguration = ReadIdentityServerConfigurationFromConfiguration(configuration);

            services.AddSingleton<IIdentityServerConfiguration>(identityServerConfiguration);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;

            });

            services.AddCustomIdentityServerAuthentication<Identity.Model.IdentityUser, IdentityRole>(identityServerConfiguration, 
                options =>
                {

                })
                .AddRoles<IdentityRole>()
                .AddRoleStore<RoleStore<IdentityRole>>()
                .AddUserStore<UserStore>()
                .AddUserManager<CustomUserManager>()
                .AddDefaultTokenProviders();
           
            services.AddTransient<ZappDevUserManager, IdentityServerUserManager>();
            services.AddTransient<SignInManager<Identity.Model.IdentityUser>>();

            services.AddSingleton<OperationAuthorizationService>();
            services.AddSingleton<IAuthorizationPolicyProvider, OperationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, OperationAuthorizationHandler>();
        }
    }

    public static class CustomIdentityServiceCollectionExtensions
    {
        public static IdentityBuilder AddCustomIdentityServerAuthentication<TUser, TRole>(
            this IServiceCollection services,
            IdentityServerConfiguration identityServerConfiguration,
            Action<IdentityOptions> setupAction)
            where TUser : class
            where TRole : class
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie(IdentityConstants.ApplicationScheme, o =>
                {
                    o.Cookie.Name = $".{identityServerConfiguration.AuthenticationCookieName}.{IdentityConstants.ApplicationScheme}";
                    o.LoginPath = new PathString("/SignInPage/Load");
                    o.ReturnUrlParameter = "returnUrl";
                    o.LogoutPath = new PathString("/Login/Logout");
                    o.AccessDeniedPath = new PathString("/Unauthorized/Render");
                    o.ExpireTimeSpan = TimeSpan.FromDays(1);
                    o.SlidingExpiration = true;
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = identityServerConfiguration.Authority;

                    options.ClientId = identityServerConfiguration.ClientId;
                    options.ClientSecret = identityServerConfiguration.ClientSecret;
                    options.ResponseType = "code";

                    options.SaveTokens = true;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = identityServerConfiguration.Authority;
                    options.Audience = "resourceapi";

                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(identityServerConfiguration.JWTKey),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
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