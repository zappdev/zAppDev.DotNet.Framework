#if NETFRAMEWORK
#else
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace CLMS.Framework.Extensions.WebConfig 
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