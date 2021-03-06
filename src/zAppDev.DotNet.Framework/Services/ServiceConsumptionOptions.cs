﻿// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using CacheManager.Core;
using System.Collections.Generic;

#if NETFRAMEWORK
using System.Web.Http.Controllers;
#endif

namespace zAppDev.DotNet.Framework.Services
{
    public class ServiceConsumptionOptions : IServiceConsumptionOptions, ICachedServiceConsumptionOptions
    {
        public string Url { get; set; }
        public RestHTTPVerb Verb { get; set; }        
        public string UserName { get; set; }
        public string ClientId { get; set; }
        public string AccessTokenUrl { get; set; }
        public string CallBackUrl { get; set; }
        public object Data { get; set; }
        public Dictionary<string, object> FormData { get; set; }
        public Dictionary<string, string> ExtraHeaderData { get; set; }
        public bool LogAccess { get; set; }
        public bool IsCachingEnabled { get; set; }
        public bool IgnoreNullValues { get; set; } = true;

        public string ProxyAddress { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUser { get; set; }
        public string ProxyPassword { get; set; }

        /// <summary>
        /// Name of the invoked API. E.g. "BlogAPI"
        /// </summary>
        public string ApiName { get; set; }

        /// <summary>
        /// Sets whether or not to use the current user's UserName in the Cache Key
        /// </summary>
        public bool CachePerUser { get; set; }

#if NETFRAMEWORK
        /// <summary>
        /// The running Http Context. Can be null, when running outside of a web application
        /// </summary>
        public HttpActionContext ActionExecutedContext { get; set; }
#endif

        /// <summary>
        /// If true, will not use the URI Query as part of the Cache Key <![CDATA[(e.g. ?pageIndex=0&&fetchRows=25)]]>
        /// </summary>
        public bool ExcludeQueryStringFromCacheKey { get; set; }

        /// <summary>
        /// Defines the supported expiration modes for cache items. Value None will indicate that no expiration should be set.
        /// </summary>
        public ExpirationMode ExpirationMode { get; set; }

        /// <summary>
        /// How long the response should be cached on the server side, in seconds. The same value will be passed as the ClientTimeSpan as well.
        /// </summary>
        public int? ServerTimeSpan { get; set; }

        public string Operation { get; set; }

        public string Arguments { get; set; }
    }
}