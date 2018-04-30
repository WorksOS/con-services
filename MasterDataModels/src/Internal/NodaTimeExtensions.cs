using System;
using NodaTime;
using NodaTime.Extensions;

namespace VSS.MasterData.Models.Internal
{
  public static class NodaTimeExtensions
  {
    /// <summary>
    /// Get the start or end date time in a time zone for the specified date range type.
    /// </summary>
    /// <param name="nowInTimeZone">Now in time zone</param>
    /// <param name="dateRangeType">The date range type (today, current week etc.)</param>
    /// <param name="isStart">True for start and false for end of date range</param>
    /// <param name="useEndOfCurrentDay">Flag to indicate if end of current periods are now or the end of the current day</param>
    /// <returns>The start or end date time for the range in the time zone</returns>
    public static LocalDateTime LocalDateTimeForDateRangeType(this LocalDateTime nowInTimeZone, DateRangeType dateRangeType, bool isStart, bool useEndOfCurrentDay = false)
    {
      LocalDateTime startToday = nowInTimeZone.PlusTicks(-nowInTimeZone.TickOfDay);
      LocalDateTime endToday = startToday.PlusDays(1).PlusSeconds(-1);
      var currentEnd = useEndOfCurrentDay ? endToday : nowInTimeZone;

      var startThisWeek = startToday.With(DateAdjusters.PreviousOrSame(IsoDayOfWeek.Monday));
      var startThisMonth = startToday.With(DateAdjusters.StartOfMonth);

      LocalDateTime dateTimeInTimeZone = nowInTimeZone;
      switch (dateRangeType)
      {
        case DateRangeType.Today:
          dateTimeInTimeZone = isStart ? startToday : currentEnd;
          break;
        case DateRangeType.Yesterday:
        case DateRangeType.PriorToYesterday:
          dateTimeInTimeZone = isStart ? startToday.PlusDays(-1) : startToday.PlusSeconds(-1);
          if (dateRangeType == DateRangeType.PriorToYesterday)
          {
            dateTimeInTimeZone = dateTimeInTimeZone.PlusDays(-1);
          }
          break;
        case DateRangeType.CurrentWeek:
          dateTimeInTimeZone = isStart ? startThisWeek : currentEnd;
          break;
        case DateRangeType.PreviousWeek:
        case DateRangeType.PriorToPreviousWeek:
          dateTimeInTimeZone = isStart ? startThisWeek.PlusDays(-7) : startThisWeek.PlusSeconds(-1);
          if (dateRangeType == DateRangeType.PriorToPreviousWeek)
          {
            dateTimeInTimeZone = dateTimeInTimeZone.PlusDays(-7);
          }
          break;
        case DateRangeType.CurrentMonth:
          dateTimeInTimeZone = isStart ? startThisMonth : currentEnd;
          break;
        case DateRangeType.PreviousMonth:
        case DateRangeType.PriorToPreviousMonth:
          dateTimeInTimeZone = isStart ? startThisMonth.PlusMonths(-1) : startThisMonth.PlusSeconds(-1);
          if (dateRangeType == DateRangeType.PriorToPreviousMonth)
          {
            dateTimeInTimeZone = dateTimeInTimeZone.PlusMonths(-1);
          }
          break;
        case DateRangeType.ProjectExtents:
        case DateRangeType.Custom:
          //do nothing
          break;
      }
      return dateTimeInTimeZone;
    }

    /// <summary>
    /// Get the start or end date time in UTC for the specified date range type in the specified time zone.
    /// </summary>
    /// <param name="dateRangeType">The date range type (today, current week etc.)</param>
    /// <param name="ianaTimeZoneName">The IANA time zone name</param>
    /// <param name="isStart">True for start and false for end of date range</param>
    /// <param name="useEndOfCurrentDay">Flag to indicate if end of current periods are now or the end of the current day</param>
    /// <returns>The start or end UTC for the range in the time zone</returns>
    public static DateTime? UtcForDateRangeType(this DateRangeType dateRangeType, string ianaTimeZoneName, bool isStart, bool useEndOfCurrentDay = false)
    {
      if (dateRangeType == DateRangeType.Custom || dateRangeType == DateRangeType.ProjectExtents || string.IsNullOrEmpty(ianaTimeZoneName))
      {
        return null;
      }

      DateTimeZone timeZone = DateTimeZoneProviders.Tzdb[ianaTimeZoneName];
      ZonedClock clock = SystemClock.Instance.InZone(timeZone);
      LocalDateTime localNow = clock.GetCurrentLocalDateTime();
      var local = localNow.LocalDateTimeForDateRangeType(dateRangeType, isStart);
      var zoned = timeZone.AtLeniently(local);
      return zoned.ToDateTimeUtc();
    }
  }
}
