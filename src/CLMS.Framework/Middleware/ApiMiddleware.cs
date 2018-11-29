using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CLMS.Framework.Middleware
{
    public class ApiMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, RequestDelegate next)
        {
            await _next(context);
        }
    }
}
