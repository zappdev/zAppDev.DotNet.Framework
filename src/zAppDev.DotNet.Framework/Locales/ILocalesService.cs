// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Xml;

namespace zAppDev.DotNet.Framework.Locales
{
    public interface ILocalesService
    {
        HashSet<string> AvailiableLanguages { get; set; }

        string DefaultLang { get; set; }

        void ClearLocalesCache();
        string GetGlobalResource(string resourceKey, string lang);
        XmlDocument GetLocales(string formName, string lang = null);
        string GetResourceValue(string formName, string name, string lang = null, string fallback = "");
        XmlDocument PrepareResources(string form, string master, bool addGlobal = true);
    }
}