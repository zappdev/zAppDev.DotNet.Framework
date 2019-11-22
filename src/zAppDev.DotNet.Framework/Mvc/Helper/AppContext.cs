#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Http;

namespace zAppDev.DotNet.Framework.Mvc.Helper
{
    public static class AppContext
    {
        private static IHttpContextAccessor _httpContextAccessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static HttpContext Current => _httpContextAccessor.HttpContext;
    }
}
#endif