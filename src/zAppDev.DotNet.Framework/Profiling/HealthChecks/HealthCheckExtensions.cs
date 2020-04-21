#if NETFRAMEWORK
#else

using HealthChecks.Network.Core;
using HealthChecks.RabbitMQ;
using HealthChecks.Uris;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Configuration;
using zAppDev.DotNet.Framework.Data.DatabaseManagers.DatabaseUtilities;
using zAppDev.DotNet.Framework.Logging;
using zAppDev.DotNet.Framework.Profiling.HealthChecks.CustomHealthChecks;
using zAppDev.DotNet.Framework.Services;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Profiling.HealthChecks
{
    public static class HealthCheckExtensions
    {
        private static IHealthChecksBuilder AddImapHealthCheck(this IHealthChecksBuilder builder, IConfiguration configuration)
        {
            var imapSettings = ConfigurationHandler.GetImapSettings(configuration);

            if (string.IsNullOrWhiteSpace(imapSettings.Host)) return builder;

            return builder.AddImapHealthCheck(setup =>
            {
                setup.Host = imapSettings.Host;
                setup.Port = imapSettings.Port.GetValueOrDefault();
                setup.AllowInvalidRemoteCertificates = true;
                setup.ConnectionType = imapSettings.EnableSsl.Value == true ? ImapConnectionType.SSL_TLS : ImapConnectionType.AUTO;
                setup.LoginWith(imapSettings.Username, imapSettings.Password);
            });
        }

        private static IHealthChecksBuilder AddRabbitMQCheck(this IHealthChecksBuilder builder, IConfiguration configuration)
        {
            var connectionFactory = RabbitMQMessagingLogger.GetConnectionFactory(configuration);
            if (string.IsNullOrWhiteSpace(connectionFactory.HostName)) return builder;

            return builder.AddRabbitMQ($"amqp://{connectionFactory.HostName}:{connectionFactory.Port}", sslOption: connectionFactory.Ssl);
        }
        private static IHealthChecksBuilder AddDatabaseCheck(this IHealthChecksBuilder builder, IConfiguration configuration)
        {
            var dbServerType = CommonUtilities.GetDatabaseServerTypeFromConfiguration(configuration);
            var connectionString = CommonUtilities.GetConnectionString(configuration);

            switch (dbServerType)
            {
                case zAppDev.DotNet.Framework.Data.DatabaseManagers.DatabaseServerType.SQLite:
                    return builder.AddSqlite(connectionString);
                case zAppDev.DotNet.Framework.Data.DatabaseManagers.DatabaseServerType.MSSQL:
                    return builder.AddSqlServer(connectionString);
                case zAppDev.DotNet.Framework.Data.DatabaseManagers.DatabaseServerType.MariaDB:
                    return builder.AddMySql(connectionString);
                default:
                    break;
            }

            return builder;
        }

        private static IHealthChecksBuilder AddMySql(this IHealthChecksBuilder builder, string connectionString, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration(
                "mysql",
                sp => new MySqlCustomHealthCheck(connectionString),
                failureStatus,
                tags));
        }

        private static IHealthChecksBuilder AddSMTPCheck(this IHealthChecksBuilder builder, IConfiguration configuration)
        {
            var smtpSettings = ConfigurationHandler.GetSmtpSettings(configuration);
            if (string.IsNullOrWhiteSpace(smtpSettings?.Smtp?.Network?.Host)) return builder;

            var enableSSL = false;
            if (bool.TryParse(configuration[$"configuration:appSettings:EnableSSL:value"], out var parsedBool)) enableSSL = parsedBool;

            return builder.AddSmtpHealthCheck(setup =>
            {
                //SSL on by default
                setup.Host = smtpSettings.Smtp.Network.Host;
                setup.Port = smtpSettings.Smtp.Network.Port;
                setup.ConnectionType = enableSSL ? SmtpConnectionType.SSL : SmtpConnectionType.AUTO;
                setup.AllowInvalidRemoteCertificates = true;
                setup.LoginWith(smtpSettings.Smtp.Network.UserName, smtpSettings.Smtp.Network.Password);
            });

        }

        private static IHealthChecksBuilder AddServerExternalIPCheck(this IHealthChecksBuilder builder, IConfiguration configuration)
        {
            var uriString = configuration[$"configuration:appSettings:add:ServerExternalIP:value"];
            if (string.IsNullOrEmpty(uriString)) return builder;

            if(Uri.TryCreate(uriString, UriKind.Absolute, out Uri url))
            {
                builder.AddUrlGroup(url, "ServerExternalIP");
            }
            return builder;
        }

            private static IHealthChecksBuilder AddOtherTierURICheck(this IHealthChecksBuilder builder, IConfiguration configuration)
        {
            var serverRole = Web.GetCurrentServerRole(configuration);
            var url = ConfigurationHandler.GetOtherTierURL(configuration, serverRole);
            if (url == null) return builder;

            var testName = serverRole == Web.ServerRole.Web ? "Application" : "Web";
            builder.AddUrlGroup(url, name: testName);
            return builder;
        }

        private static IHealthChecksBuilder AddWSDLServiceURIChecks(this IHealthChecksBuilder builder, IConfiguration configuration)
        {
            return builder;
        }


        public static IHealthChecksBuilder AddWSDLHealthChecks(this IHealthChecksBuilder builder, Dictionary<string, Uri> endpoints)
        {
            foreach(var endpoint in endpoints)
            {
                builder.AddUrlGroup(endpoint.Value, name: endpoint.Key);
            }

            return builder;
        }

        private static IHealthChecksBuilder AddRestServiceURIChecks(this IHealthChecksBuilder builder, string applicationName)
        {
            //var x = ServiceLocator.Current.GetInstance<IHostingEnvironment>();
            var restServices = AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(t => t.GetTypes())
                       .Where(t => t.IsClass && t.Namespace == $"{applicationName}.BLL.ExternalRestServices" && t.FullName.EndsWith("RestService"));

            foreach (var restService in restServices)
            {
                var baseUrl = restService.GetProperty("BaseUrl")?.GetValue(null)?.ToString();
                if (!string.IsNullOrWhiteSpace(baseUrl))
                {
                    builder.AddUrlGroup(new Uri(baseUrl), name: restService.Name);
                }
            }

            return builder;

        }


        private static IHealthChecksBuilder AddSignalRCheck(this IHealthChecksBuilder builder, IConfiguration configuration)
        {
            var baseUri = ConfigurationHandler.GetBaseUri(configuration);
            if (baseUri == null) return builder;

            string absoluteUri = baseUri.AbsoluteUri;
            if (!absoluteUri.EndsWith("/"))
                absoluteUri += "/";

            return builder.AddSignalRHub($"{absoluteUri}hub");
        }

        public static void AddAppDevHealthChecks(this IServiceCollection services, IConfiguration configuration, string applicationName)
        {
            services
                .AddHealthChecks()
                .AddImapHealthCheck(configuration)
                .AddDatabaseCheck(configuration)
                .AddSMTPCheck(configuration)
                .AddRabbitMQCheck(configuration)
                .AddRestServiceURIChecks(applicationName)
                .AddSignalRCheck(configuration)
                .AddOtherTierURICheck(configuration)
                .AddServerExternalIPCheck(configuration);
        }
    }
}
#endif