#if NETFRAMEWORK
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http.Controllers;
using WebApi.OutputCache.V2;

namespace CLMS.Framework.Services
{    
    public class CustomCacheKeyGenerator : DefaultCacheKeyGenerator
    {
        public bool CachePerUser { get; set; }
		public string ApiName { get; set; }

        private readonly ICacheKeyHasher _cacheKeyHasher = new CacheKeyHasher();

        public override string MakeCacheKey(HttpActionContext context, MediaTypeHeaderValue mediaType, bool excludeQueryString = false)
        {
            var fullName = context.ControllerContext.ControllerDescriptor.ControllerType.FullName;
            var actionName = context.ActionDescriptor.ActionName;
            var baseCachekey = context.Request.GetConfiguration().CacheOutputConfiguration().MakeBaseCachekey(fullName, actionName);

            var usernameCacheKey = "";
            if (CachePerUser)
            {
                usernameCacheKey = context.RequestContext.Principal.Identity.IsAuthenticated
                    ? context.RequestContext.Principal.Identity.Name
                    : "anon";

                usernameCacheKey = $"{usernameCacheKey}";
            }

            var queryStrings = context.ActionArguments.Where(x => x.Value != null).Select(x => x.Key + "=" + GetValue(x.Value));
            string argumentsString;
            if (!excludeQueryString)
            {
                var second = context.Request.GetQueryNameValuePairs().Where(x => x.Key.ToLower() != "callback").Select(x => x.Key + "=" + x.Value);
                argumentsString = "-" + string.Join("&", queryStrings.Union(second));
                var jsonpCallback = GetJsonpCallback(context.Request);
                if (!string.IsNullOrWhiteSpace(jsonpCallback))
                {
                    var callbackString = "callback=" + jsonpCallback;
                    if (argumentsString.Contains("&" + callbackString))
                        argumentsString = argumentsString.Replace("&" + callbackString, string.Empty);
                    if (argumentsString.Contains(callbackString + "&"))
                        argumentsString = argumentsString.Replace(callbackString + "&", string.Empty);
                    if (argumentsString.Contains("-" + callbackString))
                        argumentsString = argumentsString.Replace("-" + callbackString, string.Empty);
                    if (argumentsString.EndsWith("&"))
                        argumentsString = argumentsString.TrimEnd('&');
                }
            }
            else
            {
                argumentsString = string.Join("&", queryStrings);
            }

            var postBody = "";

            if (context.Request.Method == HttpMethod.Post ||
                context.Request.Method == HttpMethod.Delete ||
                context.Request.Method == HttpMethod.Put)
            {
                using (var stream = new MemoryStream())
                {
                    if (context.Request.Properties.ContainsKey("MS_HttpContext"))
                    {
                        var httpContext = (HttpContextBase)context.Request.Properties["MS_HttpContext"];
                        httpContext.Request.InputStream.Seek(0, SeekOrigin.Begin);
                        httpContext.Request.InputStream.CopyTo(stream);
                        postBody = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                    }
                }                
            }

            if (argumentsString == "-")
            {
                argumentsString = string.Empty;
            }

           _cacheKeyHasher.ApiName = this.ApiName;
           _cacheKeyHasher.Operation = actionName;
           _cacheKeyHasher.UserName = usernameCacheKey;
           _cacheKeyHasher.OriginalKey = $"{baseCachekey}|{usernameCacheKey}|{argumentsString}|{postBody}|{mediaType}";

            return _cacheKeyHasher.GetHashedKey();
        }


        private static string GetJsonpCallback(HttpRequestMessage request)
        {
            if (request.Method != HttpMethod.Get) return string.Empty;

            var jsonpCallback = string.Empty;
            var queryNameValuePairs = request.GetQueryNameValuePairs();

            if (queryNameValuePairs == null) return jsonpCallback;

            var keyValuePair = queryNameValuePairs.FirstOrDefault(x => x.Key.ToLower() == "callback");
            if (!keyValuePair.Equals(new KeyValuePair<string, string>()))
                jsonpCallback = keyValuePair.Value;

            return jsonpCallback;
        }

        private static string GetValue(object val)
        {
            if (!(val is IEnumerable) || val is string)
                return val.ToString();
            var empty = string.Empty;
            return ((IEnumerable) val).Cast<object>().Aggregate(empty, (current, paramValue) => current + paramValue + ";");
        }
    }
}
#endif