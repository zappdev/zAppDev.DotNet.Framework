#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Http;

namespace zAppDev.DotNet.Framework.Mvc.API
{
    public interface IApiCacheKeyGenerator
    {
        string MakeCacheKey(HttpContext context, bool excludeQueryString = false);
    }
}

#endif