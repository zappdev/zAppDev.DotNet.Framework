#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace zAppDev.DotNet.Framework.Mvc.API
{
    public static class ApiManagerFeatureExtensions
    {
        public static void SetApiRequestConfig(this HttpContext context, ApiManagerConfig config)
        {
            context.SetApiRequestConfig(config.LogEnabled, config.AllowPartialResponse, config.Controller, config.Action);
        }

        public static void SetApiRequestConfig(this HttpContext context, bool logEnabled, bool allowPartialResponse, string controller, string action)
        {
            var feature = context.Features.Get<ApiManagerConfig>();

            if (feature == null)
            {
                feature = new ApiManagerConfig();
                context.Features.Set(feature);
            }

            feature.AllowPartialResponse = allowPartialResponse;
            feature.LogEnabled = logEnabled;
            feature.Controller = controller;
            feature.Action = action;
        }

        internal static string GetController(this HttpContext context)
        {
            return context.GetController(out var _);
        }

        internal static string GetController(this HttpContext context, out ApiManagerConfig feature)
        {
            feature = context.Features.Get<ApiManagerConfig>();
            return feature?.Controller ?? "";
        }

        internal static string GetAction(this HttpContext context)
        {
            return context.GetAction(out var _);
        }

        internal static string GetAction(this HttpContext context, out ApiManagerConfig feature)
        {
            feature = context.Features.Get<ApiManagerConfig>();
            return feature?.Action ?? "";
        }

        internal static bool IsLogEnabled(this HttpContext context)
        {
            return context.IsLogEnabled(out var _);
        }

        internal static bool IsLogEnabled(this HttpContext context, out ApiManagerConfig feature)
        {
            feature = context.Features.Get<ApiManagerConfig>();
            return feature != null && feature.LogEnabled;
        }

        internal static bool IsAllowPartialResponseEnabled(this HttpContext context)
        {
            return context.IsAllowPartialResponseEnabled(out var _);
        }

        internal static bool IsAllowPartialResponseEnabled(this HttpContext context, out ApiManagerConfig feature)
        {
            feature = context.Features.Get<ApiManagerConfig>();
            return feature != null && feature.AllowPartialResponse;
        }
    }

    public static class HttpContextExtensions
    {
        private static readonly RouteData EmptyRouteData = new RouteData();

        private static readonly ActionDescriptor EmptyActionDescriptor = new ActionDescriptor();

        public static Task WriteModelAsync<TModel>(this HttpContext context, TModel model)
        {
            var result = new ObjectResult(model)
            {
                DeclaredType = typeof(TModel)
            };

            return context.ExecuteResultAsync(result);
        }

        public static Task ExecuteResultAsync<TResult>(this HttpContext context, TResult result)
            where TResult : IActionResult
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (result == null) throw new ArgumentNullException(nameof(result));

            var executor = context.RequestServices.GetRequiredService<IActionResultExecutor<TResult>>();

            var routeData = context.GetRouteData() ?? EmptyRouteData;
            var actionContext = new ActionContext(context, routeData, EmptyActionDescriptor);

            return executor.ExecuteAsync(actionContext, result);
        }
    }
}
#endif