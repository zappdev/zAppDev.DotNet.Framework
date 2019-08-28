using zAppDev.DotNet.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Utilities
{
    [TestClass]
    public class ZipIonicTest
    {
        [TestMethod]
        public void CompressDecompressTest()
        {
            var comp = ZipIonic.Compress("test00000000000000000000000000000000000000");
            
            Assert.IsNotNull(comp);

            var result = ZipIonic.Decompress(comp);
            
            Assert.AreEqual("test00000000000000000000000000000000000000", result);
        }
    }
}