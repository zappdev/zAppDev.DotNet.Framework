#if NETFRAMEWORK
#else
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace zAppDev.DotNet.Framework.Extensions.WebConfig 
{
    public static class WebConfigurationExtensions
    {
        public static IConfigurationBuilder AddWebConfigFile(this IConfigurationBuilder builder, string path)
        {
            return AddWebConfigFile(builder, provider: null, path: path, optional: false, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddWebConfigFile(this IConfigurationBuilder builder, string path, bool optional)
        {
            return AddWebConfigFile(builder, provider: null, path: path, optional: optional, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddWebConfigFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            return AddWebConfigFile(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);
        }

        public static IConfigurationBuilder AddWebConfigFile(this IConfigurationBuilder builder, List<string> paths, bool optional, bool reloadOnChange)
        {
            foreach(var path in paths)
                AddWebConfigFile(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);
            return builder;
        }

        private static IConfigurationBuilder AddWebConfigFile(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional, bool reloadOnChange)
        {
            if (provider == null && Path.IsPathRooted(path))
            {
                provider = new PhysicalFileProvider(Path.GetDirectoryName(path));
                path = Path.GetFileName(path);
            }
            var source = new WebConfigurationSource
            {
                FileProvider = provider,
                Path = path,
                Optional = optional,
                ReloadOnChange = reloadOnChange
            };
            builder.Add(source);
            return builder;
        }
    }
}
#endif