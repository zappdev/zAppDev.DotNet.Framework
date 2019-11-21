// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System.IO;
using System.Linq;
using System.Xml;
using zAppDev.DotNet.Framework.Utilities;
using Microsoft.Extensions.Configuration;

namespace zAppDev.DotNet.Framework.Configuration
{   
    public class ConfigurationHandler
    {

#if NETFRAMEWORK
        public static string GetAppSetting(string key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key];
        }
#else
        public static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddXmlFile("Web.config", true, true)
                .Add(new LegacyConfigurationProvider())
                .Build();
        }

        public static MailSettings GetSmtpSettings()
        {
            return GetInjectedConfig().GetSection("system.net:mailSettings")
                .Get<MailSettings>();
        }

        public static ImapConfiguration GetImapSettings()
        {
            var config = ServiceLocator.Current.GetInstance<IConfiguration>();

            var status = bool.TryParse(config?[$"configuration:appSettings:add:host:value"], out var enableSsl);
            enableSsl = status ? enableSsl : false;

            int.TryParse(config?[$"configuration:appSettings:add:host:value"], out var port);
            port = status ? port : 0;

            return new ImapConfiguration
            {
                Host = config?[$"configuration:appSettings:add:host:value"],
                Username = config?[$"configuration:appSettings:add:username:value"],
                Password = config?[$"configuration:appSettings:add:password:value"],
                EnableSsl = enableSsl,
                Port = port
            };
        }

        public static string GetAppSetting(string key)
        {
            var config = ServiceLocator.Current.GetInstance<IConfiguration>();
            return config?[$"configuration:appSettings:add:{key}:value"];
        }

        public static AppConfiguration GetAppConfiguration()
        {
            var config = ServiceLocator.Current.GetInstance<IConfiguration>();
            return new AppConfiguration(config);
        }

        public static ConnectionSettings GetDatabaseSetting(string key)
        {
            var config = ServiceLocator.Current.GetInstance<IConfiguration>();
            return new ConnectionSettings
            {
                ConnectionString = config?[$"configuration:connectionStrings:add:{key}:connectionString"],
                Name = config?[$"configuration:connectionStrings:add:{key}:name"],
                ProviderName = config?[$"configuration:connectionStrings:add:{key}:providerName"]
            };
        }

        public static ConfigurationBuilder SetUpConfigurationBuilder(ConfigurationBuilder config)
        {
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.Add(new LegacyConfigurationProvider());
            return config;
        }
        
        private static IConfiguration GetInjectedConfig()
        {
            var config = ServiceLocator.Current.GetInstance<IConfiguration>();
            return (config ?? GetConfiguration());
        }
#endif
    }

    public class LegacyConfigurationProvider : ConfigurationProvider, IConfigurationSource
    {
        public override void Load()
        {
            var doc = new XmlDocument();

            doc.Load("App.config");

            var selectNodes = doc.SelectNodes("//configuration/connectionStrings/add");
            if (selectNodes != null)
            {
                foreach (XmlNode connection in selectNodes)
                {
                    if (connection.Attributes == null) continue;

                    Data.Add($"ConnectionStrings:{connection.Attributes["name"].Value}:connectionString",
                        connection.Attributes["connectionString"].Value);
                    Data.Add($"ConnectionStrings:{connection.Attributes["name"].Value}:name",
                        connection.Attributes["name"].Value);
                    Data.Add($"ConnectionStrings:{connection.Attributes["name"].Value}:providerName",
                        connection.Attributes["providerName"].Value);
                }
            }

            var appSettNodeList = doc.SelectNodes("//configuration/appSettings/add");
            if (appSettNodeList == null) return;

            foreach (XmlNode appSett in appSettNodeList)
            {
                if (appSett.Attributes == null) continue;

                var key = appSett.Attributes["key"].Value;

                var prefix = key.Contains(":") ? "" : "AppSettings:";
                key = string.Join(":", key.Split(':').Select(Handle));
                Data.Add($"{prefix}{key}", appSett.Attributes["value"].Value);
            }
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return this;
        }

        private static string Handle(string c)
        {
            if (c.Equals("IMAP")) return "ImapConfiguration";
            return c;
        }
    }

}
