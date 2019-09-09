// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using zAppDev.DotNet.Framework.Utilities;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Mvc.UI.Helper
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
#endif