using System;
using System.Globalization;

namespace LogAnalytics.Client.Helper
{
    public static class DateUtils
    {
        public static string ToISO8601(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }

        public static DateTime FromModSecDateTime(this string dateTime)
        {
            return DateTime.ParseExact(dateTime, "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture);
        }
    }
}