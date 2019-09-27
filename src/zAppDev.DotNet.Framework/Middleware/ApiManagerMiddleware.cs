// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Data;
using zAppDev.DotNet.Framework.Logging;
using zAppDev.DotNet.Framework.Mvc.API;

namespace zAppDev.DotNet.Framework.Middleware
{
    public class ApiManagerMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IMiniSessionService _miniSessionService;

        private readonly IAPILogger _logger;

        public ApiManagerMiddleware(RequestDelegate next, IMiniSessionService miniSessionService, IAPILogger logger)
        {
            _next = next;
            _logger = logger;
            _miniSessionService = miniSessionService;
        } 

        public async Task Invoke(HttpContext context)
        {
            context.Items[ApiManagerItemsKeys.LogTimer] = Stopwatch.StartNew();
            context.Items[ApiManagerItemsKeys.RequestIsLogged] = false;

            var id = Guid.NewGuid();
            context.Items[ApiManagerItemsKeys.ExposedServiceRequestId] = id;
            context.TraceIdentifier = id.ToString();
            try
            {
                _miniSessionService.OpenSessionWithTransaction();
                await _next(context);
                _miniSessionService.CommitChanges();
                if (context.IsAllowPartialResponseEnabled()) CreateByVariableTypeDtoResponse(context);
            }
            catch (Exception ex)
            {
                HandleException(context, ex);
            }
            var timer = (Stopwatch)context.Items[ApiManagerItemsKeys.LogTimer];
            timer.Stop();
            var _elapsed = timer.Elapsed;
            if (!context.IsLogEnabled()) return;

            //IdentityHelper.LogAction(
            //    filterContext.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName,
            //    filterContext.ActionContext.ActionDescriptor.ActionName,
            //    filterContext.Exception == null,
            //    filterContext.Exception?.Message);

            if (!(bool)context.Items[ApiManagerItemsKeys.RequestIsLogged])
            {
                _logger?.LogExposedAPIAccess(context.GetController(), context.GetAction(), id, _elapsed, false);
                context.Items[ApiManagerItemsKeys.RequestIsLogged] = true;
            }
        }

        private void CreateByVariableTypeDtoResponse(HttpContext context)
        {

        }

        private void HandleException(HttpContext context, Exception ex)
        {
            
        }
    }
}
#endif