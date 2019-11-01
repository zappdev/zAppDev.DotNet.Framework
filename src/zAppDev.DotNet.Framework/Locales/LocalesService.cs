// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.

#if NETFRAMEWORK
#else

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using zAppDev.DotNet.Framework.Mvc.API;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Locales
{
    public class LocalesService : ILocalesService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private Dictionary<string, XmlDocument> _cache = new Dictionary<string, XmlDocument>();

        private Dictionary<string, Dictionary<string, string>> _formToResources = new Dictionary<string, Dictionary<string, string>>();

        private readonly ILogger<LocalesService> _logger;

        public HashSet<string> AvailiableLanguages { get; set; } = new HashSet<string>();

        public string DefaultLang { get; set; }

        public LocalesService(ILogger<LocalesService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public void ClearLocalesCache()
        {
            _cache = new Dictionary<string, XmlDocument>();
            _formToResources = new Dictionary<string, Dictionary<string, string>>();
        }

        public string GetGlobalResource(string resourceKey, string lang)
        {
            return GetResourceValue("GlobalResources", resourceKey, lang);
        }

        private Dictionary<string, string> GetLocalesAsDictionary(string formName, string lang)
        {
            var key = GetFormResourcesKey(formName, lang);
            // Create Dictionary
            if (!_formToResources.ContainsKey(key))
            {
                var doc = GetLocales(formName, lang);
                var resourceNodes = doc.SelectNodes("/Locales/Loc");
                var innerDictionary = new Dictionary<string, string>();
                foreach (XmlElement node in resourceNodes)
                {
                    var resourceKey = node.GetAttribute("Key");
                    var resourceValue = node.GetAttribute("Value");
                    if (innerDictionary.ContainsKey(resourceKey)) continue;
                    innerDictionary.Add(resourceKey, resourceValue);
                }
                _formToResources[key] = innerDictionary;
            }
            var dic = _formToResources[key];
            return dic;
        }

        private string GetFormResourcesKey(string formName, string lang)
        {
            if (!AvailiableLanguages.Contains(DefaultLang))
            {
                DefaultLang = AvailiableLanguages.First();
            }

            if (lang == null)
            {
                lang = GetCurrentLanguage();
            }

            if (!AvailiableLanguages.Contains(lang))
            {
                try
                {
                    var bestMatch = AvailiableLanguages.FirstOrDefault(l => l.Split('-').First() == lang.Split('-').First());
                    lang = bestMatch ?? DefaultLang;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not get best language match!");
                    lang = DefaultLang;
                }
            }
            var key = formName + "_" + lang;
            return key;
        }

        public XmlDocument GetLocales(string formName, string lang = null)
        {
            var key = GetFormResourcesKey(formName, lang);
            if (_cache.ContainsKey(key))
            {
                return _cache[key].CloneNode(true) as XmlDocument;
            }
            var doc = new XmlDocument();
            doc.LoadXml(GetResourceContents(key + "_res.xml"));
            if (lang != DefaultLang)
            {
                MergeWithDefaultResources(formName, doc);
            }
            if (!_cache.ContainsKey(key))
            {
                _cache.Add(key, doc);
            }
            return doc.CloneNode(true) as XmlDocument;
        }

        private void MergeWithDefaultResources(string formName, XmlDocument doc)
        {
            var defaultResources = GetLocales(formName, DefaultLang);
            foreach (XmlElement node in defaultResources.SelectNodes("/Locales/Loc"))
            {
                var translated = doc.SelectSingleNode("/Locales/Loc[@Key='" + node.GetAttribute("Key") + "']");
                if (string.IsNullOrWhiteSpace(translated?.Attributes["Value"]?.Value))
                {
                    var imported = doc.ImportNode(node, true) as XmlElement;
                    doc.DocumentElement.AppendChild(imported);
                    if (translated != null)
                    {
                        doc.DocumentElement.RemoveChild(translated);
                    }
                }
            }
        }

        public string GetResourceValue(string formName, string name, string lang = null, string fallback = "")
        {
            var formResources = GetLocalesAsDictionary(formName, lang);
            return formResources.ContainsKey(name)
                   ? formResources[name]
                   : fallback;
        }

        private string GetResourceContents(string fullPath)
        {
            return File.ReadAllText(Web.MapPath("~/Locales/" + fullPath));
        }

        public XmlDocument PrepareResources(string form, string master, bool addGlobal = true)
        {
            var resources = GetLocales(form);
            if (!string.IsNullOrWhiteSpace(master))
            {
                // Merge Master Page Resources
                var masterResources = GetLocales(master);
                foreach (XmlElement node in masterResources.SelectNodes("//Loc"))
                {
                    var copied = resources.ImportNode(node, true) as XmlElement;
                    copied.SetAttribute("Key", "MASTER_" + copied.GetAttribute("Key"));
                    resources.DocumentElement.AppendChild(copied);
                }
            }
            if (addGlobal)
            {
                // Merge Global Resources
                var globalResources = GetLocales("GlobalResources");
                foreach (XmlElement node in globalResources.SelectNodes("//Loc"))
                {
                    var copied = resources.ImportNode(node, true) as XmlElement;
                    copied.SetAttribute("Key", "GLOBAL_" + copied.GetAttribute("Key"));
                    resources.DocumentElement.AppendChild(copied);
                }
            }
            return resources;
        }

        private string GetCurrentLanguage()
        {

            if (!_httpContextAccessor.HttpContext.Items.ContainsKey(HttpContextItemKeys.Culture)) return null;
            return (_httpContextAccessor.HttpContext.Items[HttpContextItemKeys.Culture] as CultureInfo).Name.ToLowerInvariant();
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static void AddGlobalization(this IServiceCollection services, LocaleServiceConfig configuration)
        {
            services.AddSingleton<ILocalesService>((context) =>
            {
                return new LocalesService(context.GetService<ILogger<LocalesService>>(), context.GetService<IHttpContextAccessor>())
                {
                    DefaultLang = configuration.DefaultLang,
                    AvailiableLanguages = configuration.AvailiableLanguages
                };
            });
        }
    }
}
#endif