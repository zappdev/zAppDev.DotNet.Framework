// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using zAppDev.DotNet.Framework.Data;
using zAppDev.DotNet.Framework.Utilities;
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Components;
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Configuration;


namespace zAppDev.DotNet.Framework.Mvc.API
{
    public class ApiManagerFilter : ActionFilterAttribute
    {
        public ActionMonitor ExposedAPIMonitor;
        protected ExposedAPIConfiguration ExposedAPIMonitorConfiguration => ServiceLocator.Current.GetInstance<PerformanceMonitorConfiguration>().ExposedAPI;


        public bool LogEnabled { get; set; }

        public bool AllowPartialResponse { get; set; }

        public bool ReadOnly { get; set; }


        public override void OnActionExecuting(ActionExecutingContext context)
        {

            base.OnActionExecuting(context);

            var action = (string)context.RouteData.Values["Action"];
            var controller = (string)context.RouteData.Values["Controller"];

            context.HttpContext.SetApiRequestConfig(LogEnabled, AllowPartialResponse, controller, action);

            // var manager = ServiceLocator.Current.GetInstance<IMiniSessionService>();
            var manager = context.HttpContext.RequestServices.GetRequiredService<IMiniSessionService>();


            if (ReadOnly)
            {
                manager.OpenSession();
            }
            else
            {
                manager.OpenSessionWithTransaction();
            }
            ExposedAPIMonitor = new ActionMonitor(ExposedAPIMonitorConfiguration, manager.Session, null);
            ExposedAPIMonitor?.Start();


        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            // var manager = ServiceLocator.Current.GetInstance<IMiniSessionService>();
            var manager = context.HttpContext.RequestServices.GetRequiredService<IMiniSessionService>();

            var action = (string)context.RouteData.Values["Action"];
            var controller = (string)context.RouteData.Values["Controller"];

            if (ReadOnly)
            {
                manager.CloseSession();
            }
            else
            {
                manager.CommitChanges(context.Exception);
            }

            ExposedAPIMonitor?.Stop();
            ExposedAPIMonitor?.LogExposedAPIStatistics(controller, action);
        }
    }
}
#endif