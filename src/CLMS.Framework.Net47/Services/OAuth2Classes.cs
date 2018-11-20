using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CLMS.Framework.Utilities;
using Newtonsoft.Json.Linq;

namespace Services
{
    public class OAuth2TokenData
    {
        public string Token;
        public string Refresh_token;
        public string ServiceUrl;
        public string Token_type;

        public bool ForceRefreshToken;

        public OAuth2TokenData()
        {
            this.Token = null;
            this.Refresh_token = null;
            this.ServiceUrl = null;
            this.Token_type = "";
            this.ForceRefreshToken = false;
        }


        public void Parse(string postRresponseString)
        {
            JObject obj = JObject.Parse(postRresponseString);
            this.Token = (string) obj["access_token"];
            if (this.Refresh_token == null)
            {
                this.Refresh_token = (string) obj["Refresh_token"];
            }
            this.ServiceUrl = (string) obj["instance_url"];
            this.Token_type = (string) obj["token_type"];
        }

    }

    public class OAuth2ReturnUrl
    {
        public string ReturnUrl;

        public OAuth2ReturnUrl(string returnUrl)
        {
            this.ReturnUrl = returnUrl;
        }
    }


    public class OAuth2Code
    {
        public string Code;

        public OAuth2Code(string code)
        {
            this.Code = code;
        }
    }


    public class OAuth2SessionData<T>
    {
        private static string SessionKey(string serviceName)
        {
            if (serviceName != null)
            {
                return typeof (T).Name + "_" + serviceName;
            }
            return null;
        }


        public static T Get(string serviceName)
        {
            try
            {
                if (serviceName != null)
                {
                    return (T)Web.Session.Get(SessionKey(serviceName));
                }
                return default(T);
            }
            catch (Exception)
            {
                return default(T);
            }
        }


        public static T Get(HttpContext httpContext, string serviceName)
        {
            try
            {
                if (serviceName != null)
                {
                    return (T)httpContext.Session[SessionKey(serviceName)];
                }
                return default(T);
            }
            catch (Exception)
            {
                return default(T);
            }
        }


        public static void Set(string serviceName, T tobj)
        {
            try
            {
                if (serviceName != null)
                {
                    Web.Session.Set(SessionKey(serviceName), tobj);
                }
            }
            catch (Exception)
            {

            }
        }


        public static void Set(HttpContext httpContext, string serviceName, T tobj)
        {
            try
            {
                if (serviceName != null)
                {
                    httpContext.Session[SessionKey(serviceName)] = tobj;
                }
            }
            catch (Exception)
            {
            }
        }

        public static void Initialize(string serviceName)
        {
            Set(serviceName, default(T));
        }

        public static void Initialize(HttpContext httpContext, string serviceName)
        {
            Set(httpContext,serviceName, default(T));
        }




    }


}
