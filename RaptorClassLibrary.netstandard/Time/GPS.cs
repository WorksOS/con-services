using System;

namespace VSS.TRex.Time
{
    /// <summary>
    /// Utility functions related to GPS time
    /// </summary>
    public static class GPS
    {
        private const int secsPerMin = 60;
        private const int minsPerHour = 60;
        private const int secsPerHour = minsPerHour * secsPerMin;
        private const int secsPerDay = 24 * secsPerHour;
        private const int mSecsPerMin = secsPerMin * 1000;
        private const int mSecsPerHour = minsPerHour * mSecsPerMin;
        private const int mSecsPerDay = secsPerDay * 1000;
        private const int mSecsPerWeek = 7 * mSecsPerDay;

        /// <summary>
        /// The GPS time origin
        /// The first day of GPS starts at midnight on the 5/6 Jan, 1980.
        /// </summary>
        private static DateTime kGPSOriginDate = new DateTime(1980, 1, 6);

        /// <summary>
        /// Returns a TimeSpan containing the GMT offset in the current local at the current time
        /// </summary>
        /// <returns></returns>
        public static TimeSpan GetLocalGMTOffset() => TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);

        /// <summary>
        /// Returns a TimeSpan containing the GMT offset in the current local as at the supplied time
        /// </summary>
        /// <returns></returns>
        public static TimeSpan GetLocalGMTOffset(DateTime AtTime) => TimeZoneInfo.Local.GetUtcOffset(AtTime);

        /// <summary>
        /// Takes a time defined as a GPS week number and a count of elapsed milliseconds within that week
        /// and convertes it to a DateTime with respect to the .Net time origin
        /// </summary>
        /// <param name="weekNumber"></param>
        /// <param name="milliSecondsInWeek"></param>
        /// <returns></returns>
        public static DateTime GPSOriginTimeToDateTime(int weekNumber, uint milliSecondsInWeek)
        {
            int days = 7 * weekNumber;
            int ms = (int)milliSecondsInWeek;
            int hours = ms / mSecsPerHour;
            ms = ms % mSecsPerHour;
            int minutes = ms / mSecsPerMin;
            ms = ms % mSecsPerMin;
            int seconds = ms / 1000;
            ms = ms % 1000;

            return kGPSOriginDate.Add(new TimeSpan(days, hours, minutes, seconds, ms));
        }

        /// <summary>
        /// Converts a DateTime into the GPS Week and milli-seconds in week relative to the GPS time origin
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="weekNumber"></param>
        /// <param name="milliSecondsInWeek"></param>
        public static void DateTimeToGPSOriginTime(DateTime dateTime, out uint weekNumber, out uint milliSecondsInWeek)
        {
            if (dateTime < kGPSOriginDate)
            {
                throw new ArgumentException("Date to be converted to GPS date is before the GPS date origin", "dateTime");
            }

            TimeSpan span = dateTime - kGPSOriginDate;
            long ms = (long)Math.Round(span.TotalMilliseconds);

            weekNumber = (uint)(ms / mSecsPerWeek);
            milliSecondsInWeek = (uint)(ms % mSecsPerWeek);
        }
    }
}
