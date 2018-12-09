using System.Collections.Generic;
using System.Reflection;
using CLMS.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Utilities
{
    [TestClass]
    public class MambaRuntimeTypeTest
    {
        public string Property { get; set; }

        public string _myField;

        [TestMethod]
        public void InitTest()
        {
            var info = typeof(MambaRuntimeTypeTest).GetProperty("Property");

            var mambaType = new MambaRuntimeType(info);

            Assert.AreEqual("Property", mambaType.Name);
            Assert.AreEqual(typeof(string), mambaType.PropertyType);
            Assert.AreEqual(info, mambaType.Info);

            mambaType.SetValue(this, "Hello");
            Assert.AreEqual("Hello", mambaType.GetValue(this));

            var field = typeof(MambaRuntimeTypeTest).GetField("_myField");
            mambaType = new MambaRuntimeType(field);

            Assert.AreEqual("_myField", mambaType.Name);
            Assert.AreEqual(typeof(string), mambaType.PropertyType);
            Assert.AreEqual(field, mambaType.Info);

            mambaType.SetValue(this, "Hello");
            Assert.AreEqual("Hello", mambaType.GetValue(this));

            var fields = new [] {info};
            var mambaTypes = MambaRuntimeType.FromPropertiesList(fields);
            Assert.AreEqual(1, mambaTypes.Count);
        }
    }
}
