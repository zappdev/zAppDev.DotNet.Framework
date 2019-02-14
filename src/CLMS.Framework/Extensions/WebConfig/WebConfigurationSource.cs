#if NETFRAMEWORK
#else
using Microsoft.Extensions.Configuration;

namespace CLMS.Framework.Extensions.WebConfig {
    public class WebConfigurationSource : FileConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            FileProvider = FileProvider ?? builder.GetFileProvider();
            return new WebConfigurationProvider(this);
        }
    }
}
#endif