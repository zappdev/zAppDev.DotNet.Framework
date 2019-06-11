using System;
using System.Globalization;

namespace CLMS.Framework.Data.Domain
{
    public static class NullableMethodExtensions
    {
        public static string ToString<T>(this T? target, string format) where T : struct, IFormattable
        {
            return target?.ToString(format, CultureInfo.InvariantCulture);
        }

        #region DateTime Compare Methods

        public static int CompareTo(this DateTime me, DateTime other)
        {
            return DateTime.Compare(me, other);
        }

        public static int CompareTo(this DateTime? me, DateTime other)
        {
            return me.GetValueOrDefault(System.Data.SqlTypes.SqlDateTime.MinValue.Value).CompareTo(other);
        }

        public static int CompareTo(this DateTime me, DateTime? other)
        {
            return me.CompareTo(other.GetValueOrDefault(System.Data.SqlTypes.SqlDateTime.MinValue.Value));
        }

        public static int CompareTo(this DateTime? me, DateTime? other)
        {
            return me.CompareTo(other.GetValueOrDefault(System.Data.SqlTypes.SqlDateTime.MinValue.Value));
        }

        #endregion

        // These methods are handy when coding facilty fails to detect a NON-nullable identifier
        // and appends a .GetValueOrDefault(x) postfix. This would normally result in a compilation error,
        // but these methods prevent it.
        // NOTE: nHibernate may throw a "method not suported" exception at RUNTIME if any of these methods
        // are used inside a query predicate.

        #region Fail-safe methods

        public static object GetValueOrDefault(this object me, object defaultValue)
        {
            return me;
        }

        public static int GetValueOrDefault(this int me, object defaultValue)
        {
            return me;
        }

        public static long GetValueOrDefault(this long me, object defaultValue)
        {
            return me;
        }

        public static bool GetValueOrDefault(this bool me, object defaultValue)
        {
            return me;
        }

        public static double GetValueOrDefault(this double me, object defaultValue)
        {
            return me;
        }

        public static decimal GetValueOrDefault(this decimal me, object defaultValue)
        {
            return me;
        }

        public static float GetValueOrDefault(this float me, object defaultValue)
        {
            return me;
        }

        public static char GetValueOrDefault(this char me, object defaultValue)
        {
            return me;
        }

        public static byte GetValueOrDefault(this byte me, object defaultValue)
        {
            return me;
        }

        #endregion
    }
}
