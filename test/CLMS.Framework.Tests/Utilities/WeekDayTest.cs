using CLMS.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace CLMS.Framework.Tests.Utilities
{
    [TestClass]
    public class WeekDayTest
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

            day = WeekDay.FromIndex(3);

            Assert.AreEqual(3, day.Index);
            Assert.AreEqual(DayOfWeek.Wednesday, day.Value);
            Assert.AreEqual(false, day.IsWeekEnd);
            Assert.AreEqual(true, day == Week.Wednesday);
            Assert.AreEqual("Wed", day.GetShortName());
            Assert.AreEqual("Mi", day.GetShortName(CultureInfo.GetCultureInfo("de")));

            Assert.AreEqual("Wednesday", day.GetName());
            Assert.AreEqual("Mittwoch", day.GetName(CultureInfo.GetCultureInfo("de")));

            day = WeekDay.FromIndex(4);

            Assert.AreEqual(4, day.Index);
            Assert.AreEqual(DayOfWeek.Thursday, day.Value);
            Assert.AreEqual(false, day.IsWeekEnd);
            Assert.AreEqual(true, day == Week.Thursday);
            Assert.AreEqual("Thu", day.GetShortName());
            Assert.AreEqual("Do", day.GetShortName(CultureInfo.GetCultureInfo("de")));

            Assert.AreEqual("Thursday", day.GetName());
            Assert.AreEqual("Donnerstag", day.GetName(CultureInfo.GetCultureInfo("de")));

            day = WeekDay.FromIndex(5);

            Assert.AreEqual(5, day.Index);
            Assert.AreEqual(DayOfWeek.Friday, day.Value);
            Assert.AreEqual(false, day.IsWeekEnd);
            Assert.AreEqual(true, day == Week.Friday);
            Assert.IsTrue(Week.Friday.Equals(day));
            Assert.IsTrue(Week.Monday != day);
            Assert.AreEqual("Fri", day.GetShortName());
            Assert.AreEqual("Fr", day.GetShortName(CultureInfo.GetCultureInfo("de")));

            Assert.AreEqual("Friday", day.GetName());
            Assert.AreEqual("Freitag", day.GetName(CultureInfo.GetCultureInfo("de")));

            day = WeekDay.FromIndex(6);

            Assert.AreEqual(6, day.Index);
            Assert.AreEqual(DayOfWeek.Saturday, day.Value);
            Assert.AreEqual(true, day.IsWeekEnd);
            Assert.AreEqual(true, day == Week.Saturday);
            Assert.AreEqual("Sat", day.GetShortName());
            Assert.AreEqual("Sa", day.GetShortName(CultureInfo.GetCultureInfo("de")));

            Assert.AreEqual("Saturday", day.GetName());
            Assert.AreEqual("Samstag", day.GetName(CultureInfo.GetCultureInfo("de")));

	        day = WeekDay.FromIndex(7);
	        Assert.IsNull(day);

            day = WeekDay.FromEnglishName("Sunday");
            Assert.AreEqual(0, day.Index);
            day = WeekDay.FromEnglishName("Monday");
            Assert.AreEqual(1, day.Index);
            day = WeekDay.FromEnglishName("Tuesday");
            Assert.AreEqual(2, day.Index);
            day = WeekDay.FromEnglishName("Wednesday");
            Assert.AreEqual(3, day.Index);
            day = WeekDay.FromEnglishName("Thursday");
            Assert.AreEqual(4, day.Index);
            day = WeekDay.FromEnglishName("Friday");
            Assert.AreEqual(5, day.Index);
            day = WeekDay.FromEnglishName("Saturday");
            Assert.AreEqual(6, day.Index);
            day = WeekDay.FromEnglishName("em");
	        Assert.IsNull(day);

            day = WeekDay.FromShortEnglishName("Sun");
            Assert.AreEqual(0, day.Index);
            day = WeekDay.FromShortEnglishName("Mon");
            Assert.AreEqual(1, day.Index);
            day = WeekDay.FromShortEnglishName("Tue");
            Assert.AreEqual(2, day.Index);
            day = WeekDay.FromShortEnglishName("Wed");
            Assert.AreEqual(3, day.Index);
            day = WeekDay.FromShortEnglishName("Thu");
            Assert.AreEqual(4, day.Index);
            day = WeekDay.FromShortEnglishName("Fri");
            Assert.AreEqual(5, day.Index);
            day = WeekDay.FromShortEnglishName("Sat");
            Assert.AreEqual(6, day.Index);
            Assert.AreEqual(6, day.GetHashCode());
            day = WeekDay.FromShortEnglishName("em");
	        Assert.IsNull(day);

            day = WeekDay.FromDateTime((DateTime?)new DateTime(2018, 1, 7));
            Assert.AreEqual(0, day.Index);
            day = WeekDay.FromDateTime((DateTime?)new DateTime(2018, 1, 8));
            Assert.AreEqual(1, day.Index);
            day = WeekDay.FromDateTime((DateTime?)new DateTime(2018, 1, 9));
            Assert.AreEqual(2, day.Index);
            day = WeekDay.FromDateTime((DateTime?)new DateTime(2018, 1, 10));
            Assert.AreEqual(3, day.Index);
            day = WeekDay.FromDateTime((DateTime?)new DateTime(2018, 1, 11));
            Assert.AreEqual(4, day.Index);
            day = WeekDay.FromDateTime((DateTime?)new DateTime(2018, 1, 12));
            Assert.AreEqual(5, day.Index);
            day = WeekDay.FromDateTime((DateTime?)new DateTime(2018, 1, 13));
            Assert.AreEqual(6, day.Index);
            day = WeekDay.FromDateTime(null);
	        Assert.IsNull(day);

            day = WeekDay.FromShortEnglishName("Sat");
            Assert.IsTrue(day > null);
            Assert.IsFalse(null > day);
            Assert.IsFalse(day < null);
            Assert.IsTrue(null < day);
            day = null;
            Assert.IsFalse(null > day);
            Assert.IsFalse(day < null);

            day = WeekDay.FromIndex(6);
            Assert.IsTrue(Week.Thursday < day);
            Assert.IsFalse(Week.Thursday > day);
            
        }

        [TestMethod]
        public void WeekTest() {
            var week = new Week();
            var weekdays = week.GetWeek(true);           
            Assert.AreEqual(DayOfWeek.Sunday, weekdays[0].Value);

            weekdays = week.GetWeek(false);           
            Assert.AreEqual(DayOfWeek.Monday, weekdays[0].Value);

            weekdays = week.GetWeekStartingWith(Week.Tuesday);           
            Assert.AreEqual(DayOfWeek.Tuesday, weekdays[0].Value);

            weekdays = week.GetWeekStartingWith(Week.Wednesday);           
            Assert.AreEqual(DayOfWeek.Wednesday, weekdays[0].Value);

            weekdays = week.GetWeekStartingWith(Week.Thursday);           
            Assert.AreEqual(DayOfWeek.Thursday, weekdays[0].Value);

            weekdays = week.GetWeekStartingWith(Week.Friday);           
            Assert.AreEqual(DayOfWeek.Friday, weekdays[0].Value);

            weekdays = week.GetWeekStartingWith(Week.Saturday);           
            Assert.AreEqual(DayOfWeek.Saturday, weekdays[0].Value);

            weekdays = week.GetWeekStartingWith(null);
            Assert.IsNull(weekdays);           
        }

    }
}
