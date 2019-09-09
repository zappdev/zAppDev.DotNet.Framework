// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using zAppDev.DotNet.Framework.Logging;
using zAppDev.DotNet.Framework.Identity;
using zAppDev.DotNet.Framework.Identity.Model;

namespace zAppDev.DotNet.Framework.WebApi
{
    public class CustomWebApiAuthorizeAttributeBase : AuthorizeAttribute
    {
        protected IAPILogger MessagingPublisher =>
            GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IAPILogger)) as IAPILogger;

        protected readonly Guid _id = Guid.NewGuid();
        protected Stopwatch _stopwatch;

        public bool LogEnabled
        {
            get;
            set;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            var service = actionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var method = actionContext.ActionDescriptor.ActionName;
            var isAuthenticated = actionContext.RequestContext.Principal.Identity.IsAuthenticated;
            var identityName = isAuthenticated ? actionContext.RequestContext.Principal.Identity.Name : null;
            var identityNameExists = !string.IsNullOrWhiteSpace(identityName);

            if (isAuthenticated && identityNameExists)
            {
                if (!ClaimPermission.CheckAccess(ClaimTypes.ExposedService, service, method, identityName))
                {
                    this.HandleUnauthorizedRequest(actionContext);
                }
            }
            else
            {
                // Log this weird case
                if (isAuthenticated && !identityNameExists)
                {
                    log4net.LogManager.GetLogger(nameof(CustomWebApiAuthorizeAttributeBase)).Warn($"Authenticated user without Identity.Name! Handling as unauthenticated... ({service}/{method})");
                }

                if (!ClaimPermission.CheckAccess(ClaimTypes.ExposedService, service, method))
                {
                    this.HandleUnauthenticatedRequest(actionContext);
                }
            }
        }

        protected void HandleUnauthenticatedRequest(HttpActionContext actionContext, string msg = "")
        {
            CreateResponseAndLog(actionContext, HttpStatusCode.Unauthorized, msg);
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            CreateResponseAndLog(actionContext, HttpStatusCode.Forbidden);
        }

        protected void CreateResponseAndLog(HttpActionContext actionContext, HttpStatusCode status, string msg = "")
        {
            var errorCode = string.IsNullOrWhiteSpace(msg) ?
                        (status == HttpStatusCode.Forbidden ? "You are not authorized!" : "Please provide credentials!") :
                        msg;

            actionContext.Response = actionContext.Request.CreateErrorResponse(status, errorCode);

            _stopwatch.Stop();

            if (!LogEnabled) return;

            // TODO - evaluate if this is needed. Maybe it should be refactored to a pure access log but it needs to also record IP, etc.
            IdentityHelper.LogAction(
                actionContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                actionContext.ActionDescriptor.ActionName,
                false,
                actionContext.Response.StatusCode.ToString());
        }
    }

    public class ApiManagementLogMessage : LogMessage
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string Product { get; set; }

        public ApiManagementLogMessage()
        {

        }

        public ApiManagementLogMessage(Guid requestId, HttpRequestMessage request, HttpResponseMessage response, TimeSpan processingTime, bool cacheHit) :
            base(requestId, request, response, processingTime, cacheHit)
        {

        }

        public override string ToString()
        {
            return $"{RequestMethod} {RequestUri} - {StatusCode} {Message} - {ElapsedMsecs} msecs, user: {Username}, ip: {IP}, Product: {Product}, Client: {ClientName}, ClientId: {ClientId}";
        }
    }
}
#endif