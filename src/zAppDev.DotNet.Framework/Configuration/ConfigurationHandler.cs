// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System.IO;
using System.Linq;
using System.Xml;
using zAppDev.DotNet.Framework.Utilities;
using Microsoft.Extensions.Configuration;
using System;
using static zAppDev.DotNet.Framework.Utilities.Web;

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

        public static MailSettings GetSmtpSettings(IConfiguration configuration = null)
        {
            var config = configuration == null ? ServiceLocator.Current.GetInstance<IConfiguration>() : configuration;

            int.TryParse(config?[$"configuration:system.net:mailSettings:smtp:network:port"], out var port);

            return new MailSettings
            {
                Smtp = new SmtpSettings
                {
                    From = config?[$"configuration:system.net:mailSettings:smtp:from"],
                    Network = new SmtpNetworkSettings
                    {
                        Host = config?[$"configuration:system.net:mailSettings:smtp:network:host"],
                        Password = config?[$"configuration:system.net:mailSettings:smtp:network:password"],
                        Port = port,
                        UserName = config?[$"configuration:system.net:mailSettings:smtp:network:userName"]
                    }
                }
            };
        }

        public static Uri GetOtherTierURL(IConfiguration configuration = null, ServerRole role = ServerRole.None)
        {
            var config = configuration == null ? ServiceLocator.Current.GetInstance<IConfiguration>() : configuration;

            var serverRole = role == ServerRole.None ? Web.GetCurrentServerRole(configuration) : role;

            var appSetting = "";

            switch (serverRole)
            {
                case Web.ServerRole.Application:
                    appSetting = "WebServerUrl";
                    break;
                case Web.ServerRole.Web:
                    appSetting = "AppServerUrl";
                    break;
                default:
                    return null;
            }

            var baseUri = config[$"configuration:appSettings:add:{appSetting}:value"];
            if (!string.IsNullOrWhiteSpace(baseUri))
            {
                if (Uri.TryCreate(baseUri, UriKind.Absolute, out Uri result)) return result;
            }

            return null;
        }

        public static Uri GetBaseUri(IConfiguration configuration = null)
        {
            var config = configuration == null ? ServiceLocator.Current.GetInstance<IConfiguration>() : configuration;

            var serverRole = Web.GetCurrentServerRole(configuration);
            var appSetting = "";

            switch (serverRole)
            {
                case Web.ServerRole.Combined:
                    appSetting = "WebServerUrl";
                    break;
                case Web.ServerRole.Application:
                    appSetting = "AppServerUrl";
                    break;
                case Web.ServerRole.Web:
                    appSetting = "WebServerUrl";
                    break;
                default:
                    return null;
            }

            var baseUri = config[$"configuration:appSettings:add:{appSetting}:value"];
            if (!string.IsNullOrWhiteSpace(baseUri))
            {
                if (Uri.TryCreate(baseUri, UriKind.Absolute, out Uri result)) return result;
            }

            var urlsString = config.GetValue<string>("urls");
            var splitter = new char[] { ';' };
            var urls = urlsString.Split(splitter, options: StringSplitOptions.RemoveEmptyEntries);
            var firstURL = urls?.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(firstURL))
            {
                if (Uri.TryCreate(firstURL, UriKind.Absolute, out Uri result)) return result;
            }

            return null;
        }

        public static ImapConfiguration GetImapSettings(IConfiguration configuration = null)
        {
            var config = configuration == null ? ServiceLocator.Current.GetInstance<IConfiguration>() : configuration;

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
