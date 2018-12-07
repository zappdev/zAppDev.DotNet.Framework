using CLMS.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace CLMS.Framework.Test.Utilities
{
    [TestClass]
    public class WebToolsTest
    {
        [TestMethod]
        public void FromIndexTest()
        {
            Assert.AreEqual(null, WeekDay.FromIndex(null));
            int? val = 0;
            Assert.AreEqual(0, WeekDay.FromIndex(val).Index);
            
            var day = WeekDay.FromIndex(0);

            Assert.AreEqual(0, day.Index);
            Assert.AreEqual(DayOfWeek.Sunday, day.Value);
            Assert.AreEqual(true, day.IsWeekEnd);
            Assert.AreEqual(true, day == Week.Sunday);
            Assert.AreEqual("Sun", day.GetShortName());
            Assert.AreEqual("So", day.GetShortName(CultureInfo.GetCultureInfo("de")));

            Assert.AreEqual("Sunday", day.GetName());
            Assert.AreEqual("Sonntag", day.GetName(CultureInfo.GetCultureInfo("de")));

            day = WeekDay.FromIndex(1);

            Assert.AreEqual(1, day.Index);
            Assert.AreEqual(DayOfWeek.Monday, day.Value);
            Assert.AreEqual(false, day.IsWeekEnd);
            Assert.AreEqual(true, day == Week.Monday);
            Assert.AreEqual("Mon", day.GetShortName());
            Assert.AreEqual("Mo", day.GetShortName(CultureInfo.GetCultureInfo("de")));

            Assert.AreEqual("Monday", day.GetName());
            Assert.AreEqual("Montag", day.GetName(CultureInfo.GetCultureInfo("de")));

            day = WeekDay.FromIndex(2);

            Assert.AreEqual(2, day.Index);
            Assert.AreEqual(DayOfWeek.Tuesday, day.Value);
            Assert.AreEqual(false, day.IsWeekEnd);
            Assert.AreEqual(true, day == Week.Tuesday);
            Assert.AreEqual("Tue", day.GetShortName());
            Assert.AreEqual("Di", day.GetShortName(CultureInfo.GetCultureInfo("de")));

            Assert.AreEqual("Tuesday", day.GetName());
            Assert.AreEqual("Dienstag", day.GetName(CultureInfo.GetCultureInfo("de")));
        }

    }
}
