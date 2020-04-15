// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using System.IO;
using Microsoft.Extensions.Configuration;

namespace zAppDev.DotNet.Framework.Extensions.WebConfig {
    public class WebConfigurationProvider : FileConfigurationProvider
    {
        public WebConfigurationProvider(WebConfigurationSource source) : base(source) { }

        public override void Load(Stream stream)
        {
            var parser = new WebConfigurationFileParser();
            
            Data = parser.Parse(stream);
        }
    }
}
#endif