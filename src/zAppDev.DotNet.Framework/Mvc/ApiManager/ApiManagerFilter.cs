#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using zAppDev.DotNet.Framework.Data;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Mvc.API
{
    public class ApiManagerFilter : ActionFilterAttribute
    {
        public bool LogEnabled { get; set; }

        public bool AllowPartialResponse { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var action = (string)context.RouteData.Values["Action"];
            var controller = (string)context.RouteData.Values["Controller"];

            context.HttpContext.SetApiRequestConfig(LogEnabled, AllowPartialResponse, controller, action);

            var manager = ServiceLocator.Current.GetInstance<IMiniSessionService>();
            manager.OpenSessionWithTransaction();
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            var manager = ServiceLocator.Current.GetInstance<IMiniSessionService>();

            manager.CommitChanges(context.Exception);
        }
    }
}
#endif