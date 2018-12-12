using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System;
using System.Text;

namespace CLMS.Framework.Utilities
{
    public class Serializer<T>
    {
        public class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter(StringBuilder sb) : base(sb)
            {
            }

            public override Encoding Encoding => Encoding.UTF8;
        }

        public string ToJson(T instance, bool preventCircles = true, bool ignoreNullValues = false)
        {
            return JsonConvert.SerializeObject(instance, new JsonSerializerSettings
            {
                PreserveReferencesHandling = preventCircles ?
                    PreserveReferencesHandling.All : PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = ignoreNullValues ?
                    NullValueHandling.Ignore : NullValueHandling.Include,
                ContractResolver = new NHibernateContractResolver(true),
                //DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
            });
        }

        public T FromJson(string data)
        {
            return JsonConvert.DeserializeObject<T>(data,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
        }

        public string ToXml(T instance, bool utf8 = false)
        {
            string result = string.Empty;
            if (instance != null)
            {
                var serializer = new XmlSerializer(instance.GetType());
                var sb = new StringBuilder();

                using (var writer = (utf8 ? new Utf8StringWriter(sb) : new StringWriter(sb)))
                {
                    serializer.Serialize(writer, instance);
                    result = sb.ToString();
                }
            }
            return result;
        }

        public T FromXml(string data)
        {
            T result = default(T);
            if (!string.IsNullOrEmpty(data))
            {
                using (var reader = new StringReader(data))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    result = (T)serializer.Deserialize(reader);
                }
            }
            return result;
        }

        public T ParseEnum(string data)
        {
            if (string.IsNullOrEmpty(data)) return default(T);
            return (T)Convert.ChangeType(Enum.Parse(typeof(T), data), typeof(T));
        }
    }
}
