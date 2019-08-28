#if NETFRAMEWORK
using Autofac;
using Autofac.Integration.WebApi;
using CacheManager.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.SessionState;

namespace zAppDev.DotNet.Framework.Utilities
{
    internal class SessionState
    {
        internal static readonly JsonSerializerSettings DefaultDeserializationSettingsWithCycles = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ContractResolver = new NHibernateContractResolver()
        };

        public DateTime Created { get; set; }
        public bool Locked { get; set; }
        public int LockId { get; set; }
        public DateTime LockDate { get; set; }
        public int Timeout { get; set; }
        public SessionStateActions Flags { get; set; }
        public byte[] ItemsData { get; set; }

        [JsonIgnore]
        public SessionStateItemCollection Items { get; set; }
        
        internal SessionState()
        {
            Items = new SessionStateItemCollection();
            Locked = false;
            Created = DateTime.UtcNow;
        }

        public string Serialized()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                Items.Serialize(writer);
                ItemsData = ms.ToArray();
                writer.Close();
            }

            return JsonConvert.SerializeObject(this, DefaultDeserializationSettingsWithCycles);
        }

        public static bool TryParse(string raw, out SessionState data)
        {
            var result = false;

            if (string.IsNullOrEmpty(raw))
            {
                data = null;
                return result;
            }

            try
            {
                data = JsonConvert.DeserializeObject<SessionState>(raw, DefaultDeserializationSettingsWithCycles);
                using (var ms = new MemoryStream(data.ItemsData))
                {
                    if (ms.Length > 0)
                    {
                        using (var reader = new BinaryReader(ms))
                        {
                            data.Items = SessionStateItemCollection.Deserialize(reader);
                        }
                    }
                    else
                    {
                        data.Items = new SessionStateItemCollection();
                    }
                }
                result = true;
            }
            catch (Exception e)
            {
                data = null;
                log4net.LogManager.GetLogger(typeof(SessionState)).Error(e);
            }
            return result;
        }
    }


    public class SessionStateStoreProvider : SessionStateStoreProviderBase
    {   
        private readonly Func<HttpContext, HttpStaticObjectsCollection> _staticObjectsGetter;
        private string _name = "SessionStateStoreProvider";
        private string _cacheName;

        internal SessionStateStoreProvider(Func<HttpContext, HttpStaticObjectsCollection> staticObjectsGetter)
        {
            _staticObjectsGetter = staticObjectsGetter;
        }

        public SessionStateStoreProvider()
        {
            _staticObjectsGetter = ctx => SessionStateUtility.GetSessionStaticObjects(ctx);
            log4net.LogManager.GetLogger(GetType()).Debug($"SessionStateStoreProvider");
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            _name = name;
            _cacheName = config["cacheName"] ?? "SessionCache";
            log4net.LogManager.GetLogger(GetType()).Debug($"SessionStateStoreProvider.Initialize");
        }

        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            return new SessionStateStoreData(new SessionStateItemCollection(),
               _staticObjectsGetter(context),
               timeout);
        }

        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            var key = GetSessionIdKey(id);
            var client = GetClient();
            var state = new SessionState()
            {
                Timeout = timeout,
                Flags = SessionStateActions.InitializeItem
            };
            client.Put(key, state.Serialized());

        }

        public override void Dispose()
        {

        }

        public override void EndRequest(HttpContext context)
        {

        }

        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            return GetItem(false, context, id, out locked, out lockAge, out lockId, out actions);
        }

        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            return GetItem(true, context, id, out locked, out lockAge, out lockId, out actions);
        }

        public override void InitializeRequest(HttpContext context)
        {
        }

        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {

        }

        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            var key = GetSessionIdKey(id);
            var client = GetClient();
            client.Remove(key);

        }

        public override void ResetItemTimeout(HttpContext context, string id)
        {
            var client = GetClient();
        }

        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            var key = GetSessionIdKey(id);
            var client = GetClient();
            var state = new SessionState
            {
                Items = (SessionStateItemCollection)item.Items,
                Locked = false,
                Timeout = item.Timeout
            };
            client.Put(key, state.Serialized());

            log4net.LogManager.GetLogger(GetType()).Debug($"Put a new session item with key: {key}");
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            var client = GetClient();

            var eventDelegate = (MulticastDelegate) client.GetType().GetField("OnRemoveByHandle", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(client);

            if (eventDelegate == null)
            {
                client.OnRemoveByHandle += GetExprireHandler(expireCallback);
            }
            
            return true;
        }

        private EventHandler<CacheManager.Core.Internal.CacheItemRemovedEventArgs> GetExprireHandler(SessionStateItemExpireCallback expireCallback)
        {
            return (sender, args) =>
            {
                SessionState state = null;
                SessionStateStoreData result = null;
                SessionState.TryParse(args.Value as string, out state);
                result = new SessionStateStoreData(state.Items, null, state.Timeout);
                expireCallback.Invoke(args.Key, result);
            };
        }

        private SessionStateStoreData GetItem(bool isExclusive, HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            actions = SessionStateActions.None;
            lockId = null;
            locked = false;
            lockAge = TimeSpan.Zero;

            SessionStateStoreData result = null;
            SessionState state = null;

            var key = GetSessionIdKey(id);
            var client = GetClient();
            var data = client.Get<string>(key);
            if (!SessionState.TryParse(data, out state)) return null;

            var items = actions == SessionStateActions.InitializeItem ? new SessionStateItemCollection() : state.Items;
            result = new SessionStateStoreData(items, _staticObjectsGetter(context), state.Timeout);

            return result;
        }

        private string GetSessionIdKey(string id)
        {
            return $"{_name}#{id}";
        }

        private ICacheManager<object> GetClient()
        {
            var resolver = GlobalConfiguration.Configuration.DependencyResolver as AutofacWebApiDependencyResolver;
            return resolver.Container.ResolveNamed<ICacheManager<object>>(_cacheName);
        }
    }
}
#endif