#if NETFRAMEWORK
using zAppDev.DotNet.Framework.Mvc;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using WebApiThrottle;
using WebApiThrottle.Models;

namespace zAppDev.DotNet.Framework.WebApi
{
    public class ExternalTableRateEnrty
    {
        public string ProductName
        {
            get;
            set;
        }
        public NullableRateLimits Limits
        {
            get;
            set;
        }
        public string ClientSecret
        {
            get;
            set;
        }
        public string ClientName
        {
            get;
            set;
        }
    }

    public class NullableRateLimits
    {
        public long? PerSecond
        {
            get;
            set;
        }
        public long? PerMinute
        {
            get;
            set;
        }
        public long? PerHour
        {
            get;
            set;
        }
        public long? PerDay
        {
            get;
            set;
        }
        public long? PerWeek
        {
            get;
            set;
        }

        public RateLimits NotNullable()
        {
            return new RateLimits
            {
                PerSecond = PerSecond.GetValueOrDefault(),
                PerMinute = PerMinute.GetValueOrDefault(),
                PerHour = PerHour.GetValueOrDefault(),
                PerDay = PerDay.GetValueOrDefault(),
                PerWeek = PerWeek.GetValueOrDefault(),
            };
        }
    }

    public class RedisRateTableThrottling : IExternalRateTable
    {
        public RateLimits Get(HttpRequestMessage request, RequestIdentity identity)
        {
            return GetEntry(request, identity)?.Limits?.NotNullable();
        }

        public ExternalTableRateEnrty GetEntry(HttpRequestMessage request, RequestIdentity identity = null)
        {
            return zAppDev.DotNet.Framework.Utilities.ApplicationCache.Get<List<ExternalTableRateEnrty>>(GetRequestKey(request, identity))?.FirstOrDefault();
        }

        public string GetRequestKey(HttpRequestMessage request, RequestIdentity identity = null)
        {
            if (identity == null)
            {
                identity = new RequestIdentity
                {
                    ClientId = GetClientId(request),
                    Endpoint = System.Web.HttpContext.Current?.Request?.Path?.ToLower()
                };
            }
            var endpoint = identity.Endpoint.ToLower();
            var appPath = System.Web.HttpContext.Current?.Request?.ApplicationPath?.ToLower();
            if (!string.IsNullOrWhiteSpace(appPath) && appPath != "/")
            {
                endpoint = endpoint.Replace(appPath, "");
            }
            if (!endpoint.StartsWith("/"))
            {
                endpoint = "/" + endpoint;
            }
            return $"{identity.ClientId}|{endpoint}|{request.Method.Method.ToLower()}";
        }

        public string GetClientId(HttpRequestMessage request)
        {
            return request.Headers.Contains("X-CLMSAPI-ClientId") ? request.Headers.GetValues("X-CLMSAPI-ClientId").First() : "";
        }

        public void UpdateIdentity(RequestIdentity identity, HttpRequestMessage request)
        {
            identity.ClientId = GetClientId(request);
            identity.ProductName = GetEntry(request)?.ProductName + "_" + BaseViewPageBase<string>.AppVersion;
        }

        public string GetClientSecret(HttpRequestMessage request)
        {
            return request.Headers.Contains("X-CLMSAPI-ClientSecret") ? request.Headers.GetValues("X-CLMSAPI-ClientSecret").First() : "";
        }
    }
}
#endif