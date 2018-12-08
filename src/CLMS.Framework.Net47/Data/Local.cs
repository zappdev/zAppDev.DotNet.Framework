using System;
using System.Collections;
using System.Web;

namespace CLMS.Framework.Data
{
    public static class Local
    {
        public static LocalData Data { get; } = new LocalData();
    }

    public class LocalData
    {
        [ThreadStatic]
        private static Hashtable _localData;
        private static readonly object LocalDataHashtableKey = new object();

        private static Hashtable LocalHashtable
        {
            get
            {
#if NETFRAMEWORK
                if (!RunningInWeb)
                {
                    if (_localData == null)
                        _localData = new Hashtable();
                    return _localData;
                }
                var web_hashtable = HttpContext.Current.Items[LocalDataHashtableKey] as Hashtable;
                if (web_hashtable == null)
                {
                    web_hashtable = new Hashtable();
                    HttpContext.Current.Items[LocalDataHashtableKey] = web_hashtable;
                }
                return web_hashtable;
#else
throw new NotImplementedException();
#endif
            }
        }

        public object this[object key]
        {
            get
            {
                return LocalHashtable[key];
            }
            set
            {
                LocalHashtable[key] = value;
            }
        }

        public int Count => LocalHashtable.Count;

        public void Clear()
        {
            LocalHashtable.Clear();
        }

        public static bool RunningInWeb
        {
            get
            {
#if NETFRAMEWORK
                return HttpContext.Current != null;
#else
                throw new NotImplementedException();
#endif
            }
        }
    }
}