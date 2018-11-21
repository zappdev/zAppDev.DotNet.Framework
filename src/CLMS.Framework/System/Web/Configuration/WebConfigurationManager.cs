using Microsoft.Extensions.Configuration;
using System.IO;

namespace System.Web.Configuration
{
    public class WebConfigurationManager
    {
        internal static IConfiguration OpenWebConfiguration(string applicationPath)
        {
            var builder = new ConfigurationBuilder()
                .AddXmlFile(Path.Join(applicationPath, "webapp.config"));

            return builder.Build();
        }
    }
}
