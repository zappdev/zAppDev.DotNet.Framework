﻿// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.SecurityDataDtos;
using zAppDev.DotNet.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace zAppDev.DotNet.Framework.Tests.Utilities
{
    [TestClass]
    public class SerializerTest
    {
        [TestMethod]
        public void SerializeTest()
        {
            var operation = new ApplicationPermission
            {
                Id = 0,
                IsCustom = true,
                Name = "name"
            };

            var serializer = new Serializer<ApplicationPermission>();

            var jsonStr = serializer.ToJson(operation);
            var xmlStr = serializer.ToXml(operation);

            Assert.AreEqual(operation.Name, serializer.FromJson(jsonStr).Name);
            Assert.AreEqual(operation.Name, serializer.FromXml(xmlStr).Name);
            
            jsonStr = serializer.ToJson(operation, false, true);
            xmlStr = serializer.ToXml(operation, true);

            Assert.AreEqual(operation.Name, serializer.FromJson(jsonStr).Name);
            Assert.AreEqual(operation.Name, serializer.FromXml(xmlStr).Name);

            var enumSerializer = new Serializer<AppDevSemantic>();

            Assert.AreEqual(AppDevSemantic.CalculatedExpression, enumSerializer.ParseEnum("CalculatedExpression"));
            Assert.AreEqual(AppDevSemantic.None, enumSerializer.ParseEnum(null));

            var ioStream = new Serializer<object>.Utf8StringWriter(new StringBuilder());

            Assert.AreEqual(Encoding.UTF8, ioStream.Encoding);

        }
    }
}
