using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using CLMS.Framework.Utilities;
using Moq;
using Newtonsoft.Json.Linq;

namespace CLMS.Framework.Tests.Utilities
{
    public class Item
    {
        public int Value { get; set; }
    }

    [TestClass]
    public class CommonTest
    {
        [TestMethod]
        public void SafeCastTest()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);

            Assert.AreEqual(1, Common.SafeCast<int>((object)1));
            Assert.AreEqual(1, Common.SafeCast<int>((object)"1"));
            Assert.AreEqual(1, Common.SafeCast<int?>((object)"1"));

            Assert.AreEqual(1.2, Common.SafeCast<double>((object)1.2));
            Assert.AreEqual(1.2, Common.SafeCast<double>((object)"1.2"));
            Assert.AreEqual(1.2, Common.SafeCast<double?>((object)"1.2"));

            Assert.IsTrue(Math.Abs(Common.SafeCast<float>((object)1.2) - 1.2) < 0.0001);
            Assert.IsTrue(Math.Abs(Common.SafeCast<float>((object)"1.2") - 1.2) < 0.0001);
            Assert.IsTrue(Math.Abs(Common.SafeCast<float?>((object)"1.2").Value - 1.2) < 0.0001);

            Assert.AreEqual(1, Common.SafeCast<short>((object)1));
            Assert.AreEqual(1, Common.SafeCast<short>((object)"1"));
            Assert.AreEqual((short)1, Common.SafeCast<short?>("1"));

            Assert.AreEqual(1, Common.SafeCast<long>((object)1));
            Assert.AreEqual(1, Common.SafeCast<long>((object)"1"));
            Assert.AreEqual(1, Common.SafeCast<long?>((object)"1"));

            Assert.AreEqual(1, Common.SafeCast<decimal>((object)1));
            Assert.AreEqual(1, Common.SafeCast<decimal>((object)"1"));
            Assert.AreEqual((decimal)1, Common.SafeCast<decimal?>((object)"1"));

            Assert.AreEqual(true, Common.SafeCast<bool>((object)true));
            Assert.AreEqual(true, Common.SafeCast<bool>((object)"true"));
            Assert.AreEqual(true, Common.SafeCast<bool?>((object)"true"));

            var date = DateTime.Parse("2010-01-01T10:10:10Z");
            Assert.AreEqual(date, Common.SafeCast<DateTime>((object)date));
            Assert.AreEqual(date, Common.SafeCast<DateTime>((object)date.ToString()));
            Assert.AreEqual(date, Common.SafeCast<DateTime?>((object)date.ToString()));

            var item = new Item
            {
                Value = 10
            };
            var itemSeriazed = "{\"Value\": 10}";
            Assert.AreEqual(item.Value, Common.SafeCast<Item>((object)item).Value);
            Assert.AreEqual(item.Value, Common.SafeCast<Item>(itemSeriazed).Value);

            itemSeriazed = "<Item><Value>10</Value></Item>";
            Assert.AreEqual(item.Value, Common.SafeCast<Item>(itemSeriazed).Value);

            itemSeriazed = "<ItemError><Value>10</Value></ItemError>";
            Assert.IsNull(Common.SafeCast<Item>(itemSeriazed));

            Assert.AreEqual(0, Common.SafeCast<int>("error"));

            itemSeriazed = "{\"Value\": 10}";

