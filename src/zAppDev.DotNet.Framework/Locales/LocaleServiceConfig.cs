// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Locales
{
    public class LocaleServiceConfig
    {
        public HashSet<string> AvailiableLanguages { get; set; } = new HashSet<string>();

        public string DefaultLang { get; set; }
    }
}