using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;

namespace CLMS.Framework.Powershell
{
    public static class Convertor
    {
        public static JsonSerializerSettings DefaultSerializationSettingsWithCycles = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
        };

        public static T Deserialize<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data, DefaultSerializationSettingsWithCycles);
        }

        public static List<T> Convert<T>(this Collection<PSObject> psObjects)
        {
            return psObjects.Select(x => x.Convert<T>()).ToList();
        }

        public static T Convert<T>(this PSObject psObject)
        {
            var serialized = JsonConvert.SerializeObject(psObject.Properties.Where(x => x.Value != null || true).ToDictionary(k => k.Name, v => v.Value));
            var deserialized = JsonConvert.DeserializeObject<T>(serialized);
            return deserialized;
        }

        public static bool SingleObject(this Collection<PSObject> psObjects)
        {
            return psObjects?.Count > 1;
        }

    }
}
