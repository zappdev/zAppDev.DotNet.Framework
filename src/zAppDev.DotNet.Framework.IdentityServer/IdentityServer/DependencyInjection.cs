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
using System.Linq;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Data;
using System.Security.Claims;
using System.Collections.Generic;
using zAppDev.DotNet.Framework.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace zAppDev.DotNet.Framework.IdentityServer.IdentityServer
{
    public static class ServiceCollectionExtensions
    {
        private static IIdentityServerConfiguration ReadIdentityServerConfigurationFromConfiguration(IConfiguration configuration,
                                                                                                     IIdentityServerConfiguration identityServerConfiguration = null)
        {
            if (identityServerConfiguration == null)
            {
                identityServerConfiguration = new IdentityServerConfiguration();
            }

            var key = EncodingUtilities.StringToByteArray(configuration.GetValue("configuration:appSettings:add:JWTKey:value", "MIksRlTn0KG6nmjW*fzq*FYTY0RifkNQE%QTqdfS81CgNEGtUmMCY5XEgPTSL&28"), "ascii");

            var scopes = configuration.GetValue("configuration:appSettings:add:IdentityServer:Scope:value", "")
                                      .Split(",")
                                      .Select(scope => scope.Trim())
                                      .Where(scope => !string.IsNullOrEmpty(scope))
                                      .ToList();

            identityServerConfiguration.Authority = configuration.GetValue("configuration:appSettings:add:IdentityServer:Authority:value", "");
            identityServerConfiguration.AuthenticationCookieName = configuration.GetValue("configuration:appSettings:add:AuthenticationCookieName:value", "AspNetCore");
            identityServerConfiguration.ClientId = configuration.GetValue("configuration:appSettings:add:IdentityServer:ClientId:value", "");
            identityServerConfiguration.Scopes = scopes;
            identityServerConfiguration.ClientSecret = configuration.GetValue("configuration:appSettings:add:IdentityServer:ClientSecret:value", "");
            identityServerConfiguration.JWTKey = key;

            return identityServerConfiguration;
        }
        public static void AddIdentityServerManager(this IServiceCollection services, IConfiguration configuration, IIdentityServerConfiguration identityServerConfiguration)
        {
            identityServerConfiguration = ReadIdentityServerConfigurationFromConfiguration(configuration, identityServerConfiguration);

            services.AddSingleton<IIdentityServerConfiguration>(identityServerConfiguration);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;

            });

            services.AddCustomIdentityServerAuthentication<Identity.Model.IdentityUser, IdentityRole>(
                identityServerConfiguration,
                options =>
                {

                })
                    .AddRoles<IdentityRole>()
                    .AddRoleStore<RoleStore<IdentityRole>>()
                    .AddUserStore<UserStore>()
                    .AddUserManager<IdentityServerUserManager>()
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
            IIdentityServerConfiguration identityServerConfiguration,
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
                    o.LoginPath = new PathString("/IdentityServer/ChallengeIdentity/HomePage/Render/HomePage/Render");
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

                    options.GetClaimsFromUserInfoEndpoint = true;

                    foreach (var scope in identityServerConfiguration.Scopes)
                        options.Scope.Add(scope);

                    options.ResponseType = "code";
                    options.ResponseMode = "query";

                    options.SaveTokens = true;

                    //options.Events.OnAuthorizationCodeReceived = (context) => {
                    //    return Task.CompletedTask;
                    //};

                    options.Events.OnTicketReceived = async (context) =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<IdentityServerUserManager>>();
                        var manager = context.HttpContext.RequestServices.GetRequiredService<ZappDevUserManager>();

                        var username = context.Principal.FindFirst(identityServerConfiguration.UsernameClaim);

                        if (username != null)
                        {
                            var user = manager.FindByName(username.Value);

                            var email = (string.IsNullOrEmpty(identityServerConfiguration.EmailClaim))
                                    ? "" : context.Principal.FindFirst(identityServerConfiguration.EmailClaim)?.Value ?? "";
                            var name = (string.IsNullOrEmpty(identityServerConfiguration.NameClaim))
                                ? "" : context.Principal.FindFirst(identityServerConfiguration.NameClaim)?.Value ?? "";

                            if (user != null)
                            {
                                // Update information from Identity Server
                                if (string.IsNullOrEmpty(identityServerConfiguration.NameClaim))
                                {
                                    user.User.Name = name;
                                }
                                if (string.IsNullOrEmpty(identityServerConfiguration.EmailClaim)) 
                                {
                                    user.User.Email = email;
                                    user.Email = email;                                    
                                }
                                await manager.UpdateAsync(user);
                                context.HttpContext.RequestServices.GetRequiredService<IMiniSessionService>().CommitChanges();
                                logger.LogInformation("Local user updated successfully! ");
                            }
                            else
                            {
                                // Create local user
                                user = IdentityHelper.GetIdentityUser(username.Value, email, name, identityServerConfiguration.UserClass);

                                var result = await manager.CreateAsync(user);

                                if (result.Succeeded)
                                {
                                    context.HttpContext.RequestServices.GetRequiredService<IMiniSessionService>().CommitChanges();
                                    identityServerConfiguration?.ExternalUserCreating(user.User);
                                    logger.LogInformation("Local user created successfully!");
                                } 
                                else
                                {
                                    logger.LogInformation($"Local user not created! Errors:\t{string.Join(Environment.NewLine, result.Errors.Select(e => e.Description))}");
                                }
                            }
                            
                            if (context.Principal.Identity is ClaimsIdentity claimIdentity) 
                            {
                                claimIdentity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
                                claimIdentity.AddClaim(new Claim(ClaimTypes.Email, user.User.Email));
                            }
                        } 
                        else
                        {
                            logger.LogInformation("Username is empty!");
                        }
                    };

                    //options.Events.OnTokenResponseReceived = (context) => {
                    //    return Task.CompletedTask;
                    //};
                    //options.CallbackPath = new PathString("/IdentityServer/HandleSuccess");
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