using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CLMS.Framework.Middleware
{
    public static class ApiMiddlewareExtensions
    {
        public static void AddApiMiddleware(this IServiceCollection services)
        {

        }

        public static IApplicationBuilder UseApiMiddleware(
            this IApplicationBuilder builder)
        {
            builder.MapWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder => {
                appBuilder.UseMiddleware<ApiMiddleware>();
            });


            return builder;
        }
    }
}
