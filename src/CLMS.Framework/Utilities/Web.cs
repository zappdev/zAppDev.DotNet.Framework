using System;
using System.Collections.Generic;
using System.Linq;
using AppDevCache = CLMS.AppDev.Cache;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Web;

#if NETFRAMEWORK
using System.Net.Http;
#else
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
#endif

namespace CLMS.Framework.Utilities
{
    public class Web
    {
        public static Uri GetRequestUri()
        {
#if NETFRAMEWORK
            return HttpContext.Current.Request.Url;
#else
            var contextAccessor = ServiceLocator.Current.GetInstance<IHttpContextAccessor>();
            var req = contextAccessor.HttpContext.Request;
            return new Uri($"{req.Scheme}://{req.Host}{req.Path}{req.QueryString}"); ;
#endif
        }

        public static HttpContext GetContext()
        {
#if NETFRAMEWORK
            return HttpContext.Current;
#else
            var contextAccessor = ServiceLocator.Current.GetInstance<IHttpContextAccessor>();
            return contextAccessor.HttpContext;
#endif
        }

        public static string GetClientIp()
        {
#if NETFRAMEWORK
            var ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            ip =
 string.IsNullOrEmpty(ip) == false ? ip.Split(',')[0].Trim() : HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            return ip;
#else
            var contextAccessor = ServiceLocator.Current.GetInstance<IHttpContextAccessor>();
            return contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
#endif
        }

        public static string GetBrowserType()
        {
#if NETFRAMEWORK
            return HttpContext.Current.Request.Browser.Type;
#else
            return GetContext().Request.Headers["User-Agent"].ToString();
#endif
        }

        public static bool IsUserInRole(string roleName)
        {
            return GetContext().User.IsInRole(roleName);
        }

        public static bool IsInControllerAction(string action)
        {
            return GetFormArgument("_currentControllerAction") == action;
        }

        public static string GetQuery()
        {
#if NETFRAMEWORK
            var query = "";
            var routeData = "";

            //Good for links such as: ~/GoTo?aNumber=5&astring=xaxa
            if (HttpContext.Current.Request.QueryString?.HasKeys() == true)
            {
                var result = new List<string>();
                foreach (var key in HttpContext.Current.Request.QueryString.AllKeys)
                {
                    result.Add($@"{key}={HttpContext.Current.Request.QueryString[key]}");
                }
                query = string.Join("&", result);
                if (!string.IsNullOrWhiteSpace(query)) query = $"?{query}";
            }

            //Good for links such as:  ~/GoTo/5/xaxa
            if (HttpContext.Current.Request.RequestContext.RouteData?.Values?.Count > 0)
            {
                var foundControllerKey = false;
                var foundActionKey = false;
                var result = new List<string>();

                foreach (var key in HttpContext.Current.Request.RequestContext.RouteData.Values.Keys)
                {
                    /*
                        Usually, the first keys are 'controller' and 'action'. However, I don't wanna ignore them COMPLETELY. Just the first ones.
                        'Cause maybe a controller action has an 'action' parameter. Don't wanna lose that.
                    */
                    if ((!foundControllerKey) && (string.Compare(key, "controller", StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        foundControllerKey = true;
                        continue;
                    }

                    if ((!foundActionKey) && (string.Compare(key, "action", StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        foundActionKey = true;
                        continue;
                    }

                    result.Add($@"{HttpContext.Current.Request.RequestContext.RouteData.Values[key]}");
                }
                routeData = string.Join("/", result);
                if (!string.IsNullOrWhiteSpace(routeData)) routeData = $"/{routeData}";
            }

            return routeData + query;
#else
            var query = "";
            var routeData = "";
            var context = GetContext();
            
            //Good for links such as: ~/GoTo?aNumber=5&astring=xaxa
            if (context.Request.QueryString.HasValue)
            {
                var result = context.Request.Query
                    .Keys.Select(key => $@"{key}={context.Request.Query[key]}").ToList();
                query = string.Join("&", result);
                if (!string.IsNullOrWhiteSpace(query)) query = $"?{query}";
            }
            
            return routeData + query;
#endif
        }

        public static string GetFormArgument(string argname)
        {
#if NETFRAMEWORK
            //Good for links such as: ~/GoTo?aNumber=5&astring=xaxa
            if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString[argname]))
            {
                return HttpContext.Current.Request.QueryString[argname];
            }

            //Good for links such as:  ~/GoTo/5/xaxa
            var result = HttpContext.Current.Request.RequestContext.RouteData?.Values[argname];
            if (!string.IsNullOrEmpty(result?.ToString()))
            {
                return result.ToString();
            }

            if (HttpContext.Current.Items[argname] != null)
            {
                return HttpContext.Current.Items[argname].ToString();
            }

            if (argname == "returnUrl")
            {
                var returnUrl = GetReturnUrl();

                if (returnUrl != null)
                {
                    return returnUrl.TrimStart('~');
                }
            }

            return string.Empty;
#else
            var context = GetContext();
            // Good for links such as: ~/GoTo?aNumber=5&astring=xaxa
            if (!string.IsNullOrEmpty(context.Request.Query[argname]))
            {
                return context.Request.Query[argname];
            }

            // Good for links such as:  ~/GoTo/5/xaxa
            // Not supported by .NET Core

            if (context.Items[argname] != null)
            {
                return context.Items[argname].ToString();
            }

            if (argname != "returnUrl") return string.Empty;
            
            var returnUrl = GetReturnUrl();
            return returnUrl != null ? returnUrl.TrimStart('~') : string.Empty;

#endif
        }

        public static string GetRequestHeader(string header)
        {
            return GetContext()?.Request?.Headers[header];
        }

        public static string GetReturnUrl()
        {
#if NETFRAMEWORK
            if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["returnUrl"]))
            {
                return HttpContext.Current.Request.QueryString["returnUrl"];
            }

            if (!string.IsNullOrEmpty(HttpContext.Current.Request.UrlReferrer?.Query))
            {
                var query = HttpContext.Current.Request.UrlReferrer.Query;

                return HttpUtility.ParseQueryString(query)["returnUrl"];
            }
            return string.Empty;
#else
            if (!string.IsNullOrEmpty(GetContext().Request.Query["returnUrl"]))
            {
                return GetContext().Request.Query["returnUrl"];
            }

            if (string.IsNullOrEmpty(GetContext().Request.Headers["Referer"].ToString())) 
                return string.Empty;
            
            var query = GetContext().Request.Headers["Referer"].ToString();;
            return HttpUtility.ParseQueryString(query)["returnUrl"];
#endif
        }

