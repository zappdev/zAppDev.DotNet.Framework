using Microsoft.VisualStudio.TestTools.UnitTesting;
using zAppDev.DotNet.Framework.Utilities.CronExpressionDescriptor;

namespace zAppDev.DotNet.Framework.Tests.Utilities.CronExpressionDescriptor
{
    [TestClass]
    public class GlossaryTest
    {
        [TestMethod]
        public void GetStringTest()
        {
            Assert.AreEqual(Glossary.EveryMinute, Glossary.GetString("EveryMinute"));
            Assert.AreEqual(Glossary.EveryHour, Glossary.GetString("EveryHour"));
            Assert.AreEqual(Glossary.AnErrorOccuredWhenGeneratingTheExpressionD, Glossary.GetString("AnErrorOccuredWhenGeneratingTheExpressionD"));
            Assert.AreEqual(Glossary.AtSpace, Glossary.GetString("AtSpace"));
            Assert.AreEqual(Glossary.EveryMinuteBetweenX0AndX1, Glossary.GetString("EveryMinuteBetweenX0AndX1"));
            Assert.AreEqual(Glossary.At, Glossary.GetString("At"));
            Assert.AreEqual(Glossary.SpaceAnd, Glossary.GetString("SpaceAnd"));
            Assert.AreEqual(Glossary.EverySecond, Glossary.GetString("EverySecond"));
            Assert.AreEqual(Glossary.EveryX0Seconds, Glossary.GetString("EveryX0Seconds"));
            Assert.AreEqual(Glossary.SecondsX0ThroughX1PastTheMinute, Glossary.GetString("SecondsX0ThroughX1PastTheMinute"));
            Assert.AreEqual(Glossary.AtX0SecondsPastTheMinute, Glossary.GetString("AtX0SecondsPastTheMinute"));
            Assert.AreEqual(Glossary.EveryX0Minutes, Glossary.GetString("EveryX0Minutes"));
            Assert.AreEqual(Glossary.MinutesX0ThroughX1PastTheHour, Glossary.GetString("MinutesX0ThroughX1PastTheHour"));
            Assert.AreEqual(Glossary.AtX0MinutesPastTheHour, Glossary.GetString("AtX0MinutesPastTheHour"));

            Assert.AreEqual(Glossary.EveryX0Hours, Glossary.GetString("EveryX0Hours"));
            Assert.AreEqual(Glossary.BetweenX0AndX1, Glossary.GetString("BetweenX0AndX1"));
            Assert.AreEqual(Glossary.AtX0, Glossary.GetString("AtX0"));
            Assert.AreEqual(Glossary.ComaEveryDay, Glossary.GetString("ComaEveryDay"));
            Assert.AreEqual(Glossary.ComaEveryX0DaysOfTheWeek, Glossary.GetString("ComaEveryX0DaysOfTheWeek"));
            Assert.AreEqual(Glossary.ComaX0ThroughX1, Glossary.GetString("ComaX0ThroughX1"));
            Assert.AreEqual(Glossary.First, Glossary.GetString("First"));
            Assert.AreEqual(Glossary.Second, Glossary.GetString("Second"));
            Assert.AreEqual(Glossary.Third, Glossary.GetString("Third"));
            Assert.AreEqual(Glossary.Fourth, Glossary.GetString("Fourth"));
            Assert.AreEqual(Glossary.Fifth, Glossary.GetString("Fifth"));
            Assert.AreEqual(Glossary.ComaOnThe, Glossary.GetString("ComaOnThe"));
            Assert.AreEqual(Glossary.SpaceX0OfTheMonth, Glossary.GetString("SpaceX0OfTheMonth"));

            Assert.AreEqual(Glossary.ComaOnTheLastX0OfTheMonth, Glossary.GetString("ComaOnTheLastX0OfTheMonth"));
            Assert.AreEqual(Glossary.ComaOnlyOnX0, Glossary.GetString("ComaOnlyOnX0"));
            Assert.AreEqual(Glossary.ComaEveryX0Months, Glossary.GetString("ComaEveryX0Months"));
            Assert.AreEqual(Glossary.ComaOnlyInX0, Glossary.GetString("ComaOnlyInX0"));
            Assert.AreEqual(Glossary.ComaOnTheLastDayOfTheMonth, Glossary.GetString("ComaOnTheLastDayOfTheMonth"));
            Assert.AreEqual(Glossary.ComaOnTheLastWeekdayOfTheMonth, Glossary.GetString("ComaOnTheLastWeekdayOfTheMonth"));
            Assert.AreEqual(Glossary.FirstWeekday, Glossary.GetString("FirstWeekday"));
            Assert.AreEqual(Glossary.WeekdayNearestDayX0, Glossary.GetString("WeekdayNearestDayX0"));
            Assert.AreEqual(Glossary.ComaOnTheX0OfTheMonth, Glossary.GetString("ComaOnTheX0OfTheMonth"));

            Assert.AreEqual(Glossary.ComaEveryX0Days, Glossary.GetString("ComaEveryX0Days"));
            Assert.AreEqual(Glossary.ComaBetweenDayX0AndX1OfTheMonth, Glossary.GetString("ComaBetweenDayX0AndX1OfTheMonth"));
            Assert.AreEqual(Glossary.SpaceAndSpace, Glossary.GetString("SpaceAndSpace"));
            Assert.AreEqual(Glossary.ComaEveryMinute, Glossary.GetString("ComaEveryMinute"));
            Assert.AreEqual(Glossary.ComaEveryHour, Glossary.GetString("ComaEveryHour"));
            Assert.AreEqual(Glossary.ComaEveryX0Years, Glossary.GetString("ComaEveryX0Years"));

            Assert.AreEqual(Glossary.CommaStartingX0, Glossary.GetString("CommaStartingX0"));
            Assert.AreEqual(Glossary.AMPeriod, Glossary.GetString("AMPeriod"));
            Assert.AreEqual(Glossary.PMPeriod, Glossary.GetString("PMPeriod"));
            Assert.AreEqual(Glossary.CommaDaysBeforeTheLastDayOfTheMonth, Glossary.GetString("CommaDaysBeforeTheLastDayOfTheMonth"));

            Assert.AreEqual(Glossary.ComaOnDayX0OfTheMonth, Glossary.GetString("ComaOnDayX0OfTheMonth"));
            
        }
    }
}
