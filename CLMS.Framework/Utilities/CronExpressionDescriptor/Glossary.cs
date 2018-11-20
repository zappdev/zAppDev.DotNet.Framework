using System.Collections.Generic;
using log4net;

namespace CLMS.Framework.Utilities.CronExpressionDescriptor
{
    internal class Glossary
    {
        private static Dictionary<string, string> _dictionary { get; set; }

        private static void Initialize()
        {
            if (_dictionary?.Count > 0) return;

            _dictionary = new Dictionary<string, string>();
            _dictionary.Add("EveryMinute", "every minute");
            _dictionary.Add("EveryHour", "every hour");
            _dictionary.Add("AnErrorOccuredWhenGeneratingTheExpressionD", "An error occured when generating the expression description.  Check the cron expression syntax.");
            _dictionary.Add("AtSpace", "At ");
            _dictionary.Add("EveryMinuteBetweenX0AndX1", "Every minute between {0} and {1}");
            _dictionary.Add("At", "At");
            _dictionary.Add("SpaceAnd", " and");
            _dictionary.Add("EverySecond", "every second");
            _dictionary.Add("EveryX0Seconds", "every {0} seconds");
            _dictionary.Add("SecondsX0ThroughX1PastTheMinute", "seconds {0} through {1} past the minute");
            _dictionary.Add("AtX0SecondsPastTheMinute", "at {0} seconds past the minute");
            _dictionary.Add("EveryX0Minutes", "every {0} minutes");
            _dictionary.Add("MinutesX0ThroughX1PastTheHour", "minutes {0} through {1} past the hour");
            _dictionary.Add("AtX0MinutesPastTheHour", "at {0} minutes past the hour");
            _dictionary.Add("EveryX0Hours", "every {0} hours");
            _dictionary.Add("BetweenX0AndX1", "between {0} and {1}");
            _dictionary.Add("AtX0", "at {0}");
            _dictionary.Add("ComaEveryDay", ", every day");
            _dictionary.Add("ComaEveryX0DaysOfTheWeek", ", every {0} days of the week");
            _dictionary.Add("ComaX0ThroughX1", ", {0} through {1}");
            _dictionary.Add("First", "first");
            _dictionary.Add("Second", "second");
            _dictionary.Add("Third", "third");
            _dictionary.Add("Fourth", "fourth");
            _dictionary.Add("Fifth", "fifth");
            _dictionary.Add("ComaOnThe", ", on the ");
            _dictionary.Add("SpaceX0OfTheMonth", " {0} of the month");
            _dictionary.Add("ComaOnTheLastX0OfTheMonth", ", on the last {0} of the month");
            _dictionary.Add("ComaOnlyOnX0", ", only on {0}");
            _dictionary.Add("ComaEveryX0Months", ", every {0} months");
            _dictionary.Add("ComaOnlyInX0", ", only in {0}");
            _dictionary.Add("ComaOnTheLastDayOfTheMonth", ", on the last day of the month");
            _dictionary.Add("ComaOnTheLastWeekdayOfTheMonth", ", on the last weekday of the month");
            _dictionary.Add("FirstWeekday", "first weekday");
            _dictionary.Add("WeekdayNearestDayX0", "weekday nearest day {0}");
            _dictionary.Add("ComaOnTheX0OfTheMonth", ", on the {0} of the month");
            _dictionary.Add("ComaEveryX0Days", ", every {0} days");
            _dictionary.Add("ComaBetweenDayX0AndX1OfTheMonth", ", between day {0} and {1} of the month");
            _dictionary.Add("ComaOnDayX0OfTheMonth", ", on day {0} of the month");
            _dictionary.Add("SpaceAndSpace", " and ");
            _dictionary.Add("ComaEveryMinute", ", every minute");
            _dictionary.Add("ComaEveryHour", ", every hour");
            _dictionary.Add("ComaEveryX0Years", ", every {0} years");
            _dictionary.Add("CommaStartingX0", ", starting {0}");
            _dictionary.Add("AMPeriod", "AM");
            _dictionary.Add("PMPeriod", "PM");
            _dictionary.Add("CommaDaysBeforeTheLastDayOfTheMonth", ", {0} days before the last day of the month");

        }

