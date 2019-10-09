#if NETFRAMEWORK
#else
using System;

namespace zAppDev.DotNet.Framework.Mvc.API
{
    public enum ExpirationMode
    {
        Default = 0,
        None = 1,
        Sliding = 2,
        Absolute = 3
    }

    public class ApiResponseCacheConfig
    {
        public bool NoCache { get; set; }

        private int? _sharedTimeSpan = null;

        public int SharedTimeSpan
        {
            get // required for property visibility
            {
                if (!_sharedTimeSpan.HasValue)
                    throw new Exception("should not be called without value set");
                return _sharedTimeSpan.Value;
            }
            set { _sharedTimeSpan = value; }
        }
        public int ClientTimeSpan { get; set; }
        public int ServerTimeSpan { get; set; }
        public bool ExcludeQueryStringFromCacheKey { get; set; }
        public bool MustRevalidate { get; set; }
        public bool AnonymousOnly { get; set; }
        public Type CacheKeyGenerator { get; set; }
        public bool Private { get; set; }
        public Type CustomTimeSpanMethodClassType { get; set; }
        public string CustomTimeSpanMethodName { get; set; }
        public bool CachePerUser { get; set; }
        public ExpirationMode ExpirationMode { get; set; }
        public string ApiName { get; set; }
        public bool LogEnabled { get; set; }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
    }
}

#endif