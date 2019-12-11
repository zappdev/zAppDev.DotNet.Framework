// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using System;
using System.Collections.Generic;
using System.Linq;
using CacheManager.Core;
using Newtonsoft.Json;
using zAppDev.DotNet.Framework.Mvc.API;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Services
{
    public interface IExternalServiceCacheProvider
    {
        Mvc.API.ExpirationMode ExpirationMode { get; set; }
        TimeSpan ExpirationTimeSpan { get; set; }

        bool Contains(string statusCodeKey);

        bool TryGetValue<TV>(string key, out TV response);
        void Set(string key, object obje, DateTimeOffset absoluteExpiration, string baseKey = null);

        void Clear();
    }

    public class ExternalServiceCacheProvider : IExternalServiceCacheProvider
    {
        private static readonly ISet<string> Keys = new HashSet<string>();

        public static JsonSerializerSettings CacheJsonSerializerSettings =>
            new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

        public Mvc.API.ExpirationMode ExpirationMode { get ; set; }

        private readonly string _nullCacheObject = "CLMS_NULL";

        public TimeSpan ExpirationTimeSpan { get; set; }
        
        private static ICacheManager<object> Instance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ICacheManager<object>>("ExternalServiceCache");
            }
        }

        public IEnumerable<string> AllKeys => Keys;

        public void Clear()
        {
            Instance.Clear();
        }

        public void RemoveStartsWith(string key)
        {
            var keys = Keys.Where(k => k.StartsWith(key)).ToList();
            foreach (var k in keys)
            {
                Remove(k);
            }
        }

        public void Remove(string key)
        {
            if (Instance.Remove(key))
            {
                Keys.Remove(key);
            }
        }

        public bool Contains(string key)
        {
            if (!Keys.Contains(key))
            {
                if (Instance.Exists(key))
                {
                    Keys.Add(key);
                    return true;
                }
                return false;
            }
            return true;
        }

        public void Set(string key, object obj, DateTimeOffset absoluteExpiration, string baseKey = null)
        {
            if (obj == null) obj = _nullCacheObject;
            var item = new CacheItem<object>(key, obj, (CacheManager.Core.ExpirationMode)ExpirationMode, ExpirationTimeSpan);

            Instance.Put(item);
            if (!Keys.Contains(key)) Keys.Add(key);
        }

        public bool TryGetValue<TV>(string key, out TV response)
        {
            try
            {
                response = Instance.Get<TV>(key);
                return response != null;
            }
            catch (InvalidCastException)
            {
                var value = Instance.Get<string>(key);
                if (value == _nullCacheObject)
                {
                    response = default;
                    return true;
                }
                response = default;
                return false;
            }
        }
    }
}

#endif