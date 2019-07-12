#if NETFRAMEWORK
#else
using CLMS.Framework.Utilities;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;

namespace CLMS.Framework.Mvc.UI
{

    namespace Helper
    {
        public class Routes
        {
            public static Dictionary<string, string> GetRouteData()
            {
                var result = new Dictionary<string, string>();

                var routedData = Web.GetContext().GetRouteData();
                if (routedData != null)
                {
                    foreach (var key in routedData.Values.Keys)
                    {
                        var value = routedData.Values[key];
                        result.Add(key, value?.ToString());
                    }
                }

                return result;
            }
        }

        public static class RulesHelper
        {
            public static HtmlString Attribute(string name, string value)
            {
                return new HtmlString($"{name}=\"{value}\"");
            }
        }
    }
}
#endif