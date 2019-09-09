// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Services;
#if NETFRAMEWORK
using System.Web;
#else
using Microsoft.AspNetCore.Http;

#endif


namespace zAppDev.DotNet.Framework.Services
{
    public class OAuth2InvalidToken
    {
    }


    public enum RestResultType
    {
        JSON,
        XML,
        STRING,
        NONE
    }

    public enum RestHTTPVerb
    {
        GET,
        POST,
        DELETE,
        PUT,
        NONE
    }

    public enum PostType
    {
        UrlEncoded,
        XML,
        JSON,
        Text
    }

    public enum RestSecurityType
    {
        NoAuth,
        BasicAuth,
        OAuth2
    }


    public enum OAuth2GrantType
    {
        Password,
        WebServer
    }


    public class RestServiceConsumptionOptions : ServiceConsumptionOptions, IRestServiceConsumptionOptions
    {
        public RestResultType Type { get; set; }
        public PostType PostType { get; set; }
        public RestSecurityType SecurityType { get; set; }
        public OAuth2GrantType oAuth2GrantType { get; set; }
        public string Password { get; set; }
        public string ClientSecret { get; set; }
        public string AuthorizationURL { get; set; }
        public string Scope { get; set; }
    }

    public class RestServiceConsumer
    {
        private static IEnumerable<KeyValuePair<string, string>> ConvertNameValueCollectionToKeyValuePair(
            NameValueCollection input)
        {
            return input.AllKeys.Select(key => new KeyValuePair<string, string>(key, input[key]));
        }


