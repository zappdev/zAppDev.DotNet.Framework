// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
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
using WebApiContrib.Core.Results;
using zAppDev.DotNet.Framework.Locales;
using zAppDev.DotNet.Framework.Mvc.API;
using System.Globalization;

namespace zAppDev.DotNet.Framework.Mvc.Exceptions
{
    public class ExceptionActionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<ExceptionActionFilter> _logger;
        private readonly ILocalesService _localesService;

        public ExceptionActionFilter(
            ILogger<ExceptionActionFilter> logger,
            ILocalesService localesService)
        {
            _logger = logger;
            _localesService = localesService;
        }

        public override void OnException(ExceptionContext context)
        {
            var actionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
            Type controllerType = actionDescriptor.ControllerTypeInfo;

            var controllerBase = typeof(ControllerBase);
            var controller = typeof(Controller);

            _logger.LogError(context.Exception, $"Exception occurs when Action[{actionDescriptor.ControllerName}.{actionDescriptor.ActionName}] is executed");

            // Api's implements ControllerBase but not Controller
            if (controllerType.IsSubclassOf(controllerBase) && !controllerType.IsSubclassOf(controller))
            {
                HandleApiException(context);
            }

            // Pages implements ControllerBase and Controller
            if (controllerType.IsSubclassOf(controllerBase) && controllerType.IsSubclassOf(controller))
            {
                Common.SetLastError(context.Exception);
                HandleWebException(context);
            }

            base.OnException(context);
        }

        private void HandleWebException(ExceptionContext context)
        {
            if (context.Exception == null) return;

            HandleException(context);
        }

        private NHibernate.StaleObjectStateException EnrichStaleObjectStateException(ExceptionContext context)
        {
            var staleObjectStateException = context.Exception as NHibernate.StaleObjectStateException;

            //If it's already enriched, return it and get out
            if (staleObjectStateException.Data["ZAPPDEV_MESSAGE"] != null)
            {
                return staleObjectStateException;
            }

            var culture_id = GetCultureId(context);
       
            //Otherwise, enrich it
            var defaultMessage = @"Somebody already committed modifications to the data you are about to commit. <br/>You have been working with <i>stale data</i>.<br/>Please, refresh your page in order to restart with fresh data.";
            var defaultTitle = "Stale Data";
            var message = _localesService.GetResourceValue("GlobalResources", "RES_SITE_StaleObjectStateExceptionMessage", culture_id).ToString();
            var title = _localesService.GetResourceValue("GlobalResources", "RES_SITE_StaleObjectStateExceptionTitle", culture_id).ToString();
            if (string.IsNullOrWhiteSpace(message)) message = defaultMessage;
            if (string.IsNullOrWhiteSpace(title)) title = defaultTitle;
            staleObjectStateException.Data.Add("ZAPPDEV_MESSAGE", message);
            staleObjectStateException.Data.Add("ZAPPDEV_TITLE", title);
            return staleObjectStateException;
        }

        private string GetCultureId(ExceptionContext context)
        {
            if (context.HttpContext.Items.ContainsKey(HttpContextItemKeys.Culture)) return null;
            return (context.HttpContext.Items[HttpContextItemKeys.Culture] as CultureInfo).Name.ToLowerInvariant();
        }

        private void HandleApiException(ExceptionContext context)
        {
            if (context.Exception == null) return;

            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ExceptionActionFilter>>();

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

        private void HandleException(ExceptionContext context)
        {
            if (context?.Exception?.GetType() == typeof(NHibernate.StaleObjectStateException))
            {
                context.Exception = EnrichStaleObjectStateException(context);
            }
            context.ExceptionHandled = true;
            context.Result = GetErrorResult(context, "Render", "ErrorPage");
        }

        private ActionResult GetErrorResult(ExceptionContext context, string actionName, string formName)
        {
            string errorContent;
            try
            {
                context.HttpContext.Response.StatusCode = 500;
                errorContent = SerializeException(context);
            }
            catch (Exception e)
            {
                _logger.LogError("Could not produce friendly message for exception!", e);
                errorContent = context.Exception.Message;
            }

            if (context.HttpContext.Request.IsAjaxRequest())
            {
                return new BadRequestObjectResult(new
                {
                    Type = "Error",
                    Data = "",
                    StackTrace = context.Exception.StackTrace,
                    RedirectURL = $"{formName}/{actionName}"
                });
            }
            return new RedirectToActionResult(actionName, formName, new
            {
                action = actionName,
                controller = formName
            });
        }

        private static string SerializeException(ExceptionContext context)
        {
            var friendlyMessageHandler = new ExceptionHandler();
            var msgObject = friendlyMessageHandler.HandleException(context.Exception);
            return Newtonsoft.Json.JsonConvert.SerializeObject(msgObject,
               new Newtonsoft.Json.JsonSerializerSettings
               {
                   PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects
               });
        }
    }
}

#endif