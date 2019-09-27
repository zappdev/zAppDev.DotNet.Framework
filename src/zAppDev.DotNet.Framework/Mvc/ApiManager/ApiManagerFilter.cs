#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Mvc.Filters;

namespace zAppDev.DotNet.Framework.Mvc.API
{
    public class ApiManagerFilter : ActionFilterAttribute
    {
        public bool LogEnabled { get; set; }

        public bool AllowPartialResponse { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var action = (string)context.RouteData.Values["Action"];
            var controller = (string)context.RouteData.Values["Controller"];

            context.HttpContext.SetApiRequestConfig(LogEnabled, AllowPartialResponse, controller, action);
            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
             var id = context.HttpContext.Items["exposed-service-requestId"];
        }
    }
}
#endif