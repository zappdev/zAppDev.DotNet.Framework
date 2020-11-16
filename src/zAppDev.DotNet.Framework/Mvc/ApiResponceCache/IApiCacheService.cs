#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Http;

namespace zAppDev.DotNet.Framework.Mvc.API
{
    public interface IApiCacheService
    {
        bool TryGetValue(HttpContext context, out ApiCacheOutput response);

        void Set(HttpContext context, ApiCacheOutput response);

        void Clear();

        void InvalidateApiCache(string api, string username = null);

        void InvalidateOperationCache(string api, string operation, string username = null);
    }
}
#endif