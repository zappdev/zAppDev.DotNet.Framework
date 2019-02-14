#if NETFRAMEWORK
#else
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Configuration.Extensions.WebConfig {
    public class WebConfigurationProvider : FileConfigurationProvider
    {
        public WebConfigurationProvider(WebConfigurationSource source) : base(source) { }

        public override async void Load(Stream stream)
        {
            var parser = new WebConfigurationFileParser();
            
            Data = await parser.Parse(stream);
        }
    }
}
#endif