namespace CLMS.Framework.Utilities.CronExpressionDescriptor
{
    public enum DescriptionTypeEnum
    {
        FULL,
        TIMEOFDAY,
        SECONDS,
        MINUTES,
        HOURS,
        DAYOFWEEK,
        MONTH,
        DAYOFMONTH,
        YEAR
    }

    public class Options
    {
        public Options()
        {
            this.ThrowExceptionOnParseError = true;
            this.Verbose = false;
            this.DayOfWeekStartIndexZero = true;
        }

        public bool ThrowExceptionOnParseError { get; set; }
        public bool Verbose { get; set; }
        public bool DayOfWeekStartIndexZero { get; set; }
        public bool? Use24HourTimeFormat { get; set; }
        public string Locale { get; set; }
    }
}
