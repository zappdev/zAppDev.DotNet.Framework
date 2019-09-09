// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace zAppDev.DotNet.Framework.Mvc
{
    public class AntiXssConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            try
            {
                if (token.Type == JTokenType.Null) return null;
                var rawValue = token?.ToString();
                return Utilities.StringContainsEncodedHtml(rawValue)
                       ? HttpUtility.HtmlDecode(rawValue)
                       : rawValue;
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(typeof(AntiXssConverter)).Error($"Could not parse token {token?.ToString()}!", e);
                return null;
            }
            throw new JsonSerializationException("Unexpected token type: " + token.Type.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
#if NETFRAMEWORK
            var str = value?.ToString();
            if (Utilities.StringContainsHtml(str))
            {
                writer.WriteValue(System.Web.Security.AntiXss.AntiXssEncoder.HtmlEncode(str, false));
            }
            else
            {
                writer.WriteValue(str);
            }
#else
            var str = value?.ToString();
            if (Utilities.StringContainsHtml(str))
            {
                writer.WriteValue(HttpUtility.HtmlDecode(str));
            }
            else
            {
                writer.WriteValue(str);
            }
#endif
        }
    }

    
}