        public static bool IsUser(string username)
        {
            return GetContext().User.Identity.Name == username;

        }

        // Serialization Sanitization entries
        // This replaces weird character sequences inside serialized objects
        // that cause asp.net mvc to crash when NormalizeJQueryToMvc internal method is invoked
        public static Dictionary<string, string> SerializationSanitizationEntries = new Dictionary<string, string>()
        {
            {"=", ">>MVC_1<<"}
        };

        private static ServerRole _serverRole;

        public static ServerRole CurrentServerRole =>
            _serverRole == ServerRole.None ? GetCurrentServerRole() : _serverRole;

        private static ServerRole GetCurrentServerRole()
        {
            try
            {
                var role = ConfigurationManager.AppSettings["ServerRole"];

                _serverRole = string.IsNullOrWhiteSpace(role)
                    ? ServerRole.Combined
                    : (ServerRole) Enum.Parse(typeof(ServerRole), role);
            }
            catch
            {
                _serverRole = ServerRole.Combined;
            }

            return _serverRole;
        }

        public static string MapPath(string path)
        {
#if NETFRAMEWORK
            try
            {
                return System.Web.HttpContext.Current.Server.MapPath(path);
            }
            catch
            {
                return System.Web.Hosting.HostingEnvironment.MapPath(path);
            }
#else
            var hosting = ServiceLocator.Current.GetInstance<IHostingEnvironment>();
            return Path.Combine(hosting.ContentRootPath, path.Replace("~/", ""));
#endif
        }

        public class Session
        {
            public static AppDevCache.ICache<string> GetCache()
            {
#if NETFRAMEWORK
                return AppDevCache.CacheManager.Current;
#else
                return ServiceLocator.Current.GetInstance<AppDevCache.ICache<string>>();
#endif
            }

            public static Dictionary<string, object> GetStorage()
            {
                var sessionId = GetSessionId();
                var items = GetCache().KeyValuePair(item => item.Key.StartsWith(sessionId));
                return items.ToDictionary(item => item.Key.Replace(sessionId, ""), item => item.Value);
            }

            private static string GetSessionId()
            {
#if NETFRAMEWORK
                var sessionId = HttpContext.Current.Session?.SessionID;
                if (!string.IsNullOrWhiteSpace(sessionId)) return sessionId;

                var httpRequestMessage = HttpContext.Current.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
                if (httpRequestMessage == null)
                {
                    throw new ApplicationException("Could not find MS_HttpRequestMessage in HttpContext!");
                }

                return httpRequestMessage.GetCorrelationId().ToString();
#else
                var sessionId = GetContext().Session?.Id;
                if (!string.IsNullOrWhiteSpace(sessionId)) return sessionId;

                throw new ApplicationException("Session Id is empty");
#endif
            }

            public static object Get(string key)
            {
                key = $"{GetSessionId()}{key}";
                return GetCache().Get<object>(key);
            }

            public static void Set(string key, object value)
            {
                key = $"{GetSessionId()}{key}";
                GetCache().Set(key, value);
            }

            public static void Add(string key, object value)
            {
                key = $"{GetSessionId()}{key}";
                GetCache().Add(key, value);
            }

            public static void Remove(string key)
            {
                key = $"{GetSessionId()}{key}";
                GetCache().Remove<object>(key);
            }
        }

        public enum ServerRole
        {
            None,
            Combined,
            Application,
            Web
        }
    }
}