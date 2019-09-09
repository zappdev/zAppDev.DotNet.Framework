// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Tests.Utilities
{
    [TestClass]
    public class ImagingTest
    {
        [TestMethod]
        public void GetThumbnailFromByte()
        {
            var data = Imaging.GetThumbnail("./Assets/garden.png", 100, 100);

            var result = Imaging.GetThumbnail(data.ToArray(), 100, 100);

            var resized = Imaging.ByteArrayToImage(result.ToArray());

            resized.Save($"./Assets/resized-garden-100.png", System.Drawing.Imaging.ImageFormat.Png); // Debuging

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetThumbnailFromPath()
        {
            var result = Imaging.GetThumbnail("./Assets/garden.png", 100, 100);

            var resized = Imaging.ByteArrayToImage(result.ToArray());

            resized.Save($"./Assets/resized-garden.png", System.Drawing.Imaging.ImageFormat.Png); // Debuging

            Assert.IsNotNull(result);
        }
    }
}
