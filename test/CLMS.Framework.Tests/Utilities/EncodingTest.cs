using CLMS.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace CLMS.Framework.Tests.Utilities
{
    [TestClass]
    public class EncodingTest
    {
        [TestMethod]
        public void EncodingUtilitiesTest()
        {
            var data = EncodingUtilities.StringToByteArray("test", "utf-8");
            Assert.AreEqual("test", EncodingUtilities.ByteArrayToString(data, "utf-8"));

            data = EncodingUtilities.StringToByteArray("test", "ascii");
            Assert.AreEqual("test", EncodingUtilities.ByteArrayToString(data, "ascii"));

            data = EncodingUtilities.StringToByteArray("test", "utf-16");
            Assert.AreEqual("test", EncodingUtilities.ByteArrayToString(data, "utf-16"));

            Assert.ThrowsException<Exception>(() => { EncodingUtilities.StringToByteArray("test", null); });
            Assert.ThrowsException<Exception>(() => { EncodingUtilities.StringToByteArray("test", "utf16"); });
            Assert.ThrowsException<Exception>(() => { EncodingUtilities.StringToByteArray("test", "u"); });
        }

    }
}
