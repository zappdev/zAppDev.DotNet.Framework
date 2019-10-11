#if NETFRAMEWORK
#else
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Mvc.Exceptions
{
    public class ExceptionActionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<ExceptionActionFilter> _logger;

        public ExceptionActionFilter(
            ILogger<ExceptionActionFilter> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            var actionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
            Type controllerType = actionDescriptor.ControllerTypeInfo;

            var controllerBase = typeof(ControllerBase);
            var controller = typeof(Controller);

            // Api's implements ControllerBase but not Controller
            if (controllerType.IsSubclassOf(controllerBase) && !controllerType.IsSubclassOf(controller))
            {
                HandleApiException(context);
            }

            // Pages implements ControllerBase and Controller
            if (controllerType.IsSubclassOf(controllerBase) && controllerType.IsSubclassOf(controller))
            {
                // Handle page exception
            }

            base.OnException(context);
        }

        private void HandleApiException(ExceptionContext context)
        {
            if (context.Exception == null) return;

            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ExceptionActionFilter>>();

            logger.LogError(context.Exception, "Exception occurs when action is executed");
            try
            {
                var msgObject = new ExceptionHandler()
                    .HandleException(context.Exception);

                context.Result = new ObjectResult(msgObject)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, "Could not produce friendly message for exception!");

                context.Result = new ObjectResult(context.Exception)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}

#endif