        private static string GetServiceName<T>()
        {
            try
            {
                string[] fullnameParts = typeof(T)?.FullName?.Split('.');
                if (fullnameParts != null && fullnameParts.Length > 1) //to avoid exception
                {
                    return fullnameParts[fullnameParts.Length - 2];
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static object Consume<T>(RestServiceConsumptionOptions options, ServiceConsumptionContainer resultBag)
        {
            string serviceName = GetServiceName<T>();

            int retries = 0;
            Object obj = InnerConsume<T>(options, serviceName, retries++, resultBag);

            if (obj != null && obj.GetType() == typeof(OAuth2InvalidToken))
            {
                GetOAuth2Token.InitializeTokens(options, serviceName);
                return (InnerConsume<T>(options, serviceName, retries, resultBag));
            }

            return obj;
        }


        public static object InnerConsume<T>(RestServiceConsumptionOptions options, string serviceName, int retries,
            ServiceConsumptionContainer resultBag)
        {
            OAuth2TokenData currentOAuth2TokenData = null;
            if (options.SecurityType == RestSecurityType.OAuth2)
            {
                currentOAuth2TokenData = GetOAuth2Token.GetAuthToken(options, serviceName, Utilities.Web.GetContext());
            }

            using (var client = GetHttpClient(options, currentOAuth2TokenData))
            {          
                switch (options.Verb)
                {
                    case RestHTTPVerb.GET:
                        resultBag.HttpResponseMessage = client.GetAsync("").Result;
                        break;
                    case RestHTTPVerb.POST:

                        switch (options.PostType)
                        {
                            case PostType.JSON:
								var jsonSerialized =
 new Utilities.Serializer<object>().ToJson(options.Data, false, options.IgnoreNullValues);
                                resultBag.HttpResponseMessage = client
                                        .PostAsync("", new StringContent(jsonSerialized, Encoding.UTF8, "application/json"))
                                        .Result;

                                if (!resultBag.HttpResponseMessage.IsSuccessStatusCode)
                                {
                                    log4net.LogManager.GetLogger(typeof(RestServiceConsumer)).Error(jsonSerialized);
                                }
                                //response = client.PostAsJsonAsync("", options.Data).Result;
                                break;

                            case PostType.XML:
                                var xml = new zAppDev.DotNet.Framework.Utilities.Serializer<object>().ToXml(options.Data, true);
                                var httpContent = new StringContent(xml, Encoding.UTF8, "application/xml");
                                resultBag.HttpResponseMessage = client.PostAsync("", httpContent).Result;
                                break;
                            default:
                                /*
                                                                    HttpContent content
                                                                        = new FormUrlEncodedContent(
                                                                            ConvertNameValueCollectionToKeyValuePair(
                                                                                HttpUtility.ParseQueryString(options.Data + "")));
                                                                    response = client.PostAsync("", content).Result;
                                    */
                                resultBag.HttpResponseMessage = client.PostAsync("", PrepareFormData(options)).Result;
                                //response = client.PostAsync("", options.Data).Result;
                                break;
                        }
                        /*var str = JsonConvert.SerializeObject(options.Data);

                    var logger = LogManager.GetLogger(typeof (RestServiceConsumer));
                    if (str == "\"" + options.Data + "\"")
                    {

                        logger.Debug("POSTing as FORM");
                        logger.DebugFormat("because: {0} == {1}", str, "\"" + options.Data + "\"");

                        HttpContent content
                            = new FormUrlEncodedContent(
                                ConvertNameValueCollectionToKeyValuePair(
                                    HttpUtility.ParseQueryString(options.Data.ToString())));
                        response = client.PostAsync("", content).Result;
                    }
                    else
                    {
                        logger.Debug("POSTing as JSON");

                        response = client.PostAsJsonAsync("", options.Data).Result;
                    }*/

                        break;
                    case RestHTTPVerb.PUT:
                        switch (options.PostType)
                        {
                            case PostType.JSON:
                                resultBag.HttpResponseMessage = client.PutAsJsonAsync("", options.Data).Result;
                                break;

                            case PostType.XML:
                                resultBag.HttpResponseMessage = client.PutAsXmlAsync("", options.Data).Result;
                                break;

                            default:
                                resultBag.HttpResponseMessage = client.PutAsync("", PrepareFormData(options)).Result;

                                break;
                        }
                        break;
                    case RestHTTPVerb.DELETE:
                        resultBag.HttpResponseMessage = client.DeleteAsync("").Result;
                        break;
                    default:
                        throw new ApplicationException("Uknown Http Verb: " + options.Verb);
                }
            
                if (options.SecurityType == RestSecurityType.OAuth2 && resultBag.HttpResponseMessage.StatusCode == HttpStatusCode.Unauthorized && retries == 0)
                {                    
                    return new OAuth2InvalidToken();
                }
                    
                if (resultBag.HttpResponseMessage.IsSuccessStatusCode)
                {

                    var setCookieHeader =
 resultBag.HttpResponseMessage.Headers.FirstOrDefault(a => a.Key?.ToLower() == "set-cookie");
                    if (Utilities.Web.GetContext() != null && setCookieHeader.Value != null && setCookieHeader.Value.Any())
                    {
                        Utilities.Web.GetContext().Items["ServiceAuthCookie"] = setCookieHeader;
                    }

                    options.Type = RestResultType.STRING;

                    string contentType = resultBag.HttpResponseMessage.Content.Headers.ContentType?.ToString()
                                         ??
                                         (resultBag.HttpResponseMessage.Content.Headers.ToList()
                                             .FirstOrDefault(a => a.Key.ToLower() == "content-type")
                                             .Value?.FirstOrDefault() ?? "");

                    if (contentType.Contains("/json"))
                    {
                        options.Type = RestResultType.JSON;
                    }
                    else if (contentType.Contains("/xml"))
                    {
                        options.Type = RestResultType.XML;
                    }

                    var stringResult = resultBag.HttpResponseMessage.Content.ReadAsStringAsync().Result;
                    if (typeof (T) == typeof (string))
                    {
                        return stringResult;
                    }

                    try
                    {
                        switch (options.Type)
                        {
                            case RestResultType.STRING:
                                return stringResult;
                            case RestResultType.JSON:
                                return JsonConvert.DeserializeObject<T>(stringResult);
                            case RestResultType.XML:
                                using (var stringReader = new StringReader(stringResult))
                                {
                                    var serializer = new XmlSerializer(typeof (T));
                                    return (T) serializer.Deserialize(stringReader);
                                }
                            default:
                                log4net.LogManager.GetLogger(typeof(RestServiceConsumer))
                                    .Warn($"Don't know how to convert response to type: {options.Type}");
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        log4net.LogManager.GetLogger(typeof(RestServiceConsumer))
                                    .Error($"Could not deserialize to type: {options.Type}, returning raw string!", e);

                        return stringResult;
                    }


                    /*switch (options.Type)
                    {
                        case RestResultType.STRING:
                            return response.Content.ReadAsStringAsync().Result;
                        case RestResultType.JSON:
                            object convertedJson;
                            var stream = response.Content.ReadAsStreamAsync().Result;
                            try
                            {
                                convertedJson = response.Content.ReadAsAsync<T>().Result;
                            }
                            catch
                            {
                                stream.Seek(0, SeekOrigin.Begin);
                                convertedJson = response.Content.ReadAsAsync<string>().Result;
                            }
                            return convertedJson;
                        case RestResultType.XML:
                            var s = response.Content.ReadAsStringAsync().Result;
                            if (typeof (T) == typeof (string))
                            {
                                return s;
                            }
                            var stringReader = new StringReader(s);
                            var serializer = new XmlSerializer(typeof (T));
                            return (T) serializer.Deserialize(stringReader);

                    }*/
                    return default(T);
                }


                throw new ApplicationException($"{(int)resultBag.HttpResponseMessage.StatusCode} ({resultBag.HttpResponseMessage.ReasonPhrase})");

            }
        }


        private static HttpContent PrepareFormData(RestServiceConsumptionOptions options)
        {
            var formData = new NameValueCollection();
            foreach (var dato in options.FormData)
            {
                //formData.Add(HttpUtility.ParseQueryString(dato.Key + " " + dato.Value));
                formData.Add(dato.Key, dato.Value?.ToString() ?? "");
            }

            return new FormUrlEncodedContent(ConvertNameValueCollectionToKeyValuePair(formData));
        }

        private static HttpClient GetHttpClient(RestServiceConsumptionOptions options,
            OAuth2TokenData currentOAuth2TokenData)
        {
            var handler = new HttpClientHandler();


            if (options.SecurityType == RestSecurityType.BasicAuth)
            {
                if (!string.IsNullOrEmpty(options.UserName))
                {
                    handler.Credentials = new NetworkCredential(options.UserName, options.Password);
                }
            }
            else if (options.SecurityType == RestSecurityType.OAuth2
                     && currentOAuth2TokenData != null && currentOAuth2TokenData.ServiceUrl != null)
            {
                if (!options.Url.StartsWith(currentOAuth2TokenData.ServiceUrl))
                {
                    if (currentOAuth2TokenData.ServiceUrl.EndsWith("/"))
                    {
                        currentOAuth2TokenData.ServiceUrl = currentOAuth2TokenData.ServiceUrl.Substring(0,
                            currentOAuth2TokenData.ServiceUrl.Length - 1);
                    }

                    if (options.Url.StartsWith("/"))
                    {
                        options.Url = options.Url.Substring(1);
                    }

                    options.Url = currentOAuth2TokenData.ServiceUrl + "/" + options.Url;
                }
            }

            if (Utilities.Web.GetContext()?.Items["ServiceAuthCookie"] != null)
            {
                try
                {
                    var setCookieHeader =
                        (KeyValuePair<string, IEnumerable<string>>)Utilities.Web.GetContext().Items["ServiceAuthCookie"];
                    var cookieContainer = new CookieContainer();
                    cookieContainer.SetCookies(new Uri(options.Url), string.Join(",", setCookieHeader.Value));
                    handler.CookieContainer = cookieContainer;
                }
                catch (Exception e)
                {
                    log4net.LogManager.GetLogger(typeof(RestServiceConsumer))
                        .Error($"Could not SetCookie [ServiceAuthCookie]!", e);
                }
            }

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(options.Url)
            };

            if (options.ExtraHeaderData?.ContainsKey("accept") != true)
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            }

            if (options.ExtraHeaderData != null)
            {
                foreach (var eh in options.ExtraHeaderData)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(eh.Key, eh.Value);
                }
            }

            if (options.ExtraHeaderData?.ContainsKey("authorization") != true)
            {
                if (options.SecurityType == RestSecurityType.BasicAuth)
                {
                    var usernameAndPassword = $"{options.UserName}:{options.Password}";
                    var bytes = Encoding.UTF8.GetBytes(usernameAndPassword);
                    usernameAndPassword = Convert.ToBase64String(bytes);
                    client.DefaultRequestHeaders.Add("Authorization", $"Basic {usernameAndPassword}");
                }
                else if (options.SecurityType == RestSecurityType.OAuth2 && currentOAuth2TokenData != null)
                {
                    //Bearer
                    client.DefaultRequestHeaders.Add("Authorization",
                        currentOAuth2TokenData.Token_type + " " + currentOAuth2TokenData.Token);
                    //client.DefaultRequestHeaders.Add("Authorization", sessionAuth2TokenResults.token_type + " " + "lalala");
                }
            }

            return client;
        }

        public static JObject Consume(RestServiceConsumptionOptions options,
            ServiceConsumptionContainer resultBag = null)
        {
            return Consume<object>(options, resultBag) as JObject;
        }
    }
}