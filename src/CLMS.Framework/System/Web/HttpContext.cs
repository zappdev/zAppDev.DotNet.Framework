using Microsoft.AspNetCore.Hosting;
using System.Text.RegularExpressions;

namespace System.Web
{
    public static class HttpContext
    {
        public static string WebRootPath { get; private set; }
        public static string ContentRootPath { get; private set; }

        public static Microsoft.AspNetCore.Http.HttpContext Current { get; private set; }

        public static string MapPath(string filename)
        {
            filename = Regex.Replace(filename, @"^~{0,1}[\/\\]", "");

            return $"{ContentRootPath}\\{filename}";
        }

        internal static void Configure(Microsoft.AspNetCore.Http.HttpContext contextAccessor)
        {
            Current = contextAccessor;
        }

        internal static void Configure(IHostingEnvironment hostingEnvironment)
        {
            WebRootPath = hostingEnvironment.WebRootPath;
            ContentRootPath = hostingEnvironment.ContentRootPath;
        }
    }
}