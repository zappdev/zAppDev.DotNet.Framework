// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Logging;
using zAppDev.DotNet.Framework.Mvc.API;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Middleware
{
    public class ApiManagerMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IAPILogger _logger;

        private readonly IServiceProvider _serviceProvider;

        public ApiManagerMiddleware(
            RequestDelegate next,
            IAPILogger logger,
            IServiceProvider serviceProvider)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
        } 

        public async Task Invoke(HttpContext context)
        {
            ServiceLocator.SetLocatorProvider(_serviceProvider);

            context.Items[ApiManagerItemsKeys.LogTimer] = Stopwatch.StartNew();
            context.Items[ApiManagerItemsKeys.RequestIsLogged] = false;

            var id = Guid.NewGuid();
            context.Items[ApiManagerItemsKeys.ExposedServiceRequestId] = id;
            context.TraceIdentifier = id.ToString();
            try
            {
                await _next(context);
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