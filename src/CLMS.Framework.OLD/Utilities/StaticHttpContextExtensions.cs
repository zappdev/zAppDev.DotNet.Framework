using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace CLMS.Framework
{
    public class HttpContextMiddleware
    {
        private readonly RequestDelegate _next;

        public HttpContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Do something with context near the beginning of request processing.
            System.Web.HttpContext.Configure(context);

            await _next.Invoke(context);

            // Clean up.
        }
    }

    public static class StaticHttpContextExtensions
    {
        public static IApplicationBuilder UseStaticHttpContext(this IApplicationBuilder app)
        {
            var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            var hostingEnvironment = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();

            System.Web.HttpContext.Configure(hostingEnvironment);

            return app.UseMiddleware<HttpContextMiddleware>();
        }
    }
}
