using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#if NETFRAMEWORK
using CLMS.Framework.Profiling.Glimpse;
#endif

namespace CLMS.Framework.Tests.Profiling.Glimpse
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