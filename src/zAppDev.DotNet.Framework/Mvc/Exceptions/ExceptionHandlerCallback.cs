// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.

#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WebApiContrib.Core.Results;
using zAppDev.DotNet.Framework.Mvc.API;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Mvc
{
    public static class ExceptionHandlerCallback
    {
        public static async Task HandleException(HttpContext context)
        {
            var exceptionHandlerPathFeature =
                context.Features.Get<IExceptionHandlerPathFeature>();

            if (context.Items.ContainsKey(HttpContextItemKeys.ExposedServiceRequestId))
            {
                var logger = ServiceLocator.Current.GetInstance<ILogger<ExceptionHandler>>();

                logger.LogError(exceptionHandlerPathFeature.Error, "Exception occurs when action is executed");
                try
                {
                    var msgObject = new ExceptionHandler()
                        .HandleException(exceptionHandlerPathFeature.Error);
                    await context.BadRequest(msgObject);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Could not produce friendly message for exception!");
                    await context.BadRequest(exceptionHandlerPathFeature.Error);
                }
            }
        }
    }
}
#endif