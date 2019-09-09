// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#if NETFRAMEWORK
using zAppDev.DotNet.Framework.Profiling.Glimpse;
#endif

namespace zAppDev.DotNet.Framework.Tests.Profiling.Glimpse
{
    [TestClass]
    public class AppDevTimelineMessageTest
    {
#if NETFRAMEWORK
        [TestMethod]
        public void PropertyTest()
        {
            var info = new AppDevTimelineMessage
            {
                Duration = TimeSpan.Zero,
                EventName = "Event",
                EventSubText = "Info",
                Offset = TimeSpan.FromMilliseconds(100),
                StartTime = DateTime.Parse("2010-01-01")
            };

            Assert.IsNotNull(info.Id);
            Assert.AreEqual(TimeSpan.Zero, info.Duration);
            Assert.AreEqual(TimeSpan.FromMilliseconds(100), info.Offset);

            Assert.AreEqual("Event", info.EventName);
            Assert.AreEqual("Info", info.EventSubText);

            Assert.AreEqual(DateTime.Parse("2010-01-01"), info.StartTime);
            Assert.AreEqual("#405064", info.EventCategory.Color);
            Assert.AreEqual("#dce2ea", info.EventCategory.ColorHighlight);
            Assert.AreEqual("zAppDev", info.EventCategory.Name);

            info.EventCategory = null; 
        }
#endif
    }
}