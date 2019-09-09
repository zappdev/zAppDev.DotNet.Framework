// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Configuration
{
    public class AppConfiguration
    {
        public Dictionary<string, ConnectionSettings> ConnectionStrings { get; set; }

        public Dictionary<string, string> AppSettings { get; set; }
        
        public ImapConfiguration ImapConfiguration { get; set; }
    }

    public class ConnectionSettings
    {
        public string Name { get; set; }

        public string ConnectionString { get; set; }

        public string ProviderName { get; set; }
    }
}
