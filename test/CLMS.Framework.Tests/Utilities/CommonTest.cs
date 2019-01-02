using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using CLMS.Framework.Utilities;

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
        public string Property { get; set; }

        public List<string> Items { get; set; }

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

        [TestMethod]
        public void NormalizeLineEncodingTest()
        {
            Assert.AreEqual("Test\r\n", Common.NormalizeLineEncoding("Test\r\n"));
            Assert.AreEqual("Test\r\n", Common.NormalizeLineEncoding("Test\n"));
            Assert.AreEqual("Test\r\n", Common.NormalizeLineEncoding("Test\r"));
        }

        [TestMethod]
        public void FromListTest()
        {
            Assert.AreEqual(null, Common.GetItemFromList<object>(null, 0));
            Assert.ThrowsException<Exception>(() => Common.GetItemFromList(new List<string> { "red", "black" }, null));
            Assert.AreEqual("red", Common.GetItemFromList(new List<string> { "red", "black" }, 0));

            var items = new List<string> { "red", "black" };
            Assert.ThrowsException<Exception>(() => Common.SetItemFromList<string>(items, null, null));
            Assert.ThrowsException<Exception>(() => Common.SetItemFromList<object>(null, null, null));

            items = new List<string> { "red", "black" };
            Common.SetItemFromList(items, 0, "green");
            Assert.AreEqual("green", Common.GetItemFromList(items, 0));
        }

        [TestMethod]
        public void FromArrayTest()
        {
            var items = new string[] { "red", "black" };

            Assert.AreEqual(null, Common.GetItemFromArray<object>(null, 0));
            Assert.ThrowsException<Exception>(() => Common.GetItemFromArray(items, null));
            Assert.AreEqual("red", Common.GetItemFromArray(items, 0));

            Assert.ThrowsException<Exception>(() => Common.SetItemFromArray<string>(items, null, null));
            Assert.ThrowsException<Exception>(() => Common.SetItemFromArray<object>(null, null, null));

            Common.SetItemFromArray(items, 0, "green");
            Assert.AreEqual("green", Common.GetItemFromArray(items, 0));
        }

        [TestMethod]
        public void GetCultureInfoTest()
        {
            Assert.AreEqual("en-US", Common.GetCultureInfo(null).Name);
            Assert.AreEqual("ar", Common.GetCultureInfo(1).Name);
        }

        [TestMethod]
        public void ToUnixTimeTest()
        {
            Assert.AreEqual(1451606400, Common.ToUnixTime(new DateTime(2016, 1, 1)));
        }

        [TestMethod]
        public void ParseExactDateTest()
        {
            var ci = Common.GetCultureInfo(null);

            Assert.IsNull(Common.ParseExactDate("2017", "YYYY-MM-dd", ci));
        }

        [TestMethod]
        public void DateHasValueTest()
        {
            var ci = Common.GetCultureInfo(null);
            var date = Common.ParseExactDate("2017", "YYYY-MM-dd", ci);

            Assert.IsFalse(Common.DateHasValue(date));        
            Assert.IsFalse(Common.DateHasValue(new DateTime()));
        }

        [TestMethod]
        public void RandomTest()
        {
            Assert.IsTrue(Common.Random.Next(1, 7) > 0);
        }


        [TestMethod]
        public void FileTest()
        {
            Common.WriteAllTo("name.txt", "Example");
            Common.WriteAllTo("name.txt", "Example", 1252);
            Common.WriteAllTo("name.txt", "Example", true);
            Common.WriteAllTo("name.txt", new List<byte> { 81 });
            Common.WriteAllTo("name.txt", new byte[] { 81 });

            Common.AppendAllTo("name.txt", "Example");
            Common.AppendAllTo("name.txt", "Example", 1252);
            Common.AppendAllTo("name.txt", "Example", true);
            Common.AppendAllTo("name.txt", new List<byte> { 81 });
            Common.AppendAllTo("name.txt", new byte[] { 81 });         
            
            Common.MoveFile("./name.txt", "Assets/name.txt", true);
            Common.ReadLinesFrom("./Assets/name.txt", 2, 5, 1252);
        }

        [TestMethod]
        public void IsTypePrimitiveOrSimpleTest()
        {
            var info = typeof(CommonTest).GetProperty("Property");

            var mambaType = new MambaRuntimeType(info);

            Assert.IsTrue(Common.IsTypePrimitiveOrSimple(typeof(String)));
            Assert.IsTrue(Common.IsTypePrimitiveOrSimple(typeof(int)));
            Assert.IsTrue(Common.IsTypeCollection(typeof(List<string>)));

            Assert.IsTrue(Common.IsPropertyPrimitiveOrSimple(info));
            Assert.IsTrue(Common.IsPropertyPrimitiveOrSimple(mambaType));

            info = typeof(CommonTest).GetProperty("Items");
            mambaType = new MambaRuntimeType(info);

            Assert.IsTrue(Common.IsPropertyCollection(info));
            Assert.IsTrue(Common.IsPropertyCollection(mambaType));
        }

        [TestMethod]
        public void ConvertTest()
        {
            var status = Common.ConvertToEnum<Status>("FAILED");
            Assert.AreEqual(Status.FAILED, status);

            status = Common.ConvertToEnum<Status>("F", false);
            Assert.AreEqual(Status.FAILED, status);

            Assert.ThrowsException<FormatException>(() => Common.ConvertToEnum<Status>("F", true));

            Assert.AreEqual(0.1m, Common.ConvertToDecimal("0.1"));
            Assert.AreEqual(0m, Common.ConvertToDecimal("d", false));
            Assert.IsNull(Common.ConvertToNullableDecimal("d"));
            Assert.ThrowsException<FormatException>(() => Common.ConvertToDecimal("d", true));

            Assert.AreEqual(0.1f, Common.ConvertToFloat("0.1"));
            Assert.AreEqual(0f, Common.ConvertToFloat("d", false));
            Assert.IsNull(Common.ConvertToNullableFloat("d"));
            Assert.ThrowsException<FormatException>(() => Common.ConvertToFloat("d", true));

            Assert.AreEqual("yyyy-MM-dd tt", Common.ConvertMomentFormat("YYYY-MM-DD a"));
            Assert.AreEqual("yyyy", Common.ConvertMomentFormat("YYYY"));
            Assert.AreEqual("MM-dd", Common.ConvertMomentFormat("MM-DD"));

            Assert.AreEqual(2018, Common.ConvertToDateTime("2018-01-01").Year);
            Assert.AreEqual(1, Common.ConvertToDateTime("d", false).Year);
            Assert.IsNull(Common.ConvertToNullableDateTime("d"));
            Assert.ThrowsException<FormatException>(() => Common.ConvertToDateTime("d", true));

            Assert.AreEqual(1, Common.ConvertToInt("1"));
            Assert.AreEqual(0, Common.ConvertToInt("d", false));
            Assert.IsNull(Common.ConvertToNullableInt("d"));
            Assert.ThrowsException<FormatException>(() => Common.ConvertToInt("d", true));

            Assert.AreEqual(1L, Common.ConvertToLong("1"));
            Assert.AreEqual(0L, Common.ConvertToLong("d", false));
            Assert.IsNull(Common.ConvertToNullableLong("d"));
            Assert.ThrowsException<FormatException>(() => Common.ConvertToLong("d", true));

            Assert.AreEqual(true, Common.ConvertToBool("true"));
            Assert.AreEqual(false, Common.ConvertToBool("d", false));
            Assert.IsNull(Common.ConvertToNullableBool("d"));
            Assert.ThrowsException<FormatException>(() => Common.ConvertToBool("d", true));

            Assert.AreEqual(0.1d, Common.ConvertToDouble("0.1"));
            Assert.AreEqual(0.0d, Common.ConvertToDouble("d", false));
            Assert.IsNull(Common.ConvertToNullableDouble("d"));
            Assert.ThrowsException<FormatException>(() => Common.ConvertToDouble("d", true));

            Assert.AreEqual(0b1, Common.ConvertToByte("1"));
            Assert.AreEqual(0b0, Common.ConvertToByte("d", false));
            Assert.IsNull(Common.ConvertToNullableByte("d"));
            Assert.ThrowsException<FormatException>(() => Common.ConvertToByte("d", true));

            Assert.IsNotNull(Common.ConvertToGuid("9081ef78-2ee8-48df-a9af-4fc71fabfcc0"));
            Assert.IsNotNull(Common.ConvertToGuid("d", false));
            Assert.IsNull(Common.ConvertToNullableGuid("d"));
            Assert.ThrowsException<FormatException>(() => Common.ConvertToGuid("d", true));
        }

        [TestMethod]
        public void Base64Test() 
        {
            Assert.AreEqual("VGVzdA==", Common.Base64Encode("Test"));
            Assert.AreEqual("UQ==", Common.Base64Encode(new byte []{ 81 }));
            Assert.AreEqual("Test", Common.Base64Decode("VGVzdA=="));
            Assert.AreEqual(81, Common.Base64DecodeAsByteArray("UQ==")[0]);
        }

        [TestMethod]
        public void TypeTest()
        {
            Assert.AreEqual("CommonTest", Common.GetTypeName(typeof(CommonTest)));
            Assert.AreEqual("CLMS.Framework.Tests.Utilities.CommonTest", Common.GetTypeName(typeof(CommonTest), true));
            
            Assert.AreEqual("Property", Common.GetProperty(typeof(CommonTest), "Property").Name);
            Assert.AreEqual(null, Common.GetProperty(typeof(CommonTest), "Property", true));
        }

        [TestMethod]
        public void ReadLinesFromTest() 
        {
            var lines = Common.ReadLinesFrom("./Assets/Line.csv", 2, 5, 1252);

            Assert.AreEqual(2, lines.Count);
            Assert.AreEqual("Apple", lines[0]);

            Common.ReadLinesFrom("./Assets/Line.csv", 2, 1, 1252);

            Common.CopyFile("./Assets/Line.csv", "./Assets/LineTemp.csv");;

            lines = Common.ExtractLinesFrom("./Assets/LineTemp.csv", 2, 1252);

            Assert.AreEqual(2, lines.Count);
            Assert.AreEqual("Test", lines[0]);
            Common.ExtractLinesFrom("./Assets/LineTemp.csv", 2, 1252);
        }

        [TestMethod]
        public void RunExecutableTest() 
        {          
            Assert.ThrowsException<Win32Exception>(() => Common.RunExecutable("./hello.exe"));

#if Windows
            var result = Common.RunExecutable("./Assets/hh2.golden.exe");
            Assert.AreEqual("Hello, World!\r\n", result);
#endif
        }

        [TestMethod]
        public void GetNextExecutionTimeTest() 
        {
            Assert.AreEqual(5, Common.GetNextExecutionTime("*/5 * * * *", new DateTime(2018, 1, 1)).Minute);

            DateTime? date = new DateTime(2018, 1, 1);
            Assert.AreEqual(5, Common.GetNextExecutionTime("*/5 * * * *", date).Minute);
        }

        [TestMethod]
        public void SmoothTest()
        {
            Assert.AreEqual("4a502cbee9b51224da964f9ffb363c77", Common.GetMD5Hash(Common.SmoothRead("./Assets/JSON.ps1")));
            Assert.AreEqual(402, Common.SmoothReadBinary("./Assets/hh2.golden.exe").Length);
        }
    }

    enum Status {
        FAILED,
        COMPLETED
    }
}
