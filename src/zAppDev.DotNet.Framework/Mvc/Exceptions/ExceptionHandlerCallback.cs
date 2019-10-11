#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Mvc.API;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Mvc
{
    public static class ExceptionHandlerCallback
    {
        public static async Task HandleException(HttpContext context)
        {
            context.Response.StatusCode = 500;
            var exceptionHandlerPathFeature =
                context.Features.Get<IExceptionHandlerPathFeature>();

            if (context.Items.ContainsKey(ApiManagerItemsKeys.ExposedServiceRequestId))
            {
                var logger = ServiceLocator.Current.GetInstance<ILogger<ExceptionHandler>>();

                logger.LogError(exceptionHandlerPathFeature.Error, "Exception occurs when action is executed");
                try
                {
                    var msgObject = new ExceptionHandler()
                        .HandleException(exceptionHandlerPathFeature.Error);
                    await context.WriteModelAsync(msgObject);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Could not produce friendly message for exception!");
                    await context.WriteModelAsync(exceptionHandlerPathFeature.Error);
                }
            }
        }
    }
}
#endif