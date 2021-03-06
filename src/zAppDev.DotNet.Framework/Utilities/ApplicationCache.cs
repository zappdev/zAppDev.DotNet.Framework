// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using CacheManager.Core;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace zAppDev.DotNet.Framework.Utilities
{
    public class ApplicationCache
    {
        public static readonly JsonSerializerSettings DefaultDeserializationSettingsWithCycles = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
			ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ContractResolver = new NHibernateContractResolver()
        };

        private static object _dictionaryLock = new object();
        private static Dictionary<string, object> _lock = new Dictionary<string, object>();

        public static ICacheManager<object> Instance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ICacheManager<object>>("AppCache");
            }
        }

        public static bool Add<TV>(string key, TV fallbackValue)
        {
            throw new NotImplementedException();
        }

        public static TV AddOrGetExisting<TV>(string key, TV fallbackValue)
        {
            throw new NotImplementedException();
        }

        public static bool Contains(string key)
        {
            throw new NotImplementedException();
        }

        public static TV Get<TV>(string key)
        {
            var data = Instance.Get<string>(key);
            if (data == null)
            {
                return default(TV);
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TV>(data, DefaultDeserializationSettingsWithCycles);
        }

        public static TV Get<TV>(string key, bool throwable)
        {
            throw new NotImplementedException();
        }

        public static TV Get<TV>(string key, TV fallbackValue)
        {
            throw new NotImplementedException();
        }

        public static bool HasKey(string key)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<KeyValuePair<string, object>> KeyValuePair(Func<KeyValuePair<string, object>, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public static TV Remember<TV>(string key, Func<TV> func)
        {
            throw new NotImplementedException();
        }

        public static TV Remove<TV>(string key)
        {
            var value = Get<TV>(key);
            Instance.Remove(key);
            return value;
        }

        public static void Set<TV>(string key, TV value)
        {
            var serialized = JsonConvert.SerializeObject(value, DefaultDeserializationSettingsWithCycles);
            Instance.Put(key, serialized);
        }

        public static bool AddIfNotExists<TV>(string key, TV value)
        {
            object lockVariable = null;
            lock(_dictionaryLock)
            {
                if (!_lock.ContainsKey(key))
                {
                    lockVariable = new object();
                   _lock.Add(key, lockVariable);
                } else
                {
                    lockVariable = _lock[key];
                }
            }

            lock(lockVariable)
            {
                if (Instance.Exists(key)) {
                    return false;
                }

                var serialized = JsonConvert.SerializeObject(value, DefaultDeserializationSettingsWithCycles);
                Instance.Put(key, serialized);
                return true;
            }
        }
    }
}
