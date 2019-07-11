using CLMS.Framework.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace CLMS.Framework.Mvc
{
    public class DecimalConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal) || objectType == typeof(decimal?) ||
                   objectType == typeof(float) || objectType == typeof(float?) ||
                   objectType == typeof(double) || objectType == typeof(double?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var isNullable = objectType == typeof(decimal?) ||
                             objectType == typeof(double?) ||
                             objectType == typeof(float?);
            try
            {
                if (token.Type == JTokenType.Null && isNullable) return null;
                if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
                {
                    if (objectType == typeof(decimal) || objectType == typeof(decimal?))
                    {
                        return token.ToObject<decimal>();
                    }
                    else if (objectType == typeof(double) || objectType == typeof(double?))
                    {
                        return token.ToObject<double>();
                    }
                    else if (objectType == typeof(float) || objectType == typeof(float?))
                    {
                        return token.ToObject<float>();
                    }
                }
                else if (token.Type == JTokenType.String)
                {
                    var rawValue = token.ToString();
                    if (string.IsNullOrEmpty(rawValue))
                    {
                        if (isNullable)
                        {
                            return null;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    var decimalSeparator = ProfileHelper.GetLocaleDecimalSeparator();
                    var groupSeparator = ProfileHelper.GetLocaleNumberGroupSeparator();
                    // CultureInfo.InvariantCulture uses . as a decimal separator, and , as a thousands separator.
                    // https://msdn.microsoft.com/en-us/library/system.globalization.numberformatinfo.invariantinfo.aspx
                    // 2 steps needed here for avoiding conflicts
                    var sanitizedValue = rawValue.Replace(decimalSeparator, "#DECIMAL#")
                                         .Replace(groupSeparator, "#GROUP#");
                    sanitizedValue = sanitizedValue.Replace("#DECIMAL#", ".").Replace("#GROUP#", "");
                    if (objectType == typeof(decimal) || objectType == typeof(decimal?))
                    {
                        return decimal.Parse(sanitizedValue, NumberStyles.Float, CultureInfo.InvariantCulture);
                    }
                    else if (objectType == typeof(double) || objectType == typeof(double?))
                    {
                        return double.Parse(sanitizedValue, NumberStyles.Float, CultureInfo.InvariantCulture);
                    }
                    else if (objectType == typeof(float) || objectType == typeof(float?))
                    {
                        return float.Parse(sanitizedValue, NumberStyles.Float, CultureInfo.InvariantCulture);
                    }
                }
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(typeof(DecimalConverter))
                .Error($"Could not parse token {token?.ToString()} to number!", e);
                if (isNullable)
                {
                    return null;
                }
                else
                {
                    return 0;
                }
            }
            throw new JsonSerializationException("Unexpected token type: " + token.Type.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken.FromObject(value).WriteTo(writer);
        }
    }
}
