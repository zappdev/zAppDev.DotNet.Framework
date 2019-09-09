// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using System;
using System.Web;
using System.Globalization;
using System.Threading;

using log4net;
using zAppDev.DotNet.Framework.Data;
using zAppDev.DotNet.Framework.Data.Domain;
using zAppDev.DotNet.Framework.Utilities;

using Identity = zAppDev.DotNet.Framework.Identity;
using zAppDev.DotNet.Framework.Identity;
using zAppDev.DotNet.Framework.Identity.Model;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using zAppDev.DotNet.Framework.Hubs;

namespace zAppDev.DotNet.Framework.Mvc
{
    public class CustomControllerActionFilterBase : ActionFilterAttribute
    {
        public bool LogEnabled { get; set; }

        public bool CausesValidation { get; set; }

        public bool HasDefaultResultView { get; set; }

        public string ActionName { get; set; }

        public bool ListensToEvent { get; set; }

        public string ClaimType { get; set; }

        public bool FillDropDownInitialValues { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // do something before the action executes
            base.OnActionExecuting(context);

            var controller = (CustomControllerBase)context.Controller;

            var result = controller.PreActionFilterHook(CausesValidation, ListensToEvent, ActionName);

            if (result != null)
            {
                context.Result = result;
            }

            var response = context.HttpContext.Response;
            var manager = ServiceLocator.Current.GetInstance<IMiniSessionService>();
            manager.OpenSession();

            response.Headers.Add("zappuser", new string[] { IdentityHelper.GetCurrentUserName() });

            var culture = ProfileHelper.GetCurrentLocale();
            var id = culture.Id;
            var code = culture.Code;
            if (id == null) return;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(id.Value);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(id.Value);

            response.Headers.Add("Culture", new string[] { code });
        }

        protected static NHibernate.StaleObjectStateException EnrichStaleObjectStateException(NHibernate.StaleObjectStateException staleObjectStateException)
        {
            //If it's already enriched, return it and get out
            if (staleObjectStateException.Data["ZAPPDEV_MESSAGE"] != null)
            {
                return staleObjectStateException;
            }

            //Otherwise, enrich it
            string defaultMessage = @"Somebody already committed modifications to the data you are about to commit. <br/>You have been working with <i>stale data</i>.<br/>Please, refresh your page in order to restart with fresh data.";
            string defaultTitle = "Stale Data";
            string message = BaseViewPageBase<string>.GetResourceValue("GlobalResources", "RES_SITE_StaleObjectStateExceptionMessage").ToString();
            string title = BaseViewPageBase<string>.GetResourceValue("GlobalResources", "RES_SITE_StaleObjectStateExceptionTitle").ToString();
            if (string.IsNullOrWhiteSpace(message)) message = defaultMessage;
            if (string.IsNullOrWhiteSpace(title)) title = defaultTitle;
            staleObjectStateException.Data.Add("ZAPPDEV_MESSAGE", message);
            staleObjectStateException.Data.Add("ZAPPDEV_TITLE", title);
            return staleObjectStateException;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // do something after the action executes
            base.OnActionExecuted(context);

            try
            {
                var controller = (CustomControllerBase)context.Controller;

                var manager = ServiceLocator.Current.GetInstance<IMiniSessionService>();
                var miniSessionManagerWillFlush = manager.WillFlush;

                manager.CommitChanges(context.Exception, () =>
                {
                    if (miniSessionManagerWillFlush)
                    {
                        controller.CommitAllFiles();
                    }

                    if (!HasException(context) && context.Result is EmptyResult)
                    {
                        context.Result = controller.PostActionFilterHook(HasDefaultResultView, FillDropDownInitialValues);
                    }
                });
            }
            catch (NHibernate.StaleObjectStateException staleObjectStateException)
            {
                context.Exception = EnrichStaleObjectStateException(staleObjectStateException);
            }
            catch (Exception ex)
            {
                context.Exception = ex;
            }
            HandleException(context);
        }

        private static bool HasException(ActionExecutedContext context)
        {
            return context.Exception != null;
        }

        private static void HandleException(ActionExecutedContext context)
        {
            if (!HasException(context)) return;

            if (context?.Exception?.GetType() == typeof(NHibernate.StaleObjectStateException))
            {
                context.Exception = EnrichStaleObjectStateException(context.Exception as NHibernate.StaleObjectStateException);
            }

            ServiceLocator.Current.GetInstance<IApplicationHub>()?.RaiseApplicationErrorEvent(context.Exception);

            context.ExceptionHandled = true;
            LogManager.GetLogger(context.Controller.GetType()).Error(context.Exception);
            zAppDev.DotNet.Framework.Utilities.Common.SetLastError(context.Exception);
            context.Result = GetErrorResult(context, "{ErrorPageAction}", "{ErrorPageController}");
        }

        private static ActionResult GetErrorResult(ActionExecutedContext context, string actionName,
                string formName)
        {
            string errorContent;
            try
            {
                //context.HttpContext.Response.Clear();
                context.HttpContext.Response.StatusCode = 500;
                //context.HttpContext.Response.TrySkipIisCustomErrors = true;
                var friendlyMessageHandler = new zAppDev.DotNet.Framework.Utilities.ExceptionHandler();
                var msgObject = friendlyMessageHandler.HandleException(context.Exception);
                errorContent = Newtonsoft.Json.JsonConvert.SerializeObject(msgObject,
                               new Newtonsoft.Json.JsonSerializerSettings
                               {
                                   PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects
                               });
            }
            catch (Exception e)
            {
                LogManager
                    .GetLogger(typeof(zAppDev.DotNet.Framework.Utilities.ExceptionHandler))
                    .Error("Could not produce friendly message for exception!", e);
                errorContent = context.Exception.Message;
            }
            var controller = (CustomControllerBase)context.Controller;
            if (controller.Request.IsAjaxRequest())
            {
                return controller.Json(new
                {
                    Type = "Error",
                    Data = errorContent,
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
    }
}
#endif