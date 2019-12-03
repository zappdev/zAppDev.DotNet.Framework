// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using MQTTnet;
using MQTTnet.Server;
using MQTTnet.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace zAppDev.DotNet.Framework.IoT.Mqtt
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMqttWebsocketBroker(this IServiceCollection services, IConfiguration configuration)
        {
            var mqttServerOptions = new MqttServerOptionsBuilder().WithoutDefaultEndpoint().Build();
            services.AddHostedMqttServer(mqttServerOptions).AddMqttConnectionHandler().AddConnections();

        }
    }

    public static class ApplicationExtensions
    { 
        public static void UseMqttWebsocketBroker(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.UseConnections(c => c.MapConnectionHandler<MqttConnectionHandler>("/mqtt", options => {
                options.WebSockets.SubProtocolSelector = ApplicationBuilderExtensions.SelectSubProtocol;
            }));

            var _logger = app.ApplicationServices.GetRequiredService<ILogger<MqttServer>>();

            //app.UseMqttEndpoint();
            app.UseMqttServer(server =>
            {
                server.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(e =>
                {
                    _logger.LogInformation($"A new client with '{e.ClientId}' id is connected.");
                });

                server.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(e =>
                {
                    _logger.LogInformation($"The client with '{e.ClientId}' id is disconnected.");
                });

                server.StartedHandler = new MqttServerStartedHandlerDelegate(args =>
                {
                    _logger.LogInformation($"The MQTT WebSocket broker is now ready to accept connections.");
                });
            });
        }
    }
}
#endif
