using System;

namespace TestUtility
{
    public static class DateTimeExtensions
    {
        public static string ToSafeString(this object obj)
        {
            return (obj ?? string.Empty).ToString();
        }

        #region KeyDates

        public const int NullKeyDate = 99991231;


        public static int KeyDate(this DateTime value)
        {
            return (value.Year * 10000) + (value.Month * 100) + (value.Day);
        }

        public static int DaysDifferenceKeyDate(this int firstKeyDate, int secondKeyDate)
        {
            var daysDiff =
              (FromKeyDate(secondKeyDate) - FromKeyDate(firstKeyDate)).Days;
            return daysDiff;
        }

        public static DateTime FromKeyDate(this int keyDate)
        {
            return new DateTime(keyDate.KeyDateYear(), keyDate.KeyDateMonth(), keyDate.KeyDateDay());
        }

        /// <summary>
        ///   yy = keyDate / 10000
        /// </summary>
        /// <param name="keyDate"></param>
        /// <returns></returns>
        public static int KeyDateYear(this int keyDate)
        {
            if (keyDate < 19000000 || keyDate > 99991231)
                throw new ArgumentOutOfRangeException(string.Format("Invalid keyDate {0}", keyDate));

            return keyDate / 10000;
        }

        /// <summary>
        ///   mm = (keyDate - (yy * 10000)) / 100
        /// </summary>
        /// <param name="keyDate"></param>
        /// <returns></returns>
        public static int KeyDateMonth(this int keyDate)
        {
            if (keyDate < 19000000 || keyDate > 99991231)
                throw new ArgumentOutOfRangeException(string.Format("Invalid keyDate {0}", keyDate));

            return (keyDate - (keyDate.KeyDateYear() * 10000)) / 100;
        }

        /// <summary>
        ///   dd = (keyDate - (yy * 10000) - (MM * 100))
        /// </summary>
        /// <param name="keyDate"></param>
        /// <returns></returns>
        public static int KeyDateDay(this int keyDate)
        {
            if (keyDate < 19000000 || keyDate > 99991231)
                throw new ArgumentOutOfRangeException(string.Format("Invalid keyDate {0}", keyDate));

            return (keyDate - (keyDate.KeyDateYear() * 10000) - (keyDate.KeyDateMonth() * 100));
        }

        public static int AddDaysToKeyDate(this int keyDate, int countDays)
        {
            var newOne = new DateTime(keyDate.KeyDateYear(), keyDate.KeyDateMonth(), keyDate.KeyDateDay()).AddDays((countDays));
            return newOne.KeyDate();
        }

        #endregion

        #region Event Device Times

        /// <summary>
        ///   The string already contains the date time in the local time zone.
        ///   The offset just tells us what the time zone is.
        ///   Don't use DateTime.Parse as it assumes the date time is UTC and applies the offset so giving a double conversion.
        /// </summary>
        /// <param name="iso8601EventDeviceTime"></param>
        /// <returns></returns>
        public static DateTime ParseEventDeviceTime(string iso8601EventDeviceTime)
        {
            return DateTimeOffset.Parse(iso8601EventDeviceTime).DateTime;
        }

        public static int ParseEventDeviceTimeOffsetMinutes(string iso8601EventDeviceTime)
        {
            return (int)DateTimeOffset.Parse(iso8601EventDeviceTime).Offset.TotalMinutes;
        }

        //public static DateTime GetEventDeviceTime(TimestampDetail timestamp)
        //{
        //    return ParseEventDeviceTime(timestamp.Iso8601EventDeviceTime);
        //}

        public static string ToIso8601DateTime(DateTime dateTime, double hoursOffset)
        {
            return new DateTimeOffset(dateTime.AddHours(hoursOffset).Ticks, TimeSpan.FromHours(hoursOffset)).ToString("O");
        }

        #endregion
    }
}
