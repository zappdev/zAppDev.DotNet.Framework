// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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

        private readonly IConfiguration _configuration;
      
        public ApiManagerMiddleware(
            RequestDelegate next,
            IAPILogger logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        } 

        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();

            ServiceLocator.SetLocatorProvider(_serviceProvider);

            context.Items[HttpContextItemKeys.LogTimer] = Stopwatch.StartNew();
            context.Items[HttpContextItemKeys.RequestIsLogged] = false;

            var id = Guid.NewGuid();
            context.Items[HttpContextItemKeys.ExposedServiceRequestId] = id;
            context.TraceIdentifier = id.ToString();

            await _next(context);
            if (context.IsAllowPartialResponseEnabled())
                CreateByVariableTypeDtoResponse(context);

            var timer = (Stopwatch)context.Items[HttpContextItemKeys.LogTimer];
            timer.Stop();
            var _elapsed = timer.Elapsed;
            
            var pathValue = context.Request.Path.Value;
            var confValue = _configuration?.GetValue<string>($"configuration:apiLogSettings:add:{pathValue}:value");
            bool logEnabled = true;
            if (bool.TryParse(confValue, out bool _logEnabled))
            {
                logEnabled = _logEnabled;
            }
            if (!context.IsLogEnabled() || !logEnabled) return;

            //IdentityHelper.LogAction(
            //    filterContext.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName,
            //    filterContext.ActionContext.ActionDescriptor.ActionName,
            //    filterContext.Exception == null,
            //    filterContext.Exception?.Message);

            var logTimer = Stopwatch.StartNew();

            var metadataStruct = new ExposedServiceMetadataStruct(context, _elapsed);

            if (!(bool)context.Items[HttpContextItemKeys.RequestIsLogged])
            {
                //_logger?.LogExposedAPIAccess(context.GetController(), context.GetAction(), id, _elapsed, false);
                _logger?.LogExposedAPIMetadata(metadataStruct);
                context.Items[HttpContextItemKeys.RequestIsLogged] = true;
            }

            logTimer.Stop();
            var logger = log4net.LogManager.GetLogger(typeof(ApiManagerMiddleware));
            logger.Debug($"Logging time took : {logTimer.ElapsedMilliseconds} ms");
        }

        private void CreateByVariableTypeDtoResponse(HttpContext context)
        {

        }
    }
}
#endif