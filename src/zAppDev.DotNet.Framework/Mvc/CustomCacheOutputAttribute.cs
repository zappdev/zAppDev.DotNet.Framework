// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
using CacheManager.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebApi.OutputCache.Core;
using WebApi.OutputCache.Core.Cache;
using WebApi.OutputCache.V2;
using zAppDev.DotNet.Framework.Services;
using zAppDev.DotNet.Framework.Logging;
using System.Diagnostics;

namespace zAppDev.DotNet.Framework.Mvc
{
    public class CustomCacheOutputAttribute : CacheOutputAttribute
    {
        //Copy&Pasted OnActionExecutedAsync needs these, because of [*]
#region Copy&Paste from CacheOutputAttribute, as is
        private IApiOutputCache _webApiCache;
        private const string CurrentRequestMediaType = "CacheOutput:CurrentRequestMediaType";

        private static void SetEtag(HttpResponseMessage message, string etag)
        {
            if (etag != null)
            {
                var eTag = new EntityTagHeaderValue(@"""" + etag.Replace("\"", string.Empty) + @"""");
                message.Headers.ETag = eTag;
            }
        }

        protected override void EnsureCache(HttpConfiguration config, HttpRequestMessage req)
        {
            _webApiCache = config.CacheOutputConfiguration().GetCacheOutputProvider(req);
            if (_webApiCache is CustomCacheProvider)
                ((CustomCacheProvider)_webApiCache).ExpirationMode = this.ExpirationMode;
        }

#endregion


        protected IAPILogger APILogger =>
        GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IAPILogger)) as IAPILogger;

        private const string _defaultCustomTimeSpanMethodName = "Execute";

        public Type CustomTimeSpanMethodClassType { get; set; }
        public string CustomTimeSpanMethodName { get; set; }
        public bool CachePerUser { get; set; }
        public ExpirationMode ExpirationMode { get; set; }
		public string ApiName { get; set; }
        public bool LogEnabled { get; set; }
        
        private readonly Guid _id = Guid.NewGuid();                

        private static Dictionary<string, MambaDefinedShortTime> _mambaDefinedShortTimeObjects = new Dictionary<string, MambaDefinedShortTime>();

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            log4net.LogManager.GetLogger(this.GetType()).Info("start: OnActionExecutedAsync");

            if (CustomTimeSpanMethodClassType != null)
            {
                MambaDefinedShortTime mambaDefinedShortTimeObject = null;

                var requestLocalPath = actionExecutedContext.Request.RequestUri.LocalPath;

                mambaDefinedShortTimeObject = _mambaDefinedShortTimeObjects.FirstOrDefault(x => string.Compare(x.Key, requestLocalPath, true)==0).Value;
                if (mambaDefinedShortTimeObject == null)
                {
                    if (string.IsNullOrWhiteSpace(CustomTimeSpanMethodName)) CustomTimeSpanMethodName = _defaultCustomTimeSpanMethodName;
                    var methodInfo = CustomTimeSpanMethodClassType.GetMethod(CustomTimeSpanMethodName);
                    if (methodInfo != null)
                    {
                        mambaDefinedShortTimeObject = new MambaDefinedShortTime(actionExecutedContext, methodInfo);
                        _mambaDefinedShortTimeObjects.Add(requestLocalPath, mambaDefinedShortTimeObject);
                    }
                }

                if (mambaDefinedShortTimeObject != null)
                {
                    var _cacheTimeQuery = this.GetType().GetField("CacheTimeQuery", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    _cacheTimeQuery.SetValue(this, mambaDefinedShortTimeObject);
                }
            }

            //Copy&Paste the OnActionExecutedAsync, instead of calling base.OnActionExecutedAsync(), because of [*]
#region Copy&Paste from CacheOutputAttribute, as is

            if (actionExecutedContext.ActionContext.Response == null || !actionExecutedContext.ActionContext.Response.IsSuccessStatusCode) return;

            if (!IsCachingAllowed(actionExecutedContext.ActionContext, AnonymousOnly)) return;

            var cacheTime = CacheTimeQuery.Execute(DateTime.UtcNow);
            if (cacheTime.AbsoluteExpiration > DateTime.UtcNow || ExpirationMode == ExpirationMode.None)
            {
                var httpConfig = actionExecutedContext.Request.GetConfiguration();
                var config = httpConfig.CacheOutputConfiguration();

                var cacheKeyGenerator = config.GetCacheKeyGenerator(actionExecutedContext.Request, typeof(CustomCacheKeyGenerator));
                if (cacheKeyGenerator is CustomCacheKeyGenerator)
                {
                    ((CustomCacheKeyGenerator)cacheKeyGenerator).CachePerUser = CachePerUser;
                    ((CustomCacheKeyGenerator)cacheKeyGenerator).ApiName = ApiName;
                }

                var responseMediaType = actionExecutedContext.Request.Properties[CurrentRequestMediaType] as MediaTypeHeaderValue ?? GetExpectedMediaType(httpConfig, actionExecutedContext.ActionContext);
                var cachekey = cacheKeyGenerator.MakeCacheKey(actionExecutedContext.ActionContext, responseMediaType, ExcludeQueryStringFromCacheKey);

                // Why do we check if it already exists in cache?
                //if (!string.IsNullOrWhiteSpace(cachekey) && !(_webApiCache.Contains(cachekey)))
                if (!string.IsNullOrWhiteSpace(cachekey))
                {
                    SetEtag(actionExecutedContext.Response, CreateEtag(actionExecutedContext, cachekey, cacheTime));

                    var responseContent = actionExecutedContext.Response.Content;

                    if (responseContent != null)
                    {
                        var baseKey = config.MakeBaseCachekey(actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerType.FullName, actionExecutedContext.ActionContext.ActionDescriptor.ActionName);
                        var contentType = responseContent.Headers.ContentType;
                        string etag = actionExecutedContext.Response.Headers.ETag.Tag;

                        var content = await responseContent.ReadAsByteArrayAsync();//.ConfigureAwait(false);  [*]

                        responseContent.Headers.Remove("Content-Length");

                        if (_webApiCache is CustomCacheProvider)
                            ((CustomCacheProvider)_webApiCache).ExpirationTimeSpan = cacheTime.ClientTimeSpan;

                        _webApiCache.Add(baseKey, string.Empty, cacheTime.AbsoluteExpiration);
                        _webApiCache.Add(cachekey, content, cacheTime.AbsoluteExpiration, baseKey);


                        _webApiCache.Add(cachekey + Constants.ContentTypeKey,
                                        contentType,
                                        cacheTime.AbsoluteExpiration, baseKey);


                        _webApiCache.Add(cachekey + Constants.EtagKey,
                                        etag,
                                        cacheTime.AbsoluteExpiration, baseKey);
                    }
                }
            }

            ApplyCacheHeaders(actionExecutedContext.ActionContext.Response, cacheTime);
#endregion

            log4net.LogManager.GetLogger(this.GetType()).Info("end: OnActionExecutedAsync");

        }// end OnActionExecutedAsync()



        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            log4net.LogManager.GetLogger(this.GetType()).Info("start: OnActionExecuting");

