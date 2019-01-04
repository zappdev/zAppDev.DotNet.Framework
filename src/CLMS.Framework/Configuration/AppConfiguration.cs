using System.Collections.Generic;

namespace CLMS.Framework.Configuration
{
    public class AppConfiguration
    {
        public Dictionary<string, ConnectionSettings> ConnectionStrings { get; set; }

        public Dictionary<string, string> AppSettings { get; set; }

        public class ConnectionSettings
        {
            public string Name { get; set; }

            public string ConnectionString { get; set; }

            public string ProviderName { get; set; }
        }
    }
}
