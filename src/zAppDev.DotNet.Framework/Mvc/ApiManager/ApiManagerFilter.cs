// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
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

            // var manager = ServiceLocator.Current.GetInstance<IMiniSessionService>();
            var manager = context.HttpContext.RequestServices.GetRequiredService<IMiniSessionService>();
            manager.OpenSessionWithTransaction();
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            // var manager = ServiceLocator.Current.GetInstance<IMiniSessionService>();
            var manager = context.HttpContext.RequestServices.GetRequiredService<IMiniSessionService>();

            manager.CommitChanges(context.Exception);
        }
    }
}
#endif