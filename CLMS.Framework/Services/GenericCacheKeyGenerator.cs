using System;
using System.Web.Http.Controllers;
using WebApi.OutputCache.V2;

namespace CLMS.Framework.Services
{
    public class GenericCacheKeyGenerator : DefaultCacheKeyGenerator
    {
        public bool CachePerUser { get; set; }
        public string ApiName { get; set; }

        private readonly IServiceConsumptionOptions _serviceOptions;
        private readonly ICacheKeyGenerator _specificCacheKeyGenerator;
        private readonly ICacheKeyHasher _cacheKeyHasher = new CacheKeyHasher();

        private const string AnonymusUserName = "anon";

        /// <summary>
        /// Creates a new CacheKeyGenerator, that relies on a IServiceConsumptionOptions class, not requiring an HttpActionContext
        /// </summary>
        /// <exception cref="ArgumentNullException">If serviceOptions are null</exception>
        /// <param name="serviceOptions"></param>
        public GenericCacheKeyGenerator(IServiceConsumptionOptions serviceOptions)
        {
            if (serviceOptions == null) throw new ArgumentNullException("serviceOptions");
            _serviceOptions = serviceOptions;
            _specificCacheKeyGenerator = new CustomCacheKeyGenerator();
        }

        public string MakeCacheKey(HttpActionContext context = null, bool excludeQueryString = false)
        {
            string queryString = "";
            try
            {
                var uri = new Uri(_serviceOptions.Url);
                queryString = uri.Query;
            }
            catch (Exception)
            {
                queryString = _serviceOptions.Arguments;
            }

            var usernameCacheKey = "";

            if (context == null)
            {
                if (_serviceOptions is ServiceConsumptionOptions) context = (_serviceOptions as ServiceConsumptionOptions).ActionExecutedContext;
            }

            if (CachePerUser)
            {
                if (context != null)
                {
                    usernameCacheKey = context.RequestContext.Principal.Identity.IsAuthenticated
                    ? context.RequestContext.Principal.Identity.Name
                    : AnonymusUserName;
                }
                else
                {
                    if (System.Web.HttpContext.Current != null)
                    {
                        usernameCacheKey = System.Web.HttpContext.Current?.User?.Identity?.IsAuthenticated == true
                            ? System.Web.HttpContext.Current.User.Identity.Name
                            : AnonymusUserName;
                    }
                    else
                    {
                        usernameCacheKey = string.IsNullOrEmpty(_serviceOptions.UserName)
                            ? _serviceOptions.UserName
                            : AnonymusUserName;
                    }
                }
            }

            var data = _serviceOptions.Data == null ? "".GetHashCode() : _serviceOptions.Data.GetHashCode();

            var formData = "";
            if (_serviceOptions.FormData != null)
            {
                foreach (var fd in _serviceOptions.FormData)
                {
                    formData += fd.Value.GetHashCode();
                }
            }


            var extraHeaderData = "";
            if (_serviceOptions.ExtraHeaderData != null)
            {
                foreach (var ehd in _serviceOptions.ExtraHeaderData)
                {
                    extraHeaderData += ehd.Value.GetHashCode();
                }
            }

            var query = "";

            if (!excludeQueryString) query = queryString;

            _cacheKeyHasher.ApiName = this.ApiName;
            _cacheKeyHasher.Operation = _serviceOptions.Operation;
            _cacheKeyHasher.UserName = usernameCacheKey;
            _cacheKeyHasher.OriginalKey = $"{_serviceOptions.Verb}|{_serviceOptions.ClientId}|{_serviceOptions.AccessTokenUrl}|{_serviceOptions.CallBackUrl}|{query}|{data}|{formData}|{extraHeaderData}";

            return _cacheKeyHasher.GetHashedKey();
        }//end MakeCacheKey()


    }//end GenericCacheKeyGenerator class
}//end namespace