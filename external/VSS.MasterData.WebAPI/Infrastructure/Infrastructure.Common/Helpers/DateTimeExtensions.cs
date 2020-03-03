using System;

namespace Infrastructure.Common.Helpers
{
    public static class DateTimeExtensions
    {
        public static string ToDateTimeStringWithYearMonthDayFormat(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
