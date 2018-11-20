using System;
using System.Globalization;

namespace CLMS.Framework.Utilities
{
    public class WeekDay
    {
        public int Index { get; set; }
        public DayOfWeek Value { get; set; }
        public bool IsWeekEnd => Value == DayOfWeek.Saturday || Value == DayOfWeek.Sunday;

        public string GetName(CultureInfo culture = null)
        {
            return culture == null
                ? new CultureInfo("en-US").DateTimeFormat.GetDayName(Value)
                : culture.DateTimeFormat.GetDayName(Value);
        }

        public string GetShortName(CultureInfo culture = null)
        {
            return culture == null
                ? new CultureInfo("en-US").DateTimeFormat.GetAbbreviatedDayName(Value)
                : culture.DateTimeFormat.GetAbbreviatedDayName(Value);
        }

        public static WeekDay FromIndex(int? index)
        {
            if (index == null) return null;

            return FromIndex(index.GetValueOrDefault());
        }

        public static WeekDay FromIndex(int index)
        {
            if (index == 0) return Week.Sunday;
            if (index == 1) return Week.Monday;
            if (index == 2) return Week.Tuesday;
            if (index == 3) return Week.Wednesday;
            if (index == 4) return Week.Thursday;
            if (index == 5) return Week.Friday;
            if (index == 6) return Week.Saturday;

            return null;
        }

        public static WeekDay FromEnglishName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            if (name.Trim() == "Sunday") return Week.Sunday;
            if (name.Trim() == "Monday") return Week.Monday;
            if (name.Trim() == "Tuesday") return Week.Tuesday;
            if (name.Trim() == "Wednesday") return Week.Wednesday;
            if (name.Trim() == "Thursday") return Week.Thursday;
            if (name.Trim() == "Friday") return Week.Friday;
            if (name.Trim() == "Saturday") return Week.Saturday;

            return null;
        }

        public static WeekDay FromShortEnglishName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            if (name.Trim() == "Sun") return Week.Sunday;
            if (name.Trim() == "Mon") return Week.Monday;
            if (name.Trim() == "Tue") return Week.Tuesday;
            if (name.Trim() == "Wed") return Week.Wednesday;
            if (name.Trim() == "Thu") return Week.Thursday;
            if (name.Trim() == "Fri") return Week.Friday;
            if (name.Trim() == "Sat") return Week.Saturday;

            return null;
        }

        public static WeekDay FromDateTime(DateTime? dateTime)
        {
            if (dateTime == null) return null;

            return FromDateTime(dateTime.GetValueOrDefault());
        }

        public static WeekDay FromDateTime(DateTime dateTime)
        {
            var day = dateTime.DayOfWeek;

            switch (day)
            {
                case DayOfWeek.Monday:
                    return Week.Monday;

                case DayOfWeek.Tuesday:
                    return Week.Tuesday;

                case DayOfWeek.Wednesday:
                    return Week.Wednesday;

                case DayOfWeek.Thursday:
                    return Week.Thursday;

                case DayOfWeek.Friday:
                    return Week.Friday;

                case DayOfWeek.Saturday:
                    return Week.Saturday;

                case DayOfWeek.Sunday:
                    return Week.Sunday;

                default:
                    return null;

            }
        }

        public static bool operator <(WeekDay d1, WeekDay d2)
        {
            if (d1 != null && d2 == null) return false;
            if (d1 == null && d2 != null) return true;
            if (d1 == null && d2 == null) return false;

            return d1.Index < d2.Index;
        }

        public static bool operator >(WeekDay d1, WeekDay d2)
        {
            if (d1 != null && d2 == null) return true;
            if (d1 == null && d2 != null) return false;
            if (d1 == null && d2 == null) return false;

            return d1.Index > d2.Index;
        }

        public static bool operator ==(WeekDay d1, WeekDay d2)
        {            
            return d1?.Index == d2?.Index;
        }

        public static bool operator !=(WeekDay d1, WeekDay d2)
        {            
            return d1?.Index != d2?.Index;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null || !(obj is WeekDay)) return false;

            WeekDay w = (WeekDay)obj;

            return w.Value == Value;
        }

        public override int GetHashCode()
        {
            return Index;
        }

    }

    public class Week
    {
        public WeekDay[] GetWeek(bool sundayFirst = true)
        {
            return sundayFirst
                ? GetWeekStartingWith(Sunday)
                : GetWeekStartingWith(Monday);
        }

        public WeekDay[] GetWeekStartingWith(WeekDay day)
        {
            if (day == Sunday) return new WeekDay[] { Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday };
            if (day == Monday) return new WeekDay[] { Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday };
            if (day == Tuesday) return new WeekDay[] { Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday, Monday };
            if (day == Wednesday) return new WeekDay[] { Wednesday, Thursday, Friday, Saturday, Sunday, Monday, Tuesday };
            if (day == Thursday) return new WeekDay[] { Thursday, Friday, Saturday, Sunday, Monday, Tuesday, Wednesday };
            if (day == Friday) return new WeekDay[] { Friday, Saturday, Sunday, Monday, Tuesday, Wednesday, Thursday };
            if (day == Saturday) return new WeekDay[] { Saturday, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday };

            return null;
        }


        public static WeekDay Sunday => new WeekDay { Index = 0, Value = DayOfWeek.Sunday };
        public static WeekDay Monday => new WeekDay { Index = 1, Value = DayOfWeek.Monday };
        public static WeekDay Tuesday => new WeekDay { Index = 2, Value = DayOfWeek.Tuesday };
        public static WeekDay Wednesday => new WeekDay { Index = 3, Value = DayOfWeek.Wednesday };
        public static WeekDay Thursday => new WeekDay { Index = 4, Value = DayOfWeek.Thursday };
        public static WeekDay Friday => new WeekDay { Index = 5, Value = DayOfWeek.Friday };
        public static WeekDay Saturday => new WeekDay { Index = 6, Value = DayOfWeek.Saturday };
    }
}