            if (actionContext == null) throw new ArgumentNullException("actionContext");

            if (!IsCachingAllowed(actionContext, AnonymousOnly)) return;

            var timer = (Stopwatch)actionContext.Request.Properties["logtimer"];

            if (timer == null)
            {
                timer = new Stopwatch();
                timer.Start();
            }

            var config = actionContext.Request.GetConfiguration();

            EnsureCacheTimeQuery();
            EnsureCache(config, actionContext.Request);

			var cacheKeyGenerator = config.CacheOutputConfiguration().GetCacheKeyGenerator(actionContext.Request, typeof(CustomCacheKeyGenerator));
			if (cacheKeyGenerator is CustomCacheKeyGenerator)
			{
				((CustomCacheKeyGenerator)cacheKeyGenerator).CachePerUser = CachePerUser;
				((CustomCacheKeyGenerator)cacheKeyGenerator).ApiName = ApiName;
			}

            var responseMediaType = GetExpectedMediaType(config, actionContext);
            actionContext.Request.Properties[CurrentRequestMediaType] = responseMediaType;
            var cachekey = cacheKeyGenerator.MakeCacheKey(actionContext, responseMediaType, ExcludeQueryStringFromCacheKey);

            if (!_webApiCache.Contains(cachekey)) return;

            if (actionContext.Request.Headers.IfNoneMatch != null)
            {
                var etag = _webApiCache.Get<string>(cachekey + Constants.EtagKey);
                if (etag != null)
                {
                    if (actionContext.Request.Headers.IfNoneMatch.Any(x => x.Tag == etag))
                    {
                        var time = CacheTimeQuery.Execute(DateTime.UtcNow);
                        var quickResponse = actionContext.Request.CreateResponse(HttpStatusCode.NotModified);
                        ApplyCacheHeaders(quickResponse, time);
                        actionContext.Response = quickResponse;

                        // Log Api Access
                        if (LogEnabled && !(bool)actionContext.Request.Properties["requestIsLogged"])
                        {
                            timer.Stop();
                            APILogger?.LogExposedAPIAccess(_id, actionContext, timer.Elapsed, true);
                            actionContext.Request.Properties["requestIsLogged"] = true;
                        }

                        return;
                    }
                }
            }

            var val = _webApiCache.Get<byte[]>(cachekey);
            if (val == null) return;

            var contenttype = _webApiCache.Get<MediaTypeHeaderValue>(cachekey + Constants.ContentTypeKey) ?? responseMediaType;

            actionContext.Response = actionContext.Request.CreateResponse();
            actionContext.Response.Content = new ByteArrayContent(val);

            actionContext.Response.Content.Headers.ContentType = contenttype;

            var responseEtag = _webApiCache.Get<string>(cachekey + Constants.EtagKey);
            if (responseEtag != null)
            {
                SetEtag(actionContext.Response, responseEtag);
            }

            var cacheTime = CacheTimeQuery.Execute(DateTime.UtcNow);
            ApplyCacheHeaders(actionContext.Response, cacheTime);
            log4net.LogManager.GetLogger(this.GetType()).Info("end: OnActionExecuting");

            // Log Api Access
            if (LogEnabled && !(bool)actionContext.Request.Properties["requestIsLogged"])
            {
                timer.Stop();
                APILogger?.LogExposedAPIAccess(_id, actionContext, timer.Elapsed, true);
                actionContext.Request.Properties["requestIsLogged"] = true;
            }
        }

        protected override bool IsCachingAllowed(HttpActionContext actionContext, bool anonymousOnly)
        {
            if (anonymousOnly)
            {
                if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
                {
                    return false;
                }
            }

            if (actionContext.ActionDescriptor.GetCustomAttributes<IgnoreCacheOutputAttribute>().Any())
            {
                return false;
            }

            return true;
        }// end IsCachingAllowed()  
    }
}
#endif