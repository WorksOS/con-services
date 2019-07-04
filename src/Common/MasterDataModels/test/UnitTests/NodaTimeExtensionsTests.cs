using System;
using NodaTime;
using VSS.MasterData.Models.Internal;
using Xunit;

namespace VSS.MasterData.Models.UnitTests
{
  public class NodaTimeExtensionsTests
  {
    private readonly LocalDateTime localNow = new LocalDateTime(2017, 1, 18, 10, 25, 12);//Wednesday
    private readonly LocalDateTime startToday = new LocalDateTime(2017, 1, 18, 0, 0, 0);
    private readonly LocalDateTime endToday = new LocalDateTime(2017, 1, 18, 23, 59, 59);
    private readonly LocalDateTime startYesterday = new LocalDateTime(2017, 1, 17, 0, 0, 0);
    private readonly LocalDateTime endYesterday = new LocalDateTime(2017, 1, 17, 23, 59, 59);
    private readonly LocalDateTime startCurrentWeek = new LocalDateTime(2017, 1, 16, 0, 0, 0);//Monday
    private readonly LocalDateTime startCurrentMonth = new LocalDateTime(2017, 1, 1, 0, 0, 0);
    private readonly LocalDateTime startPreviousWeek = new LocalDateTime(2017, 1, 9, 0, 0, 0);
    private readonly LocalDateTime endPreviousWeek = new LocalDateTime(2017, 1, 15, 23, 59, 59);
    private readonly LocalDateTime startPreviousMonth = new LocalDateTime(2016, 12, 1, 0, 0, 0);
    private readonly LocalDateTime endPreviousMonth = new LocalDateTime(2016, 12, 31, 23, 59, 59);

