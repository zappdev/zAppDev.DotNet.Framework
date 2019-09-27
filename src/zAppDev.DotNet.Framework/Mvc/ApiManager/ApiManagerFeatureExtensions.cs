#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Http;

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
}
#endif