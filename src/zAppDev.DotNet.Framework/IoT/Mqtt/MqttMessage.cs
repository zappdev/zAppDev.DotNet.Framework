// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.

namespace zAppDev.DotNet.Framework.IoT.Mqtt
{
    public class MqttMessage<T>
    {
        public string ClientId { get; set; }

        public string Topic { get; set; }

        public T Payload { get; set; }

        public MqttMessage(): this(null, null, default(T)) { }

        public MqttMessage(string clientId, string topic, T payload)
        {
            ClientId = clientId;
            Topic = topic;
            Payload = payload;
        }
    }
}
