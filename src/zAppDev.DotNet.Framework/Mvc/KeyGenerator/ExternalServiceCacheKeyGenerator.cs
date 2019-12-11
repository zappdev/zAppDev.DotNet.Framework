// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using System;
using Microsoft.AspNetCore.Http;
using zAppDev.DotNet.Framework.Mvc;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Services
{
    public class ExternalServiceCacheKeyGenerator
    {
        public bool CachePerUser { get; set; }
        public string ApiName { get; set; }

        private IServiceConsumptionOptions _serviceOptions;

        private readonly ICacheKeyHasher _cacheKeyHasher;

        private const string AnonymousUserName = "anon";

        public ExternalServiceCacheKeyGenerator(ICacheKeyHasher cacheKeyHasher)
        {
            _cacheKeyHasher = cacheKeyHasher;
            
        }

        public void SetServiceOptions(IServiceConsumptionOptions serviceOptions)
        {
            _serviceOptions = serviceOptions ?? throw new ArgumentNullException("serviceOptions");
        }

        public string MakeCacheKey(HttpContext context = null, bool excludeQueryString = false)
        {
            string queryString;
            try
            {
                var uri = new Uri(_serviceOptions.Url);
                queryString = uri.Query;
            }
            catch (Exception)
            {
                queryString = _serviceOptions.Arguments;
            }

            var usernameCacheKey = GetCachePerUserKey(context);

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

            _cacheKeyHasher.ApiName = ApiName;
            _cacheKeyHasher.Operation = _serviceOptions.Operation;
            _cacheKeyHasher.UserName = usernameCacheKey;
            _cacheKeyHasher.OriginalKey = $"{_serviceOptions.Verb}|{_serviceOptions.ClientId}|{_serviceOptions.AccessTokenUrl}|{_serviceOptions.CallBackUrl}|{query}|{data}|{formData}|{extraHeaderData}";

            return _cacheKeyHasher.GetHashedKey();
        }

        private string GetCachePerUserKey(HttpContext context)
        {
            context = context ?? Web.GetContext();

            if (CachePerUser)
            {
                string usernameCacheKey;
                if (context != null)
                {
                    usernameCacheKey = context.User.Identity.IsAuthenticated == true
                        ? context.User.Identity.Name
                        : AnonymousUserName;
                }
                else
                {
                    usernameCacheKey = string.IsNullOrEmpty(_serviceOptions.UserName)
                        ? _serviceOptions.UserName
                        : AnonymousUserName;
                }

                return usernameCacheKey;
            }
            else
            {
                return "";
            }

        }
    }//end GenericCacheKeyGenerator class
}//end namespace

#endif