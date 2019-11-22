// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using Microsoft.Extensions.Configuration;
using System;

namespace zAppDev.DotNet.Framework.Configuration
{
    public class AppConfiguration
    {
        private IConfiguration _configuration;

        public AppConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string this[string key]
        {
            get => GetValue(key);
            set => SetValue(key, value);
        }

        public string GetValue(string key)
        {
            return _configuration?[$"configuration:appSettings:add:{key}:value"]; 
        }

        public void SetValue(string key, string value)
        {
            throw new NotSupportedException("Set configuration value is not supported");
        }

    }

    public class ConnectionSettings
    {
        public string Name { get; set; }

        public string ConnectionString { get; set; }

        public string ProviderName { get; set; }
    }
}
