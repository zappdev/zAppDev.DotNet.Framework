#if NETFRAMEWORK
using System.Web;
#else
using Microsoft.AspNetCore.Http;
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
            _httpContext.Session.Set(key, Utilities.BinarySerializer.Serialize(value));
#endif
        }

        public object Get(string key)
        {
#if NETFRAMEWORK
            return _httpContext.Session[key];
#else
            var result = _httpContext.Session.Get(key);
            if(result == null) return null;
            return Utilities.BinarySerializer.Desserialize(result);
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
