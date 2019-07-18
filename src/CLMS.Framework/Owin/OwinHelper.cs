#if NETFRAMEWORK
using System.Web;
using Microsoft.Owin;
using System.Collections.Generic;

namespace CLMS.Framework.Owin
{
    public class OwinHelper
    {
        public static readonly string OwinEnvironmentKey = "owin.Environment";

        private static IDictionary<string, object> GetOwinEnvironment(HttpContext context)
        {
            context = context ?? HttpContext.Current;
            return (IDictionary<string, object>)context?.Items[OwinEnvironmentKey];
        }

        public static IOwinContext GetOwinContext(HttpContext context = null)
        {
            var environment = GetOwinEnvironment(context);

            if (environment == null)
            {
                return null;
            }

            return new OwinContext(environment);
        }
    }
}

#endif