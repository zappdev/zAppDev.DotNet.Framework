using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace System.Web
{
    public static class HttpContext
    {
        private static IHttpContextAccessor _contextAccessor;

        public static string WebRootPath { get; private set; }
        public static string ContentRootPath { get; private set; }

        public static Microsoft.AspNetCore.Http.HttpContext Current => _contextAccessor.HttpContext;

        public static string MapPath(string filename)
        {
            filename = Regex.Replace(filename, @"^~{0,1}[\/\\]", "");

            return $"{ContentRootPath}\\{filename}";
        }

        internal static void Configure(IHttpContextAccessor contextAccessor, IHostingEnvironment hostingEnvironment)
        {
            WebRootPath = hostingEnvironment.WebRootPath;

            ContentRootPath = hostingEnvironment.ContentRootPath;
            _contextAccessor = contextAccessor;
        }
    }
}