// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using System.Reflection;
using zAppDev.DotNet.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Utilities
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
