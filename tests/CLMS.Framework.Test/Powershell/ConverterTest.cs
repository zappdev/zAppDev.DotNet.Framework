using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace CLMS.Framework.Test.Powershell
{
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
            
        }
        
        [TestMethod]
        public void DeserializeTest()
        {
            
        }

        [TestMethod]
        public void Convert()
        {
        }

        [TestMethod]
        public void SingleObject()
        {
        }
    }   
}
