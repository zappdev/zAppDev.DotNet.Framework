﻿// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using AppDevCache = CLMS.AppDev.Cache;
using System.Configuration;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
#if NETFRAMEWORK
using System.Net.Http;
#else
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
#endif

namespace zAppDev.DotNet.Framework.Utilities
{

#if NETFRAMEWORK

#else
    public static class HttpRequestExtensions
    {
        public static string RawUrl(this HttpRequest request)
        {
            return Microsoft.AspNetCore.Http.Extensions.UriHelper.GetEncodedUrl(request);
        }

        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.Headers != null)
                return request.Headers["X-Requested-With"] == "XMLHttpRequest";
            return false;
        }

        public static void RedirectToAction(this HttpResponse response, string controller, string action)
        {
            var helper = ServiceLocator.Current.GetInstance<IUrlHelper>();
            var path = helper.Action(action, controller);
            response.Redirect(path);
        }

        public static void RedirectToAction(this HttpResponse response, string controller)
        {
            response.RedirectToAction(controller, "");
        }
    }
#endif

    public class Web
    {
        public static Uri GetRequestUri()
        {
#if NETFRAMEWORK
            return HttpContext.Current.Request.Url;
#else
            ////var contextAccessor = ServiceLocator.Current.GetInstance<IHttpContextAccessor>();
            ////var req = contextAccessor.HttpContext.Request;
            var req = Mvc.Helper.AppContext.Current.Request;
            return new Uri($"{req.Scheme}://{req.Host}{req.Path}{req.QueryString}"); ;
#endif
        }

        public static HttpContext GetContext()
        {
#if NETFRAMEWORK
            return HttpContext.Current;
#else
            //var contextAccessor = ServiceLocator.Current.GetInstance<IHttpContextAccessor>();
            //return contextAccessor.HttpContext;

            return Mvc.Helper.AppContext.Current;
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
            //var contextAccessor = ServiceLocator.Current.GetInstance<IHttpContextAccessor>();
            //return contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            return Mvc.Helper.AppContext.Current.Connection.RemoteIpAddress.ToString();
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

        public static Dictionary<string, string> GetRouteData()
        {
            var result = new Dictionary<string, string>();
#if NETFRAMEWORK

				var keys = GetContext()?.Request?.RequestContext?.RouteData?.Values.Keys;
				if(keys != null)
				{
					foreach (var key in keys){
						var value = GetContext()?.Request?.RequestContext?.RouteData?.Values[key];
						result.Add(key, value?.ToString());
					}
				}
#else

            var routedData = GetContext()?.GetRouteData();
            if (routedData == null) return result;

            foreach (var key in routedData.Values.Keys)
            {
                var value = routedData.Values[key];
                result.Add(key, value?.ToString());
            }
#endif
            return result;
        }

        public static string GetApplicationPathUri(bool isInNTierArchitecture = false)
        {
#if NETFRAMEWORK
            var baseUrl = $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Authority}";
            var applicationPath = $"{baseUrl}/{HttpContext.Current.Request.ApplicationPath}";
#else
            var context = GetContext();
            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}/";

            var applicationPath =  $"{baseUrl}/{context.Request.PathBase}";
#endif

            if (!isInNTierArchitecture)
            {
                return applicationPath;
            }

            var pattern = @"[_\-]BackEnd";
            var regex = new Regex(pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            return regex.Replace(applicationPath, "");
        }

        public static string GetApplicationPath(bool isInNTierArchitecture = false)
        {
#if NETFRAMEWORK
            string applicationPath = GetContext().Request.ApplicationPath;
#else
            var applicationPath = $"{GetContext().Request.PathBase}";
#endif

            if (!isInNTierArchitecture)
            {
                return applicationPath;
            }

            var pattern = @"[_\-]BackEnd";
            var regex = new Regex(pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            return regex.Replace(applicationPath, "");
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

            var routedData = GetContext().GetRouteData();
            if (routedData != null)
            {
                var foundControllerKey = false;
                var foundActionKey = false;

                var result = new List<string>();

                foreach (var key in routedData.Values.Keys)
                {
                    var value = $"{routedData.Values[key]}";

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

                    result.Add(value);
                }

                routeData = string.Join("/", result);
                if (!string.IsNullOrWhiteSpace(routeData)) routeData = $"/{routeData}";
            }

            return routeData + query;
#endif
        }

        public static string GetFormArgument(string argName)
        {
#if NETFRAMEWORK
            //Good for links such as: ~/GoTo?aNumber=5&astring=xaxa
            if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString[argName]))
            {
                return HttpContext.Current.Request.QueryString[argName];
            }

            //Good for links such as:  ~/GoTo/5/xaxa
            var result = HttpContext.Current.Request.RequestContext.RouteData?.Values[argName];
            if (!string.IsNullOrEmpty(result?.ToString()))
            {
                return result.ToString();
            }

            if (HttpContext.Current.Items[argName] != null)
            {
                return HttpContext.Current.Items[argName].ToString();
            }

            if (argName == "returnUrl")
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
            if (!string.IsNullOrEmpty(context.Request.Query[argName]))
            {
                return context.Request.Query[argName];
            }

            // Good for links such as:  ~/GoTo/5/xaxa
            // Not supported by .NET Core

            if (context.Items[argName] != null)
            {
                return context.Items[argName].ToString();
            }

            if (argName != "returnUrl") return string.Empty;
            
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

        public static ServerRole GetCurrentServerRole(IConfiguration configuration = null)
        {
            try
            {
#if NETFRAMEWORK
                var role = ConfigurationManager.AppSettings["ServerRole"];
#else
                var config = configuration ?? ServiceLocator.Current.GetInstance<IConfiguration>();
                var role = config["configuration:appSettings:add:ServerRole:value"];
#endif
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
#if NETFRAMEWORK
            public static AppDevCache.ICache<string> GetCache()
            {

                return AppDevCache.CacheManager.Current;
            }
#else
            public static CacheManager.Core.ICacheManager<object> GetCache()
            {
                return ServiceLocator.Current.GetInstance<CacheManager.Core.ICacheManager<object>>("SessionStateStorage");
            }
#endif

            public static Dictionary<string, object> GetStorage()
            {
                var sessionId = GetSessionId();
#if NETFRAMEWORK
                var items = GetCache().KeyValuePair(item => item.Key.StartsWith(sessionId));
                return items.ToDictionary(item => item.Key.Replace(sessionId, ""), item => item.Value);
#else
                throw new NotImplementedException("Web.GetStorage isn't implemented on .NET Core");
#endif

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
                var sessionId = GetContext().Session?.Id?.ToString();
                if (!string.IsNullOrWhiteSpace(sessionId)) return sessionId;

                throw new ApplicationException("Session Id is empty");
#endif
            }

            public static T Get<T>(string key, T fallbackItem = default(T))
            {
                key = $"{GetSessionId()}{key}";
                var cache = GetCache();
                var value = cache.Get<T>(key);

                return (value != null) ? value : fallbackItem;
            }

            public static object Get(string key, object fallbackItem = null)
            {
                return Get<object>(key, fallbackItem);
            }

            public static void Set(string key, object value)
            {
                key = $"{GetSessionId()}{key}";
#if NETFRAMEWORK
                GetCache().Set(key, value);
#else
                GetCache().Add(key, value);
#endif
            }

            public static void Add(string key, object value)
            {
                key = $"{GetSessionId()}{key}";
                var cache = GetCache();
                cache.Add(key, value);
            }

            public static void Remove(string key)
            {
                key = $"{GetSessionId()}{key}";
#if NETFRAMEWORK
                GetCache().Remove<object>(key);
#else
                GetCache().Remove(key);
#endif
            }

            public static bool HasKey(string key)
            {
#if NETFRAMEWORK
                return GetCache().HasKey($"{GetSessionId()}{key}");
#else
                return GetCache().Exists($"{GetSessionId()}{key}");
#endif
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