    [Fact]
    public void ShouldGetLocalDateTimeForToday()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.Today, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.Today, false, true);
      Assert.Equal(startToday, start);
      Assert.Equal(endToday, end);
    }

    [Fact]
    public void ShouldGetLocalDateTimeForYesterday()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.Yesterday, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.Yesterday, false, true);
      Assert.Equal(startYesterday, start);
      Assert.Equal(endYesterday, end);
    }

    [Fact]
    public void ShouldGetLocalDateTimeForCurrentWeek()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.CurrentWeek, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.CurrentWeek, false, true);
      Assert.Equal(startCurrentWeek, start);
      Assert.Equal(endToday, end);
    }

    [Fact]
    public void ShouldGetLocalDateTimeForPreviousWeek()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.PreviousWeek, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.PreviousWeek, false, true);
      Assert.Equal(startPreviousWeek, start);
      Assert.Equal(endPreviousWeek, end);
    }

    [Fact]
    public void ShouldGetLocalDateTimeForCurrentMonth()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.CurrentMonth, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.CurrentMonth, false, true);
      Assert.Equal(startCurrentMonth, start);
      Assert.Equal(endToday, end);
    }

    [Fact]
    public void ShouldGetLocalDateTimeForPreviousMonth()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.PreviousMonth, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.PreviousMonth, false, true);
      Assert.Equal(startPreviousMonth, start);
      Assert.Equal(endPreviousMonth, end);
    }

    [Fact]
    public void ShouldGetLocalDateTimeForProjectExtents()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.ProjectExtents, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.ProjectExtents, false, true);
      Assert.Equal(localNow, start);
      Assert.Equal(localNow, end);
    }

    [Fact]
    public void ShouldGetLocalDateTimeForCustom()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.Custom, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.Custom, false, true);
      Assert.Equal(localNow, start);
      Assert.Equal(localNow, end);
    }

    [Fact]
    public void ShouldGetLocalDateTimeForCurrentMonthSpanningDaylightSavingChange()
    {
      //Daylight saving finished on Sunday 1/4/2018 at 2am in NZ      
      var localNowAfterChange = new LocalDateTime(2018, 4, 3, 15, 28, 10);//Tue 3/4/2018
      var start = localNowAfterChange.LocalDateTimeForDateRangeType(DateRangeType.CurrentMonth, true);
      var end = localNowAfterChange.LocalDateTimeForDateRangeType(DateRangeType.CurrentMonth, false);
      Assert.Equal(new LocalDateTime(2018, 4, 1, 0, 0, 0), start);
      Assert.Equal(localNowAfterChange, end);
    }

    [Fact]
    public void ShouldGetLocalDateTimeForPreviousWeekSpanningDaylightSavingChange()
    {
      //Daylight saving finished on Sunday 1/4/2018 at 2am in NZ
      var localNowAfterChange = new LocalDateTime(2018, 4, 3, 15, 28, 10);//Tue 3/4/2018
      var start = localNowAfterChange.LocalDateTimeForDateRangeType(DateRangeType.PreviousWeek, true);
      var end = localNowAfterChange.LocalDateTimeForDateRangeType(DateRangeType.PreviousWeek, false);
      Assert.Equal(new LocalDateTime(2018, 3, 26, 0, 0, 0), start);
      Assert.Equal(new LocalDateTime(2018, 4, 1, 23, 59, 59), end);
    }

    [Fact]
    public void UtcForDateRangeType_Should_return_null_When_timeZoneName_is_null()
    {
      Assert.Null(DateRangeType.CurrentWeek.UtcForDateRangeType(null, true));
    }

    [Fact]
    public void ShouldGetLocalDateTimeForPriorToYesterday()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.PriorToYesterday, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.PriorToYesterday, false, true);
      Assert.Equal(startYesterday.PlusDays(-1), start);
      Assert.Equal(endYesterday.PlusDays(-1), end);
    }

    [Fact]
    public void ShouldGetLocalDateTimeForPriorToPreviousWeek()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.PriorToPreviousWeek, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.PriorToPreviousWeek, false, true);
      Assert.Equal(startPreviousWeek.PlusDays(-7), start);
      Assert.Equal(endPreviousWeek.PlusDays(-7), end);
    }

    [Fact]
    public void ShouldGetLocalDateTimeForPriorToPreviousMonth()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.PriorToPreviousMonth, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.PriorToPreviousMonth, false, true);
      Assert.Equal(startPreviousMonth.PlusMonths(-1), start);
      Assert.Equal(endPreviousMonth.PlusMonths(-1), end);
    }

    [Fact]
    public void ShouldGetLocalDateTimeForPriorToPreviousWeekSpanningDaylightSavingChange()
    {
      //Daylight saving finished on Sunday 1/4/2018 at 2am in NZ
      var localNowAfterChange = new LocalDateTime(2018, 4, 10, 15, 28, 10);//Tue 10/4/2018
      var start = localNowAfterChange.LocalDateTimeForDateRangeType(DateRangeType.PriorToPreviousWeek, true);
      var end = localNowAfterChange.LocalDateTimeForDateRangeType(DateRangeType.PriorToPreviousWeek, false);
      Assert.Equal(new LocalDateTime(2018, 3, 26, 0, 0, 0), start);
      Assert.Equal(new LocalDateTime(2018, 4, 1, 23, 59, 59), end);
    }

    [Fact]
    public void ShouldGetLocalDateTimeForUtcUsingTimeZone()
    {
      var dateTimeUtc = new DateTime(2018, 5, 10, 15, 28, 10);
      var ianaTimeZone = "America/Chicago"; // Central Standard Time GMT-6
      var dateTimeLocal = dateTimeUtc.ToLocalDateTime(ianaTimeZone);
      Assert.NotNull(dateTimeLocal);
      Assert.Equal(dateTimeUtc.AddHours(-5), dateTimeLocal);

      var endDate = dateTimeLocal.Value.Date;
      Assert.Equal(dateTimeUtc.Date, endDate.Date);
    }

    [Fact]
    public void ShouldGetLocalDateTimeForUtcUsingTimeZone_DaylightSavings()
    {
      var dateTimeUtc = new DateTime(2018, 2, 10, 15, 28, 10);
      var ianaTimeZone = "America/Chicago";
      var dateTimeLocal = dateTimeUtc.ToLocalDateTime(ianaTimeZone);
      Assert.NotNull(dateTimeLocal);
      Assert.Equal(dateTimeUtc.AddHours(-6), dateTimeLocal);

      var endDate = dateTimeLocal.Value.Date;
      Assert.Equal(dateTimeUtc.Date, endDate.Date);
    }

    [Fact]
    public void ShouldGetLocalDateTimeForUtcUsingTimeZone_SpanMidnight()
    {
      var dateTimeUtc = new DateTime(2018, 2, 10, 1, 45, 34);
      var ianaTimeZone = "America/Chicago";
      var dateTimeLocal = dateTimeUtc.ToLocalDateTime(ianaTimeZone);
      Assert.NotNull(dateTimeLocal);
      Assert.Equal(dateTimeUtc.AddHours(-6), dateTimeLocal);

      var endDate = dateTimeLocal.Value.Date;
      Assert.Equal(dateTimeUtc.Date.AddDays(-1), endDate.Date);
    }

    [Fact]
    public void ShouldGetLocalDateTimeForUtcUsing_InvalidTimeZone()
    {
      var dateTimeUtc = new DateTime(2018, 5, 10, 15, 28, 10);
      var ianaTimeZone = "Blah/DeBlah";
      var dateTimeLocal = dateTimeUtc.ToLocalDateTime(ianaTimeZone);
      Assert.Null(dateTimeLocal);
    }
  }
}
