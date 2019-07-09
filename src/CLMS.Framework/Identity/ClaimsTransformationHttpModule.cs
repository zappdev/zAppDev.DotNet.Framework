using System;
using System.Security.Claims;
using System.Threading;
using System.Web;

#if NETFRAMEWORK
using System.IdentityModel.Services;
#else
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
#endif

namespace CLMS.Framework.Identity
{
    /// <summary>
    /// This module must be used when we have Windows Authentication, because
    /// in that case the ClaimsAuthenticationManager of Identity is not called.
    /// </summary>
#if NETFRAMEWORK
    public class ClaimsTransformationHttpModule : IHttpModule
    {
        public void Dispose()
        { }

        public void Init(HttpApplication context)
        {
            context.PostAuthenticateRequest += Context_PostAuthenticateRequest;
        }

        void Context_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;
            // no need to call transformation if session already exists
            if (FederatedAuthentication.SessionAuthenticationModule != null
                    && FederatedAuthentication.SessionAuthenticationModule.ContainsSessionTokenCookie(context.Request.Cookies))
            {
                return;
            }
            var transformer =
                FederatedAuthentication.FederationConfiguration.IdentityConfiguration.ClaimsAuthenticationManager;
            if (transformer != null)
            {
                var transformedPrincipal = transformer.Authenticate(context.Request.RawUrl, context.User as ClaimsPrincipal);
                context.User = transformedPrincipal;
                Thread.CurrentPrincipal = transformedPrincipal;
            }
        }
    }
#else
    public class ClaimsTransformationHttpMiddleware : IDisposable
    {
        private readonly RequestDelegate _next;

        public ClaimsTransformationHttpMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next.Invoke(context);
        }

        public void Dispose()
        {

        }
    }

    public static class ClaimsTransformationHttpMiddlewareExtensions
    {
        public static IApplicationBuilder UseClaimsTransformationHttpMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ClaimsTransformationHttpMiddleware>();
        }
    }
#endif
}