        public static string GetString(string term)
        {
            Initialize();
            return _dictionary[term];
        }

        /// <summary>
        ///   Looks up a localized string similar to AM.
        /// </summary>
        internal static string AMPeriod
        {
            get
            {
                return GetString("AMPeriod");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to An error occured when generating the expression description.  Check the cron expression syntax..
        /// </summary>
        internal static string AnErrorOccuredWhenGeneratingTheExpressionD
        {
            get
            {
                return GetString("AnErrorOccuredWhenGeneratingTheExpressionD");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to At.
        /// </summary>
        internal static string At
        {
            get
            {
                return GetString("At");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to At .
        /// </summary>
        internal static string AtSpace
        {
            get
            {
                return GetString("AtSpace");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to at {0}.
        /// </summary>
        internal static string AtX0
        {
            get
            {
                return GetString("AtX0");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to at {0} minutes past the hour.
        /// </summary>
        internal static string AtX0MinutesPastTheHour
        {
            get
            {
                return GetString("AtX0MinutesPastTheHour");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to at {0} seconds past the minute.
        /// </summary>
        internal static string AtX0SecondsPastTheMinute
        {
            get
            {
                return GetString("AtX0SecondsPastTheMinute");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to between {0} and {1}.
        /// </summary>
        internal static string BetweenX0AndX1
        {
            get
            {
                return GetString("BetweenX0AndX1");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to , between day {0} and {1} of the month.
        /// </summary>
        internal static string ComaBetweenDayX0AndX1OfTheMonth
        {
            get
            {
                return GetString("ComaBetweenDayX0AndX1OfTheMonth");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to , every day.
        /// </summary>
        internal static string ComaEveryDay
        {
            get
            {
                return GetString("ComaEveryDay");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to , every hour.
        /// </summary>
        internal static string ComaEveryHour
        {
            get
            {
                return GetString("ComaEveryHour");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to , every minute.
        /// </summary>
        internal static string ComaEveryMinute
        {
            get
            {
                return GetString("ComaEveryMinute");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to , every {0} days.
        /// </summary>
        internal static string ComaEveryX0Days
        {
            get
            {
                return GetString("ComaEveryX0Days");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to , every {0} days of the week.
        /// </summary>
        internal static string ComaEveryX0DaysOfTheWeek
        {
            get
            {
                return GetString("ComaEveryX0DaysOfTheWeek");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to , every {0} months.
        /// </summary>
        internal static string ComaEveryX0Months
        {
            get
            {
                return GetString("ComaEveryX0Months");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to , every {0} years.
        /// </summary>
        internal static string ComaEveryX0Years
        {
            get
            {
                return GetString("ComaEveryX0Years");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to , on day {0} of the month.
        /// </summary>
        internal static string ComaOnDayX0OfTheMonth
        {
            get
            {
                return GetString("ComaOnDayX0OfTheMonth");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to , only in {0}.
        /// </summary>
        internal static string ComaOnlyInX0
        {
            get
            {
                return GetString("ComaOnlyInX0");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to , only on {0}.
        /// </summary>
        internal static string ComaOnlyOnX0
        {
            get
            {
                return GetString("ComaOnlyOnX0");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to , on the .
        /// </summary>
        internal static string ComaOnThe
        {
            get
            {
                return GetString("ComaOnThe");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to , on the last day of the month.
        /// </summary>
        internal static string ComaOnTheLastDayOfTheMonth
        {
            get
            {
                return GetString("ComaOnTheLastDayOfTheMonth");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to , on the last weekday of the month.
        /// </summary>
        internal static string ComaOnTheLastWeekdayOfTheMonth
        {
            get
            {
                return GetString("ComaOnTheLastWeekdayOfTheMonth");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to , on the last {0} of the month.
        /// </summary>
        internal static string ComaOnTheLastX0OfTheMonth
        {
            get
            {
                return GetString("ComaOnTheLastX0OfTheMonth");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to , on the {0} of the month.
        /// </summary>
        internal static string ComaOnTheX0OfTheMonth
        {
            get
            {
                return GetString("ComaOnTheX0OfTheMonth");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to , {0} through {1}.
        /// </summary>
        internal static string ComaX0ThroughX1
        {
            get
            {
                return GetString("ComaX0ThroughX1");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to , starting {0}.
        /// </summary>
        internal static string CommaStartingX0
        {
            get
            {
                return GetString("CommaStartingX0");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to every hour.
        /// </summary>
        internal static string EveryHour
        {
            get
            {
                return GetString("EveryHour");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to every minute.
        /// </summary>
        internal static string EveryMinute
        {
            get
            {
                return GetString("EveryMinute");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Every minute between {0} and {1}.
        /// </summary>
        internal static string EveryMinuteBetweenX0AndX1
        {
            get
            {
                return GetString("EveryMinuteBetweenX0AndX1");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to every second.
        /// </summary>
        internal static string EverySecond
        {
            get
            {
                return GetString("EverySecond");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to every {0} hours.
        /// </summary>
        internal static string EveryX0Hours
        {
            get
            {
                return GetString("EveryX0Hours");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to every {0} minutes.
        /// </summary>
        internal static string EveryX0Minutes
        {
            get
            {
                return GetString("EveryX0Minutes");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to every {0} seconds.
        /// </summary>
        internal static string EveryX0Seconds
        {
            get
            {
                return GetString("EveryX0Seconds");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to fifth.
        /// </summary>
        internal static string Fifth
        {
            get
            {
                return GetString("Fifth");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to first.
        /// </summary>
        internal static string First
        {
            get
            {
                return GetString("First");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to first weekday.
        /// </summary>
        internal static string FirstWeekday
        {
            get
            {
                return GetString("FirstWeekday");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to fourth.
        /// </summary>
        internal static string Fourth
        {
            get
            {
                return GetString("Fourth");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to minutes {0} through {1} past the hour.
        /// </summary>
        internal static string MinutesX0ThroughX1PastTheHour
        {
            get
            {
                return GetString("MinutesX0ThroughX1PastTheHour");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to PM.
        /// </summary>
        internal static string PMPeriod
        {
            get
            {
                return GetString("PMPeriod");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to second.
        /// </summary>
        internal static string Second
        {
            get
            {
                return GetString("Second");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to seconds {0} through {1} past the minute.
        /// </summary>
        internal static string SecondsX0ThroughX1PastTheMinute
        {
            get
            {
                return GetString("SecondsX0ThroughX1PastTheMinute");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to  and.
        /// </summary>
        internal static string SpaceAnd
        {
            get
            {
                return GetString("SpaceAnd");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to  and .
        /// </summary>
        internal static string SpaceAndSpace
        {
            get
            {
                return GetString("SpaceAndSpace");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to  {0} of the month.
        /// </summary>
        internal static string SpaceX0OfTheMonth
        {
            get
            {
                return GetString("SpaceX0OfTheMonth");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to third.
        /// </summary>
        internal static string Third
        {
            get
            {
                return GetString("Third");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to weekday nearest day {0}.
        /// </summary>
        internal static string WeekdayNearestDayX0
        {
            get
            {
                return GetString("WeekdayNearestDayX0");
            }
        }

        internal static string CommaDaysBeforeTheLastDayOfTheMonth
        {
            get
            {
                return GetString("CommaDaysBeforeTheLastDayOfTheMonth");
            }
        }
    }
}
