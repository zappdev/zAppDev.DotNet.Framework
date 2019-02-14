#if NETFRAMEWORK
#else
using System.IO;
using Microsoft.Extensions.Configuration;

namespace CLMS.Framework.Extensions.WebConfig {
    public class WebConfigurationProvider : FileConfigurationProvider
    {
        public WebConfigurationProvider(WebConfigurationSource source) : base(source) { }

        public override void Load(Stream stream)
        {
            var parser = new WebConfigurationFileParser();
            
            Data = parser.Parse(stream);
        }
    }
}
#endif