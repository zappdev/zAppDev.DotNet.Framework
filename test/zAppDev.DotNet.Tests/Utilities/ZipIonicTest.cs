// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
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