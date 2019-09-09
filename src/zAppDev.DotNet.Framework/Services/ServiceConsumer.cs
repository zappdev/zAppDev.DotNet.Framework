// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
using CacheManager.Core;
using zAppDev.DotNet.Framework.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Web.Http;
using log4net;
using WebApi.OutputCache.Core.Time;

namespace zAppDev.DotNet.Framework.Services
{
    public class ServiceConsumer<T>
    {
        #region Fields
        private readonly Func<ServiceConsumptionContainer, T> _invokeService;
        private readonly Func<T, TimeSpan> _calculateExpiration;
        private GenericCacheKeyGenerator _cacheKeyGenerator;
        private readonly CustomCacheProvider _webApiCache = new CustomCacheProvider();
        private string _cacheKey;
        private string StatusCodeCacheKeyPostFix = "|StatusCode";
        private readonly Guid _id = Guid.NewGuid();
   
        protected IAPILogger APILogger =>
            GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IAPILogger)) as IAPILogger;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new Cache-Enabled Service Consumer for any type of External Service (SOAP, REST, etc.)
        /// </summary>
        /// <param name="invokeService">Function that does the actual remote service invocation. E.g. () => { return ExternalRestService.Blog.GetPosts(); } </param>
        /// <example>
        ///     <code>
        ///     <![CDATA[ 
        ///         Func<List<Post>> invokeService = () => {
        ///             return ExternalRestService.Blog.GetPosts();
        ///         }
        ///         var consumer = new ServiceConsumer<List<Post>>(invokeService);
        ///         ]]>
        ///     </code>
        /// </example>
        public ServiceConsumer(Func<ServiceConsumptionContainer, T> invokeService)
        {
            _invokeService = invokeService;
        }

        /// <summary>
        /// Initializes a new Cache-Enabled Service Consumer for any type of External Service (SOAP, REST, etc.)
        /// </summary>
        /// <param name="invokeService">Function that does the actual remote service invocation. E.g. () => { return ExternalRestService.Blog.GetPosts(); } </param>
        /// <param name="calculateExpiration">Function to set a user-defined TimeSpan for which the result of the invoked service is to be cached. 
        ///     <![CDATA[ 
        ///     
        ///     E.g. (List<Post> posts) => { if(!posts.Any() return new TimeSpan(0, 0, 0)) else return new TimeSpan(1, 30, 0);  }
        ///     ]]>
        /// </param>
        /// <example>
        ///     <code>
        ///     <![CDATA[ 
        ///         Func<List<Post>> invokeService = () => {
        ///             return ExternalRestService.Blog.GetPosts();
        ///         }
        ///         Func<List<Post>, TimeSpan> calculateExpiration = (List<Post> posts) => {
        ///             if(posts.Any()) return new TimeSpan(1, 30, 0);
        ///             return new TimeSpan(0, 0, 0);
        ///         }
        ///         var consumer = new ServiceConsumer<List<Post>>(invokeService, calculateExpiration);
        ///         ]]>
        ///     </code>     
        /// </example>
        public ServiceConsumer(Func<ServiceConsumptionContainer, T> invokeService, Func<T, TimeSpan> calculateExpiration)
        {
            _invokeService = invokeService;
            _calculateExpiration = calculateExpiration;
        }
        #endregion

        #region Helpful methods

        public static JsonSerializerSettings CacheJsonSerializerSettings =>
            new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

        private void SetCacheKey(ServiceConsumptionOptions serviceConsumptionOptions)
        {
            _cacheKeyGenerator = new GenericCacheKeyGenerator(serviceConsumptionOptions);

            _cacheKeyGenerator.CachePerUser = (serviceConsumptionOptions as ServiceConsumptionOptions).CachePerUser;
            _cacheKeyGenerator.ApiName = (serviceConsumptionOptions as ServiceConsumptionOptions).ApiName;

            _cacheKey = _cacheKeyGenerator.MakeCacheKey();
        }//end SetCacheKey()

        private HttpStatusCode GetCachedResponseStatusCode(ServiceConsumptionOptions serviceConsumptionOptions, out bool found)
        {
            found = false;

            var statusCodeKey = _cacheKey + StatusCodeCacheKeyPostFix;

            if (!_webApiCache.Contains(statusCodeKey)) return default(HttpStatusCode);

            var val = _webApiCache.GetGeneric<HttpStatusCode>(statusCodeKey, out found);

            if (!found) return default(HttpStatusCode);

            return val;
        }

        public T GetFromCache(ServiceConsumptionOptions serviceConsumptionOptions, out bool found)
        {
            found = false;

            SetCacheKey(serviceConsumptionOptions);

            if (!_webApiCache.Contains(_cacheKey)) return default(T);
            var val = _webApiCache.GetGeneric<T>(_cacheKey, out found);
            if (!found) return default(T);

            LogManager.GetLogger(this.GetType()).Debug("Cache hit for: " + _cacheKey);

            return val;
        }//end GetFromCache()

        private void Validate(ServiceConsumptionOptions serviceConsumptionOptions)
        {
            if (serviceConsumptionOptions.IsCachingEnabled &&
                _calculateExpiration == null &&
                !serviceConsumptionOptions.ServerTimeSpan.HasValue &&
                serviceConsumptionOptions.ExpirationMode != ExpirationMode.None
            )
            {
                throw new ArgumentException("You must provide either a CalculateExpiration method, or a ServerTimeSpan Service Consumption Option for any Expiration Mode other than 'Never'");
            }
        }//end Validate()

        private void AddToCache(ServiceConsumptionOptions serviceConsumptionOptions, T result, HttpStatusCode? statusCode)
        {
            ShortTime cacheTimeQuery = new ShortTime(0, 0, null);
            if (serviceConsumptionOptions.ServerTimeSpan.HasValue)
            {
                cacheTimeQuery = new ShortTime(serviceConsumptionOptions.ServerTimeSpan.Value, serviceConsumptionOptions.ServerTimeSpan.Value, serviceConsumptionOptions.ServerTimeSpan.Value);
            }
            else
            {
                var userDefinedTimeSpan = _calculateExpiration(result);
                int totalSeconds = Convert.ToInt32(userDefinedTimeSpan.TotalSeconds);
                cacheTimeQuery = new ShortTime(totalSeconds, totalSeconds, null);
            }

            var cacheTime = cacheTimeQuery.Execute(DateTime.UtcNow);

            if (cacheTime.AbsoluteExpiration > DateTime.UtcNow || serviceConsumptionOptions.ExpirationMode == ExpirationMode.None)
            {
                var baseKey = serviceConsumptionOptions.Url.Replace("/", "").Replace(":","") ;
                _webApiCache.ExpirationMode = serviceConsumptionOptions.ExpirationMode; 

                if (_webApiCache is CustomCacheProvider)
                {
                    _webApiCache.ExpirationTimeSpan = cacheTime.ClientTimeSpan;
                }

				//_webApiCache.Add(baseKey, string.Empty, cacheTime.AbsoluteExpiration);
                _webApiCache.Add(_cacheKey, result, cacheTime.AbsoluteExpiration);

                // store status code
                if (statusCode != null)
                {
                    _webApiCache.Add(_cacheKey + StatusCodeCacheKeyPostFix,
                                    statusCode,
                                    cacheTime.AbsoluteExpiration, baseKey);
                }


                //_webApiCache.Add(_cacheKey + Constants.EtagKey,
                //                "etag", //todo
                //                cacheTime.AbsoluteExpiration, baseKey);
            }
        }// end AddToCache()

        #endregion  


        public T Invoke(ServiceConsumptionOptions serviceConsumptionOptions)
        {
            var timer = new Stopwatch();

            if (serviceConsumptionOptions.LogAccess)
            {
                timer.Start();
            }

            Validate(serviceConsumptionOptions);

            // Search in cache
            if (serviceConsumptionOptions.IsCachingEnabled)
            {
                var found = false;
                var cachedResult = GetFromCache(serviceConsumptionOptions, out found);

                if (found)
                {                    
                    if (serviceConsumptionOptions.LogAccess)
                    {
                        var statusCodeFound = false;
                        var cachedResultStatusCode = GetCachedResponseStatusCode(serviceConsumptionOptions, out statusCodeFound);

                        if (!statusCodeFound)
                        {
                            cachedResultStatusCode = HttpStatusCode.OK;
                        }

                        LogAccess(timer, serviceConsumptionOptions, cachedResult, cachedResultStatusCode, true);
                    }

                    return cachedResult;
                }
            }
            
            // Call service
            try
            {
                var resultBag = new ServiceConsumptionContainer();
                var result = _invokeService(resultBag);
               
                if (serviceConsumptionOptions.IsCachingEnabled &&
                    (resultBag.HttpResponseMessage == null || resultBag.HttpResponseMessage.IsSuccessStatusCode)) // null from SOAP service
                {
                    AddToCache(serviceConsumptionOptions, result, resultBag.HttpResponseMessage?.StatusCode);
                }

                if (serviceConsumptionOptions.LogAccess)
                {
                    LogAccess(timer, serviceConsumptionOptions, result, resultBag.HttpResponseMessage?.StatusCode, false);
                }

                return result;
            }
            catch (Exception)
            {
                if (serviceConsumptionOptions.LogAccess)
                {
                    LogAccess(timer, serviceConsumptionOptions, null, HttpStatusCode.InternalServerError, false);
                }

                throw;
            }
                      
        }// end Invoke()

        private void LogAccess(Stopwatch timer, ServiceConsumptionOptions serviceConsumptionOptions, object result, HttpStatusCode? code, bool cachedResponse)
        {                                    
            timer.Stop();

            APILogger?.LogExternalAPIAccess(
                _id, 
                serviceConsumptionOptions.ApiName,
                serviceConsumptionOptions.Operation,
                serviceConsumptionOptions, 
                result, 
                code ?? default(HttpStatusCode), 
                timer.Elapsed,
                cachedResponse: cachedResponse
            );
        }
    }
}
#endif