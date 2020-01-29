// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using System;
using System.Net;
using System.Diagnostics;
using log4net;
using zAppDev.DotNet.Framework.Logging;
using zAppDev.DotNet.Framework.Utilities;
using CacheManager.Core;
using zAppDev.DotNet.Framework.Mvc;

namespace zAppDev.DotNet.Framework.Services
{
    public class CacheTime
    {
        // client cache length in seconds
        public TimeSpan ClientTimeSpan { get; set; }

        public TimeSpan? SharedTimeSpan { get; set; }

        public DateTimeOffset AbsoluteExpiration { get; set; }
    }

    public interface IModelQuery<in TModel, out TResult>
    {
        TResult Execute(TModel model);
    }

    public class ShortTime : IModelQuery<DateTime, CacheTime>
    {
        private readonly int serverTimeInSeconds;
        private readonly int clientTimeInSeconds;
        private readonly int? sharedTimeInSecounds;

        public ShortTime(int serverTimeInSeconds, int clientTimeInSeconds, int? sharedTimeInSecounds)
        {
            if (serverTimeInSeconds < 0)
                serverTimeInSeconds = 0;

            this.serverTimeInSeconds = serverTimeInSeconds;

            if (clientTimeInSeconds < 0)
                clientTimeInSeconds = 0;

            this.clientTimeInSeconds = clientTimeInSeconds;

            if (sharedTimeInSecounds.HasValue && sharedTimeInSecounds.Value < 0)
                sharedTimeInSecounds = 0;

            this.sharedTimeInSecounds = sharedTimeInSecounds;
        }

        public CacheTime Execute(DateTime model)
        {
            var cacheTime = new CacheTime
            {
                AbsoluteExpiration = model.AddSeconds(serverTimeInSeconds),
                ClientTimeSpan = TimeSpan.FromSeconds(clientTimeInSeconds),
                SharedTimeSpan = sharedTimeInSecounds.HasValue ? (TimeSpan?)TimeSpan.FromSeconds(sharedTimeInSecounds.Value) : null
            };

            return cacheTime;
        }
    }

    public class ServiceConsumer<T>
    {
        #region Fields
        private string _cacheKey;
        private ExternalServiceCacheKeyGenerator _cacheKeyGenerator;
        private readonly Func<T, TimeSpan> _calculateExpiration;
        private readonly Func<ServiceConsumptionContainer, T> _invokeService;

        private readonly string StatusCodeCacheKeyPostFix = "|StatusCode";
        private readonly Guid _id = Guid.NewGuid();

        protected IAPILogger APILogger => ServiceLocator.Current.GetInstance<IAPILogger>();

        protected IExternalServiceCacheProvider ServiceCacheProvider => ServiceLocator.Current.GetInstance<IExternalServiceCacheProvider>();

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

        private void SetCacheKey(ServiceConsumptionOptions serviceConsumptionOptions)
        {
            _cacheKeyGenerator = new ExternalServiceCacheKeyGenerator(ServiceLocator.Current.GetInstance<ICacheKeyHasher>())
            {
                CachePerUser = (serviceConsumptionOptions as ServiceConsumptionOptions).CachePerUser,
                ApiName = (serviceConsumptionOptions as ServiceConsumptionOptions).ApiName
            };

            _cacheKeyGenerator.SetServiceOptions(serviceConsumptionOptions);
            _cacheKey = _cacheKeyGenerator.MakeCacheKey();
        } //end SetCacheKey()

        private HttpStatusCode GetCachedResponseStatusCode(ServiceConsumptionOptions serviceConsumptionOptions, out bool found)
        {
            found = false;

            var statusCodeKey = _cacheKey + StatusCodeCacheKeyPostFix;

            if (!ServiceCacheProvider.Contains(statusCodeKey)) return default;

            found = ServiceCacheProvider.TryGetValue<HttpStatusCode>(statusCodeKey, out var val);

            if (!found) return default;

            return val;
        }

        public T GetFromCache(ServiceConsumptionOptions serviceConsumptionOptions, out bool found)
        {
            found = false;

            SetCacheKey(serviceConsumptionOptions);

            if (!ServiceCacheProvider.Contains(_cacheKey)) return default;
            found = ServiceCacheProvider.TryGetValue<T>(_cacheKey, out var val);
            if (!found) return default;

            LogManager.GetLogger(this.GetType()).Debug("Cache hit for: " + _cacheKey);
            return val;
        } //end GetFromCache()

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
                var baseKey = serviceConsumptionOptions.Url.Replace("/", "").Replace(":", "");
                ServiceCacheProvider.ExpirationMode = (Mvc.API.ExpirationMode)serviceConsumptionOptions.ExpirationMode;

                if (ServiceCacheProvider is ExternalServiceCacheProvider)
                {
                    ServiceCacheProvider.ExpirationTimeSpan = cacheTime.ClientTimeSpan;
                }

                //_webApiCache.Add(baseKey, string.Empty, cacheTime.AbsoluteExpiration);
                ServiceCacheProvider.Set(_cacheKey, result, cacheTime.AbsoluteExpiration);

                // store status code
                if (statusCode != null)
                {
                    ServiceCacheProvider.Set(_cacheKey + StatusCodeCacheKeyPostFix,
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
                var cachedResult = GetFromCache(serviceConsumptionOptions, out bool found);

                if (found)
                {
                    if (serviceConsumptionOptions.LogAccess)
                    {
                        var cachedResultStatusCode = GetCachedResponseStatusCode(serviceConsumptionOptions, out bool statusCodeFound);

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