#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Logging;
using zAppDev.DotNet.Framework.Mvc.API;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Middleware
{
    public class ApiResponseCacheMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IAPILogger _apiLogger;

        private readonly ILogger _logger;

        private readonly IApiCacheService _cache;

        private readonly IServiceProvider _serviceProvider;

        public ApiResponseCacheMiddleware(
            RequestDelegate next,
            IAPILogger apiLogger,
            ILogger<ApiResponseCacheMiddleware> logger,
            IApiCacheService apiCacheService,
            IServiceProvider serviceProvider)
        {
            _next = next;
            _apiLogger = apiLogger;
            _logger = logger;
            _cache = apiCacheService;
            _serviceProvider = serviceProvider;
        }

        public async Task Invoke(HttpContext context)
        {
            ServiceLocator.SetLocatorProvider(_serviceProvider);

            var response = context.Response;

            var originalStream = response.Body;

            using (var ms = new MemoryStream())
            {
                response.Body = ms;

                try
                {
                    await _next(context);

                    var bytes = ms.ToArray();

                    if (MustCached(context))
                    {
                        SetEtag(context, context.TraceIdentifier);
                        AddResponseToCache(context, bytes);
                    }
                }
                finally
                {
                    if (ms.Length > 0)
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        await ms.CopyToAsync(originalStream);
                    }

                    response.Body = originalStream;
                }
            }
        }

        private bool MustCached(HttpContext context) =>
            context.Response.StatusCode != 500 &&
            context.Items.ContainsKey(HttpContextItemKeys.HitCache) && 
            ((bool)context.Items[HttpContextItemKeys.HitCache]) == false;

        private void AddResponseToCache(HttpContext context, byte[] bytes)
        {
            var headers = new Dictionary<string, string>();

            foreach (string name in context.Response.Headers.Keys)
            {
                headers.Add(name, context.Response.Headers[name]);
            }
            var cacheOutput = new ApiCacheOutput(bytes, headers);
            _cache.Set(context, cacheOutput);
        }

        private static void SetEtag(HttpContext context, string etag)
        {
            if (etag != null)
            {
                var eTag = @"""" + etag.Replace("\"", string.Empty) + @"""";
                context.Response.Headers[HeaderNames.ETag] = eTag;
            }
        }
    }
}
#endif