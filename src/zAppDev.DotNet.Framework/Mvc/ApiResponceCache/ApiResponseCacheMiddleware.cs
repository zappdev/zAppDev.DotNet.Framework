#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
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

                    if (context.Items.ContainsKey(ApiManagerItemsKeys.HitCache) && ((bool)context.Items[ApiManagerItemsKeys.HitCache]) == false)
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

        private void AddResponseToCache(HttpContext context, byte[] bytes)
        {
            _cache.Set(context, new ApiCacheOutput(bytes, context.Response.Headers));
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