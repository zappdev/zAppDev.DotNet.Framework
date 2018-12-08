using CLMS.Framework.Powershell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace CLMS.Framework.Test.Powershell
{
    internal class ParameterObject
    {
        public int Age { get; set; }
    }
    
    [TestClass]
    public class ConvertorTest
    {
        private readonly JsonSerializerSettings _defaultSerializationSettingsWithCycles = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
        };
        
        [TestMethod]
        public void GetJsonSerializerSettingsTest()
        {
            Assert.AreEqual(_defaultSerializationSettingsWithCycles.PreserveReferencesHandling, 
                Convertor.DefaultSerializationSettingsWithCycles.PreserveReferencesHandling);
        }
        
        [TestMethod]
        public void DeserializeTest()
        {
            var obj = Convertor.Deserialize<ParameterObject>(@"{""Age"": 10}");            
            Assert.AreEqual(10, obj.Age);
        }

        [TestMethod]
        public void SingleObject()
        {
        }
    }   
}
