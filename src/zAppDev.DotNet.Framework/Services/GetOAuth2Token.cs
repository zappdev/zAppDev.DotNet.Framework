// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Services;
#if NETFRAMEWORK
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif

namespace Services
{
    public class GetOAuth2Token
    {
        public static OAuth2TokenData GetSessionToken(string serviceName, RestServiceConsumptionOptions options,
            HttpContext httpContext)
        {
            try
            {
                //if (serviceName == null)
                //{
                //    return null;
                //}

                var sessionOAuth2TokenData = OAuth2SessionData<OAuth2TokenData>.Get(serviceName);

                if (sessionOAuth2TokenData != null)
                {
                    if (!sessionOAuth2TokenData.ForceRefreshToken)
                    {
                        return sessionOAuth2TokenData;
                    }
                }

                var res = 0;
                Dictionary<string, string> dictParams = null;
                var refreshTokenWasUsed = false;
                if (sessionOAuth2TokenData == null)
                {
                    sessionOAuth2TokenData = new OAuth2TokenData();
                    if (options.oAuth2GrantType == OAuth2GrantType.WebServer)
                    {
                        var sessionAuth2Code = OAuth2SessionData<OAuth2Code>.Get(serviceName)?.Code;

                        if (sessionAuth2Code == null)
                        {
                            OAuth2SessionData<OAuth2ReturnUrl>.Set(serviceName,
                                new OAuth2ReturnUrl(zAppDev.DotNet.Framework.Utilities.Web.GetRequestUri().ToString()));
                            var simpleTask = Task.Run(() => { res = GetWebServerAuthorization(options, httpContext); });
                            simpleTask.Wait();

                            return null;
                        }

                        dictParams = new Dictionary<string, string>
                        {
                            {"grant_type", "authorization_code"},
                            {"code", sessionAuth2Code},
                            {"client_id", options.ClientId},
                            {"client_secret", options.ClientSecret},
                            //{"scope", ""},
                            {"redirect_uri", options.CallBackUrl}
                        };
                    }
                    else if (options.oAuth2GrantType == OAuth2GrantType.Password)
                    {
                        dictParams = new Dictionary<string, string>
                        {
                            {"grant_type", "password"},
                            {"client_id", options.ClientId},
                            {"client_secret", options.ClientSecret},
                            {"username", options.UserName},
                            {"password", options.Password}
                        };
                    }
                }
                else
                {
                    sessionOAuth2TokenData.ForceRefreshToken = false;
                    OAuth2SessionData<OAuth2TokenData>.Set(serviceName, sessionOAuth2TokenData);

                    refreshTokenWasUsed = true;

                    dictParams = new Dictionary<string, string>
                    {
                        {"grant_type", "refresh_token"},
                        {"refresh_token", sessionOAuth2TokenData.Refresh_token},
                        {"client_id", options.ClientId},
                        {"client_secret", options.ClientSecret}
                    };
                }

                var task = Task.Run(async () =>
                {
                    res = await GetOAuth2Token.GetAuthToken(
                        options.AccessTokenUrl,
                        dictParams,
                        sessionOAuth2TokenData);
                });
                task.Wait();


                if (res != 0)
                {
                    if (refreshTokenWasUsed)
                    {
                        OAuth2SessionData<OAuth2TokenData>.Initialize(serviceName);
                        OAuth2SessionData<OAuth2Code>.Initialize(serviceName);
                    }

                    return null;
                }

                OAuth2SessionData<OAuth2TokenData>.Set(serviceName, sessionOAuth2TokenData);
                return sessionOAuth2TokenData;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static void InitializeTokens(RestServiceConsumptionOptions options, string serviceName)
        {
            var sessionOAuth2TokenData = OAuth2SessionData<OAuth2TokenData>.Get(serviceName);

            switch (options.oAuth2GrantType)
            {
                case OAuth2GrantType.Password:
                    if (sessionOAuth2TokenData.Refresh_token == null)
                    {
                        OAuth2SessionData<OAuth2TokenData>.Initialize(serviceName);
                    }
                    else
                    {
                        sessionOAuth2TokenData.ForceRefreshToken = true;
                        OAuth2SessionData<OAuth2TokenData>.Set(serviceName, sessionOAuth2TokenData);
                    }

                    break;
                case OAuth2GrantType.WebServer:
                    if (sessionOAuth2TokenData.Refresh_token == null)
                    {
                        OAuth2SessionData<OAuth2TokenData>.Initialize(serviceName);
                        OAuth2SessionData<OAuth2Code>.Initialize(serviceName);
                    }
                    else
                    {
                        sessionOAuth2TokenData.ForceRefreshToken = true;
                        OAuth2SessionData<OAuth2TokenData>.Set(serviceName, sessionOAuth2TokenData);
                    }

                    break;
            }
        }

        public static OAuth2TokenData GetAuthToken(
            RestServiceConsumptionOptions options,
            string serviceName,
            HttpContext httpContext)
        {
            OAuth2TokenData sessionOAuth2TokenData = null;
            switch (options.oAuth2GrantType)
            {
                case OAuth2GrantType.Password:
                    return GetSessionToken(serviceName, options, httpContext);
                case OAuth2GrantType.WebServer:
                    sessionOAuth2TokenData = GetSessionToken(serviceName, options, httpContext);
                    if (sessionOAuth2TokenData == null)
                    {
                        throw new ApplicationException("sessionAuth2TokenData == null, " + options.oAuth2GrantType);
                    }

                    return sessionOAuth2TokenData;
                default:
                    throw new ApplicationException("Unknown OAuth2GrantType: " + options.oAuth2GrantType);
            }
        }


        public static async Task<int> GetAuthToken(
            string accessTokenUrl,
            Dictionary<string, string> dict,
            OAuth2TokenData oAuth2TokenData
        )
        {
            HttpClient authClient = null;
            HttpResponseMessage message = null;
            try
            {
                authClient = new HttpClient();

                using (HttpContent content = new FormUrlEncodedContent(dict))
                {
                    message = await authClient.PostAsync(accessTokenUrl, content);

                    if (message.StatusCode == HttpStatusCode.OK)
                    {
                        var responseString = await message.Content.ReadAsStringAsync();
                        oAuth2TokenData.Parse(responseString);
                        return 0;
                    }

                    return 1;
                }
            }
            catch (Exception)
            {
                return 2;
            }
            finally
            {
                message?.Dispose();
                authClient?.Dispose();
            }
        }


        private static int GetWebServerAuthorization(RestServiceConsumptionOptions options, HttpContext httpContext)
        {
            var authURI = new StringBuilder();
            authURI.Append(options.AuthorizationURL + "?");
            authURI.Append("response_type=code");
            authURI.Append("&client_id=" + options.ClientId);
            authURI.Append("&redirect_uri=" + options.CallBackUrl);

            if (!string.IsNullOrEmpty(options.Scope))
            {
                authURI.Append("&scope=" + options.Scope);
            }

            //Check it with Salesforce
            authURI.Append("&access_type=" + "offline");

            httpContext.Response.Redirect(authURI.ToString());


            //Object obj = null;
            //do
            //{
            //    Thread.Sleep(500);
            //    obj = httpContext.Session[GetSessionCodeTokenKeyName(serviceName)];
            //} while (obj == null);


            return 1;
        }
    }
}