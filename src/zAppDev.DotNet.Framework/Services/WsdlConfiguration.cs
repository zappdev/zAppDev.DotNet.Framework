// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using Microsoft.Extensions.Configuration;
using System;
using System.ServiceModel;

namespace zAppDev.DotNet.Framework.Services
{
    public interface IWsdlConfiguration
    {

        BasicHttpBinding GetBinding(string serviceName);

        EndpointAddress GetEndpointAddress(string serviceName);
    }

    public class WsdlConfiguration : IWsdlConfiguration
    {
        private const string ConfigurationPath = "configuration:system.serviceModel";

        private IConfiguration _configuration { get; set; }

        public WsdlConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public BasicHttpBinding GetBinding(string serviceName)
        {
            var bindingTargetBasePath = $"{ConfigurationPath}:bindings:basicHttpBinding:{serviceName}Binding:binding:{serviceName}Binding";

            var binding = new BasicHttpBinding
            {
                MaxBufferSize = int.MaxValue,
                ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max,
                MaxReceivedMessageSize = int.MaxValue,
                Name = serviceName,
            };

            var value = _configuration[$"{bindingTargetBasePath}:sendTimeout"];
            if (!string.IsNullOrEmpty(value))
                binding.SendTimeout = TimeSpan.Parse(value);

            value = _configuration[$"{bindingTargetBasePath}:receiveTimeout"];
            if (!string.IsNullOrEmpty(value))
                binding.ReceiveTimeout = TimeSpan.Parse(value);

            value = _configuration[$"{bindingTargetBasePath}:closeTimeout"];
            if (!string.IsNullOrEmpty(value))
                binding.SendTimeout = TimeSpan.Parse(value);

            value = _configuration[$"{bindingTargetBasePath}:openTimeout"];
            if (!string.IsNullOrEmpty(value))
                binding.SendTimeout = TimeSpan.Parse(value);

            value = _configuration[$"{bindingTargetBasePath}:allowCookies"];
            if (!string.IsNullOrEmpty(value))
            {
                bool.TryParse(value, out var result);
                binding.AllowCookies = result;
            }

            value = _configuration[$"{bindingTargetBasePath}:useDefaultWebProxy"];
            if (!string.IsNullOrEmpty(value))
            {
                bool.TryParse(value, out var result);
                binding.UseDefaultWebProxy = result;
            }

            return binding;
        }

        public EndpointAddress GetEndpointAddress(string serviceName)
        {
            var bindingTargetBasePath = $"{ConfigurationPath}:client:endpoint:{serviceName}:address";
            return new EndpointAddress(_configuration[bindingTargetBasePath]);
        }
    }
}
#endif