#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace zAppDev.DotNet.Framework.Mvc.API
{
    public class ApiCacheKeyGenerator : IApiCacheKeyGenerator
    {
        private static readonly char Delimeter = '|';

        private static readonly string CallbackKey = "callback";

        private static readonly string AcceptType = "Accept";

        private const string AnonymusUserName = "anon";

        private readonly ICacheKeyHasher _cacheKeyHasher;

        public ApiCacheKeyGenerator(ICacheKeyHasher cacheKeyHasher)
        {
            _cacheKeyHasher = cacheKeyHasher;
        }

        public string MakeCacheKey(HttpContext context, bool excludeQueryString = false)
        {
            context.IsOutputCachingEnabled(out var config);

            var keyParts = new List<string>
            {
                context.Request.IsHttps ? "https" : "http",
                context.Request.Host.ToString(),
                context.Request.PathBase,
                GetCachePerUserKey(context, config),
                GetQueryStringKey(context, config),
                GetBodyKey(context, config),
                context.Request.Headers[AcceptType]
            };

            var originalKey = string.Join(Delimeter.ToString(), keyParts.Where(x => !string.IsNullOrEmpty(x)));

            return _cacheKeyHasher.GetHashedKey(config.ApiName, config.ActionName, originalKey, "");
        }

        public string MakeBaseCachekey(string host, string action)
            => $"{host.ToLower()}{Delimeter}{action.ToLower()}";

        private string GetCachePerUserKey(HttpContext context, ApiResponseCacheConfig config)
        {
            if (config.CachePerUser)
            {
                return context.User.Identity.IsAuthenticated
                                    ? context.User.Identity.Name
                                    : AnonymusUserName;
            }
            else
            {
                return "";
            }
        }

        private string GetBodyKey(HttpContext context, ApiResponseCacheConfig config)
        {
            var postBody = "";

            if (context.Request.Method == HttpMethod.Post.Method ||
                context.Request.Method == HttpMethod.Delete.Method ||
                context.Request.Method == HttpMethod.Put.Method)
            {

                context.Request.EnableRewind();

                 using (var stream = new StreamReader(context.Request.Body,encoding: Encoding.UTF8,detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
                 {
                     postBody = stream.ReadToEnd();
                     context.Request.Body.Seek(0, SeekOrigin.Begin);
                 }
            }

            return postBody;
        }

        private string GetQueryStringKey(HttpContext context, ApiResponseCacheConfig config)
        {
            var pathStrings = ToList(context.GetRouteData());

            if (config.ExcludeQueryStringFromCacheKey)
            {
                return string.Join("&", pathStrings);
            }
            else 
            {
                var args = context.Request.Query
                    .Where(q => !q.Key.Equals(CallbackKey))
                    .Select(q => $"{q.Key}={q.Value}");

                var argumentsString = string.Join("&", pathStrings.Union(args));

                var jsonpCallback = GetJsonpCallback(context.Request);
                if (!string.IsNullOrWhiteSpace(jsonpCallback))
                    argumentsString = RemoveJsopCallback(argumentsString, jsonpCallback);

                return argumentsString;
            }
        }

        private static string RemoveJsopCallback(string argumentsString, string jsonpCallback)
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
            return argumentsString;
        }

        private IEnumerable<string> ToList(RouteData routeData)
        {
            if (routeData == null) return Enumerable.Empty<string>();

            return routeData.Values.Select(q => $"{q.Key}={q.Value}");
        }

        private string GetJsonpCallback(HttpRequest request)
        {
            if (request.Method != HttpMethod.Get.Method) return string.Empty;

            var jsonpCallback = string.Empty;
            var queryParams = request.Query;

            if (queryParams.Count == 0) return jsonpCallback;

            var hasJsonpCallback = queryParams.ContainsKey(CallbackKey) && !string.IsNullOrEmpty(queryParams[CallbackKey]);
            if (!hasJsonpCallback)
                jsonpCallback = queryParams[CallbackKey];

            return jsonpCallback;
        }
    }
}
#endif