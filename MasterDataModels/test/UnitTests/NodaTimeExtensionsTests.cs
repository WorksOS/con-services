using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodaTime;
using VSS.MasterData.Models.Internal;

namespace VSS.MasterData.Models.UnitTests
{
  [TestClass]
  public class NodaTimeExtensionsTests
  {
    [TestMethod]
    public void ShouldGetLocalDateTimeForToday()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.Today, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.Today, false, true);
      Assert.AreEqual(startToday, start, "Wrong start for today");
      Assert.AreEqual(endToday, end, "Wrong end for today");
    }

    [TestMethod]
    public void ShouldGetLocalDateTimeForYesterday()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.Yesterday, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.Yesterday, false, true);
      Assert.AreEqual(startYesterday, start, "Wrong start for yesterday");
      Assert.AreEqual(endYesterday, end, "Wrong end for yesterday");
    }

    [TestMethod]
    public void ShouldGetLocalDateTimeForCurrentWeek()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.CurrentWeek, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.CurrentWeek, false, true);
      Assert.AreEqual(startCurrentWeek, start, "Wrong start for current week");
      Assert.AreEqual(endToday, end, "Wrong end for current week");
    }

    [TestMethod]
    public void ShouldGetLocalDateTimeForPreviousWeek()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.PreviousWeek, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.PreviousWeek, false, true);
      Assert.AreEqual(startPreviousWeek, start, "Wrong start for previous week");
      Assert.AreEqual(endPreviousWeek, end, "Wrong end for previous week");
    }
  
    [TestMethod]
    public void ShouldGetLocalDateTimeForCurrentMonth()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.CurrentMonth, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.CurrentMonth, false, true);
      Assert.AreEqual(startCurrentMonth, start, "Wrong start for current month");
      Assert.AreEqual(endToday, end, "Wrong end for current month");
    }

    [TestMethod]
    public void ShouldGetLocalDateTimeForPreviousMonth()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.PreviousMonth, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.PreviousMonth, false, true);
      Assert.AreEqual(startPreviousMonth, start, "Wrong start for previous month");
      Assert.AreEqual(endPreviousMonth, end, "Wrong end for previous month");
    }
  
    [TestMethod]
    public void ShouldGetLocalDateTimeForProjectExtents()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.ProjectExtents, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.ProjectExtents, false, true);
      Assert.AreEqual(localNow, start, "Wrong start for project extents");
      Assert.AreEqual(localNow, end, "Wrong end for project extents");
    }

    [TestMethod]
    public void ShouldGetLocalDateTimeForCustom()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.Custom, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.Custom, false, true);
      Assert.AreEqual(localNow, start, "Wrong start for custom");
      Assert.AreEqual(localNow, end, "Wrong end for custom");
    }

    [TestMethod]
    public void ShouldGetLocalDateTimeForCurrentMonthSpanningDaylightSavingChange()
    {
      //Daylight saving finished on Sunday 1/4/2018 at 2am in NZ      
      var localNowAfterChange = new LocalDateTime(2018, 4, 3, 15, 28, 10);//Tue 3/4/2018
      var start = localNowAfterChange.LocalDateTimeForDateRangeType(DateRangeType.CurrentMonth, true);
      var end = localNowAfterChange.LocalDateTimeForDateRangeType(DateRangeType.CurrentMonth, false);
      Assert.AreEqual(new LocalDateTime(2018, 4, 1, 0, 0, 0), start, "Wrong start for current month");
      Assert.AreEqual(localNowAfterChange, end, "Wrong end for current month");
    }

    [TestMethod]
    public void ShouldGetLocalDateTimeForPreviousWeekSpanningDaylightSavingChange()
    {
      //Daylight saving finished on Sunday 1/4/2018 at 2am in NZ
      var localNowAfterChange = new LocalDateTime(2018, 4, 3, 15, 28, 10);//Tue 3/4/2018
      var start = localNowAfterChange.LocalDateTimeForDateRangeType(DateRangeType.PreviousWeek, true);
      var end = localNowAfterChange.LocalDateTimeForDateRangeType(DateRangeType.PreviousWeek, false);
      Assert.AreEqual(new LocalDateTime(2018, 3, 26, 0, 0, 0), start, "Wrong start for previous week");
      Assert.AreEqual(new LocalDateTime(2018, 4, 1, 23, 59, 59), end, "Wrong end for previous week");
    }

    [TestMethod]
    public void UtcForDateRangeType_Should_return_null_When_timeZoneName_is_null()
    {
      Assert.IsNull(DateRangeType.CurrentWeek.UtcForDateRangeType(null, true));
    }

    [TestMethod]
    public void ShouldGetLocalDateTimeForPriorToYesterday()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.PriorToYesterday, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.PriorToYesterday, false, true);
      Assert.AreEqual(startYesterday.PlusDays(-1), start, "Wrong start for prior to yesterday");
      Assert.AreEqual(endYesterday.PlusDays(-1), end, "Wrong end for prior to yesterday");
    }

    [TestMethod]
    public void ShouldGetLocalDateTimeForPriorToPreviousWeek()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.PriorToPreviousWeek, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.PriorToPreviousWeek, false, true);
      Assert.AreEqual(startPreviousWeek.PlusDays(-7), start, "Wrong start for prior to previous week");
      Assert.AreEqual(endPreviousWeek.PlusDays(-7), end, "Wrong end for prior to previous week");
    }

    [TestMethod]
    public void ShouldGetLocalDateTimeForPriorToPreviousMonth()
    {
      var start = localNow.LocalDateTimeForDateRangeType(DateRangeType.PriorToPreviousMonth, true, true);
      var end = localNow.LocalDateTimeForDateRangeType(DateRangeType.PriorToPreviousMonth, false, true);
      Assert.AreEqual(startPreviousMonth.PlusMonths(-1), start, "Wrong start for prior to previous month");
      Assert.AreEqual(endPreviousMonth.PlusMonths(-1), end, "Wrong end for prior to previous month");
    }

    [TestMethod]
    public void ShouldGetLocalDateTimeForPriorToPreviousWeekSpanningDaylightSavingChange()
    {
      //Daylight saving finished on Sunday 1/4/2018 at 2am in NZ
      var localNowAfterChange = new LocalDateTime(2018, 4, 10, 15, 28, 10);//Tue 10/4/2018
      var start = localNowAfterChange.LocalDateTimeForDateRangeType(DateRangeType.PriorToPreviousWeek, true);
      var end = localNowAfterChange.LocalDateTimeForDateRangeType(DateRangeType.PriorToPreviousWeek, false);
      Assert.AreEqual(new LocalDateTime(2018, 3, 26, 0, 0, 0), start, "Wrong start for prior to previous week");
      Assert.AreEqual(new LocalDateTime(2018, 4, 1, 23, 59, 59), end, "Wrong end for prior to previous week");
    }

    private LocalDateTime localNow = new LocalDateTime(2017, 1, 18, 10, 25, 12);//Wednesday
    private LocalDateTime startToday = new LocalDateTime(2017, 1, 18, 0, 0, 0);
    private LocalDateTime endToday = new LocalDateTime(2017, 1, 18, 23, 59, 59);
    private LocalDateTime startYesterday = new LocalDateTime(2017, 1, 17, 0, 0, 0);
    private LocalDateTime endYesterday = new LocalDateTime(2017, 1, 17, 23, 59, 59);
    private LocalDateTime startCurrentWeek = new LocalDateTime(2017, 1, 16, 0, 0, 0);//Monday
    private LocalDateTime startCurrentMonth = new LocalDateTime(2017, 1, 1, 0, 0, 0);
    private LocalDateTime startPreviousWeek = new LocalDateTime(2017, 1, 9, 0, 0, 0);
    private LocalDateTime endPreviousWeek = new LocalDateTime(2017, 1, 15, 23, 59, 59);
    private LocalDateTime startPreviousMonth = new LocalDateTime(2016, 12, 1, 0, 0, 0);
    private LocalDateTime endPreviousMonth = new LocalDateTime(2016, 12, 31, 23, 59, 59);

    private string ianaTimeZone = "Pacific/Auckland";
    private int offset = 13; //time zone offset for NZ for the 'now' datetime
  }
}
