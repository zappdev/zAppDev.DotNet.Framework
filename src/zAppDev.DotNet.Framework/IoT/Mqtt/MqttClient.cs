// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Server;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.IoT.Mqtt
{
    public class MqttClient
    {
        private static readonly JsonSerializerSettings DefaultDeserializationSettingsWithCycles = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ContractResolver = new NHibernateContractResolver()
        };

        public static string ServerUri { get; set; }

        public static IMqttServer Instance
        {
            get => ServiceLocator.Current.GetInstance<IMqttServer>();
        }

        public static void Publish<TV>(string topic, TV value)
        {
            var msg = new MqttApplicationMessageBuilder()
                        .WithPayload(JsonConvert.SerializeObject(value, DefaultDeserializationSettingsWithCycles))
                        .WithTopic(topic);
            Instance.PublishAsync(msg.Build()).GetAwaiter().GetResult();
        }

        public static void Subscribe<T>(string clientid, string topic, Action<MqttMessage<T>> handler)
        {
            SubscribeAsync(clientid, topic, handler).GetAwaiter().GetResult();
        }

        public static async Task SubscribeAsync<T>(string clientid, string topic, Action<MqttMessage<T>> handler)
        {
            await ConnectWsAsync("ws://localhost:65130/mqtt", clientid, topic, e =>
            {
                var payload = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(e.ApplicationMessage.Payload), DefaultDeserializationSettingsWithCycles);

                handler?.Invoke(new MqttMessage<T>(e.ClientId, e.ApplicationMessage.Topic, payload));
            });
        }

        private static async Task<IMqttClient> ConnectWsAsync(string url, string clientid, string topic, Action<MqttApplicationMessageReceivedEventArgs> handler)
        {
            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId(clientid)
                .WithWebSocketServer(url)
                .WithCleanSession()
                .Build();

            mqttClient.UseApplicationMessageReceivedHandler(handler);

            mqttClient.UseConnectedHandler(async e =>
            {
                await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic).Build());
            });

            await mqttClient.ConnectAsync(options, CancellationToken.None);

            mqttClient.UseDisconnectedHandler(async e =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await mqttClient.ConnectAsync(options, CancellationToken.None);
                }
                catch
                {
                }
            });

            return mqttClient;
        }
    }
}
