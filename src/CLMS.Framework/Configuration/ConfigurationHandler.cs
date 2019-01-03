using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace CLMS.Framework.Configuration
{
    public class AppConfig
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

    public class ConfigurationHandler
    {
#if NETFRAMEWORK
#else
        public static ConfigurationBuilder SetUpConfigurationBuilder(ConfigurationBuilder config)
        {
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.Add(new LegacyConfigurationProvider());
            return config;
        }        
#endif
    }

#if NETFRAMEWORK
#else
    public class LegacyConfigurationProvider : ConfigurationProvider, IConfigurationSource
    {
        public override void Load()
        {
            foreach (ConnectionStringSettings connectionString in ConfigurationManager.ConnectionStrings)
            {
                Data.Add($"ConnectionStrings:{connectionString.Name}:connectionString", connectionString.ConnectionString);
                Data.Add($"ConnectionStrings:{connectionString.Name}:name", connectionString.Name);
                Data.Add($"ConnectionStrings:{connectionString.Name}:providerName", connectionString.ProviderName);
            }

            foreach (var settingKey in ConfigurationManager.AppSettings.AllKeys)
            {
                Data.Add($"AppSettings:{settingKey}", ConfigurationManager.AppSettings[settingKey]);
            }
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return this;
        }
    }
#endif
}
