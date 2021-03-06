﻿// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using CacheManager.Core;
using System.Collections.Generic;

#if NETFRAMEWORK
using System.Web.Http.Controllers;
#endif

namespace zAppDev.DotNet.Framework.Services
{
    public interface IServiceConsumptionOptions
    {
        string AccessTokenUrl { get; set; }
        string CallBackUrl { get; set; }
        string ClientId { get; set; }
        object Data { get; set; }
        Dictionary<string, string> ExtraHeaderData { get; set; }
        Dictionary<string, object> FormData { get; set; }
        string Url { get; set; }
        string UserName { get; set; }
        RestHTTPVerb Verb { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string Operation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string Arguments { get; set; }
    }

    public interface IRestServiceConsumptionOptions
    {
        RestResultType Type { get; set; }
        PostType PostType { get; set; }
        RestSecurityType SecurityType { get; set; }
        OAuth2GrantType oAuth2GrantType { get; set; }
        string Password { get; set; }
        string ClientSecret { get; set; }
        string AuthorizationURL { get; set; }
        string Scope { get; set; }
    }

    public interface ICachedServiceConsumptionOptions
    {
        /// <summary>
        /// Name of the invoked API. E.g. "BlogAPI"
        /// </summary>
        string ApiName { get; set; }

        /// <summary>
        /// Sets whether or not to use the current user's UserName in the Cache Key
        /// </summary>
        bool CachePerUser { get; set; }

#if NETFRAMEWORK
        /// <summary>
        /// The running Http Context. Can be null, when running outside of a web application
        /// </summary>
        HttpActionContext ActionExecutedContext { get; set; }
#endif

        /// <summary>
        /// If true, will not use the URI Query as part of the Cache Key <![CDATA[(e.g. ?pageIndex=0&&fetchRows=25)]]>
        /// </summary>
        bool ExcludeQueryStringFromCacheKey { get; set; }

        /// <summary>
        /// Defines the supported expiration modes for cache items. Value None will indicate that no expiration should be set.
        /// </summary>
        ExpirationMode ExpirationMode { get; set; }

        /// <summary>
        /// How long the response should be cached on the server side, in seconds. The same value will be passed as the ClientTimeSpan as well.
        /// </summary>
        int? ServerTimeSpan { get; set; }
    }
}