            var o = JObject.Parse(itemSeriazed);
            Assert.AreEqual(item.Value, Common.SafeCast<Item>((object)o).Value);
        }

        [TestMethod]
        public void TryParseTest()
        {
            Assert.IsFalse(Common.TryParseXml<Item>("Error", out var obj));
            Assert.IsFalse(Common.TryParseJson<Item>("Error", out obj));
            Assert.IsFalse(Common.TryParseJson<Item>("{Error}", out obj));
        }

        [TestMethod]
        public void GetExcelFormatTest()
        {
            List<string> columnNames = new List<string> { "Col" };
            List<List<object>> values = new List<List<object>> { new List<object> { "1" } };

            var result = Common.GetExcelFormat(columnNames, values);
            Assert.AreEqual(899, result.Length);        
            Assert.AreEqual("ae0916e8dbdd8aff26e1d70f015a6e29", Common.GetMD5Hash(result));

            result = Common.GetExcelFormat(columnNames, new List<List<object>> { new List<object> { "1" } }, null, "Sheet1", true, "rgb(238, 233, 233)", "rgb(238, 238, 224)", "rgb(255, 255, 255)");
            Assert.AreEqual(899, result.Length);
            Assert.AreEqual("ae0916e8dbdd8aff26e1d70f015a6e29", Common.GetMD5Hash(result));

            columnNames = new List<string> { "Col", "-1" };
            result = Common.GetExcelFormat(columnNames, values);
            Assert.AreEqual(1015, result.Length);
            Assert.AreEqual("476bfd5f05408af5ea0fbe261186baa7", Common.GetMD5Hash(result));

            object[][] valuesObj = new[] {new[] { (object) 0 }};

            result = Common.GetExcelFormat(
                columnNames.ToArray(), valuesObj, null, null, "Sheet1", true, 
                "rgb(238, 233, 233)", "rgb(238, 238, 224)", "rgb(255, 255, 255)",
                "rgb(255, 255, 255)", ",");

            Assert.AreEqual(1012, result.Length);
            Assert.AreEqual("3711d1a0b725abbf7eff979301f24443", Common.GetMD5Hash(result));

            Assert.ThrowsException<NullReferenceException>((() => Common.GetExcelFormat(null, values)));
            Assert.ThrowsException<NullReferenceException>((() => Common.GetExcelFormat(columnNames.ToArray(), null)));
            Assert.ThrowsException<NullReferenceException>((() => Common.GetExcelFormat(
                null, valuesObj, new string[] {"AVERAGE"}, null, "Sheet1", true,
                "rgb(238, 233, 233)", "rgb(238, 238, 224)", "rgb(255, 255, 255)",
                "rgb(255, 255, 255)", ",")));

            result = Common.GetExcelFormat(
                columnNames.ToArray(), valuesObj, new string[] {"COUNT"}, null, "Sheet1", true,
                "rgb(238, 233, 233)", "rgb(238, 238, 224)", "rgb(255, 255, 255)",
                "rgb(255, 255, 255)", ",");

            Assert.AreEqual(1189, result.Length);
            Assert.AreEqual("b235b0c3945f375314bd7e78ced33e3c", Common.GetMD5Hash(result));

            result = Common.GetExcelFormat(
                columnNames.ToArray(), valuesObj, new string[] { "SUM" }, null, "Sheet1", true,
                "rgb(238, 233, 233)", "rgb(238, 238, 224)", "rgb(255, 255, 255)",
                "rgb(255, 255, 255)", ",");

            Assert.AreEqual(1187, result.Length);
            Assert.AreEqual("3946c1dc56202cbcce89ea1c2bf1f713", Common.GetMD5Hash(result));

            result = Common.GetExcelFormat(
                columnNames.ToArray(), valuesObj, new string[] { "AVERAGE" }, null, "Sheet1", true,
                "rgb(238, 233, 233)", "rgb(238, 238, 224)", "rgb(255, 255, 255)",
                "rgb(255, 255, 255)", ",");

            Assert.AreEqual(1187, result.Length);
            Assert.AreEqual("c1494f920adde786c52d345af8f59cb2", Common.GetMD5Hash(result));

            valuesObj = new[] { new[] { (object)0 }, new[] { (object)0 } };
            result = Common.GetExcelFormat(
                columnNames.ToArray(), valuesObj, new string[] { "AVERAGE" }, null, "Sheet1", true,
                "rgb(238, 233, 233)", "rgb(238, 238, 224)", "rgb(255, 255, 255)",
                "rgb(255, 255, 255)", ",");

            Assert.AreEqual(1315, result.Length);
            Assert.AreEqual("103f0fd9e28e1eec04300dcf61fc05b0", Common.GetMD5Hash(result));
        }

        [TestMethod]
        public void MambaErrorTest()
        {
            var error = new Common.MambaError
            {
                Message = "Error",
                StackTrace = "Stack",
                Exception = new Exception()
            };

            Assert.AreEqual("Error", error.Message);
            Assert.AreEqual("Stack", error.StackTrace);
            Assert.IsInstanceOfType(error.Exception, typeof(Exception));

            error = new Common.MambaError(new Exception());

            Assert.IsInstanceOfType(error.Exception, typeof(Exception));
        }
    }
}
