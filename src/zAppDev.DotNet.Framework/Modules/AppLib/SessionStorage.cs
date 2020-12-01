#if NETFRAMEWORK
using System.Web;
#else
using Microsoft.AspNetCore.Http;
using zAppDev.DotNet.Framework.Utilities;
#endif


namespace zAppDev.DotNet.Framework.Modules.AppLib
{
    public class SessionStorage
    {
        private HttpContext _httpContext;

        public SessionStorage(HttpContext httpContext)
        {
            _httpContext = httpContext;
        }

        public void Add(string key, object value)
        {
#if NETFRAMEWORK
            _httpContext.Session.Add(key, value);
#else
            var jsonString = new Serializer<object>().ToJson(value);
            _httpContext.Session.Set(key, Utilities.BinarySerializer.Serialize(jsonString));
#endif
        }

        public object Get(string key)
        {
#if NETFRAMEWORK
            return _httpContext.Session[key];
#else
            var result = _httpContext.Session.Get(key);
            if(result == null) return null;
            var jsonString = Utilities.BinarySerializer.Deserialize(result) as string;

            return new Serializer<object>().FromJson(jsonString);
#endif
        }

        public void Remove(string key)
        {
#if NETFRAMEWORK
            _httpContext.Session.Remove(key);
#else
            _httpContext.Session.Remove(key);
#endif
        }

        public void Clear()
        {
#if NETFRAMEWORK
            _httpContext.Session.Clear();
#else
            _httpContext.Session.Clear();
#endif
        }

        public object this[string key]
        {
            get => Get(key);
            set => Add(key, value);
        }
    }
}
