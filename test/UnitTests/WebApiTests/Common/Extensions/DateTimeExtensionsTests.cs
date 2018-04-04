using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.MasterData.Models.Internal;
using VSS.Productivity3D.Common.Extensions;

namespace VSS.Productivity3D.WebApiTests.Common.Extensions
{
  [TestClass]
  [Ignore("These are borken by DST changes; will be fixed as part of #67231")]
  public class DateTimeExtensionsTests
  {
    [TestMethod]
    public void ShouldGetCurrentWeekMonday()
    {
      //Start with a Monday
      DateTime dateTime = new DateTime(2017, 1, 2);
      for (int i = 0; i < 7; i++)
      {
        Assert.IsTrue(dateTime.CurrentWeekMonday().DayOfWeek == DayOfWeek.Monday, $"{i}: Should return a Monday");
        dateTime = dateTime.AddDays(1);
      }
    }

    [TestMethod]
    public void ShouldGetOffsetForTimeZones()
    {
      var tz = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
      int offset = tz.IsDaylightSavingTime(DateTime.UtcNow) ? 13 : 12;

      var timeZoneName = "Pacific/Auckland";
      Assert.AreEqual(offset, timeZoneName.TimeZoneOffsetFromUtc().TotalHours, "Wrong NZ time zone offset");

      timeZoneName = "Etc/UTC";
      Assert.AreEqual(0, timeZoneName.TimeZoneOffsetFromUtc().TotalHours, "Wrong UTC time zone offset");
    }

    [TestMethod]
    public void ShouldGetDateTimeForToday()
    {
      var start = now.DateTimeForDateRangeType(DateRangeType.Today, true);
      var end = now.DateTimeForDateRangeType(DateRangeType.Today, false);
      Assert.AreEqual(now.Date, start, "Wrong start for today");
      Assert.AreEqual(endToday, end, "Wrong end for today");
    }

    [TestMethod]
    public void ShouldGetDateTimeForYesterday()
    {
      var start = now.DateTimeForDateRangeType(DateRangeType.Yesterday, true);
      var end = now.DateTimeForDateRangeType(DateRangeType.Yesterday, false);
      Assert.AreEqual(now.AddDays(-1).Date, start, "Wrong start for yesterday");
      Assert.AreEqual(now.Date.AddSeconds(-1), end, "Wrong end for yesterday");
    }

    [TestMethod]
    public void ShouldGetDateTimeForCurrentWeek()
    {
      //now is a Wednesday so start of week (Monday) is -2 days
      var start = now.DateTimeForDateRangeType(DateRangeType.CurrentWeek, true);
      var end = now.DateTimeForDateRangeType(DateRangeType.CurrentWeek, false);
      Assert.AreEqual(now.AddDays(-2).Date, start, "Wrong start for current week");
      Assert.AreEqual(endToday, end, "Wrong end for current week");
    }

    [TestMethod]
    public void ShouldGetDateTimeForPreviousWeek()
    {
      //now is a Wednesday so start of previous week (Monday) is -9 days
      //end of previous week is start of current week less a second
      var start = now.DateTimeForDateRangeType(DateRangeType.PreviousWeek, true);
      var end = now.DateTimeForDateRangeType(DateRangeType.PreviousWeek, false);
      Assert.AreEqual(now.AddDays(-9).Date, start, "Wrong start for previous week");
      Assert.AreEqual(now.Date.AddDays(-2).AddSeconds(-1), end, "Wrong end for previous week");
    }

    [TestMethod]
    public void ShouldGetDateTimeForCurrentMonth()
    {
      var start = now.DateTimeForDateRangeType(DateRangeType.CurrentMonth, true);
      var end = now.DateTimeForDateRangeType(DateRangeType.CurrentMonth, false);
      Assert.AreEqual(new DateTime(now.Year, now.Month, 1), start, "Wrong start for current month");
      Assert.AreEqual(endToday, end, "Wrong end for current month");
    }

    [TestMethod]
    public void ShouldGetDateTimeForPreviousMonth()
    {
      var start = now.DateTimeForDateRangeType(DateRangeType.PreviousMonth, true);
      var end = now.DateTimeForDateRangeType(DateRangeType.PreviousMonth, false);
      var prevMonth = now.AddMonths(-1);
      Assert.AreEqual(new DateTime(prevMonth.Year, prevMonth.Month, 1), start, "Wrong start for previous month");
      //End of previous month is start of current month  less a second
      Assert.AreEqual(new DateTime(now.Year, now.Month, 1).AddSeconds(-1), end, "Wrong end for previous month");
    }

    [TestMethod]
    public void ShouldGetDateTimeForProjectExtents()
    {
      var start = now.DateTimeForDateRangeType(DateRangeType.ProjectExtents, true);
      var end = now.DateTimeForDateRangeType(DateRangeType.ProjectExtents, false);
      Assert.AreEqual(DateTime.MinValue, start, "Wrong start for project extents");
      Assert.AreEqual(DateTime.MinValue, end, "Wrong end for project extents");
    }

    [TestMethod]
    public void ShouldGetDateTimeForCustom()
    {
      var start = now.DateTimeForDateRangeType(DateRangeType.Custom, true);
      var end = now.DateTimeForDateRangeType(DateRangeType.Custom, false);
      Assert.AreEqual(DateTime.MinValue, start, "Wrong start for custom");
      Assert.AreEqual(DateTime.MinValue, end, "Wrong end for custom");
    }

    [TestMethod]
    public void ShouldGetUtcForToday()
    {
      var startUtc = utcNow.UtcForDateRangeType(DateRangeType.Today, ianaTimeZone, true);
      var endUtc = utcNow.UtcForDateRangeType(DateRangeType.Today, ianaTimeZone, false);
      Assert.AreEqual(now.Date.AddHours(-offset), startUtc, "Wrong startUtc for today");
      Assert.AreEqual(endToday.AddHours(-offset), endUtc, "Wrong endUtc for today");
    }

    [TestMethod]
    public void ShouldGetUtcForYesterday()
    {
      var startUtc = utcNow.UtcForDateRangeType(DateRangeType.Yesterday, ianaTimeZone, true);
      var endUtc = utcNow.UtcForDateRangeType(DateRangeType.Yesterday, ianaTimeZone, false);
      Assert.AreEqual(now.AddDays(-1).Date.AddHours(-offset), startUtc, "Wrong startUtc for yesterday");
      Assert.AreEqual(now.Date.AddSeconds(-1).AddHours(-offset), endUtc, "Wrong endUtc for yesterday");
    }

    [TestMethod]
    public void ShouldGetUtcForCurrentWeek()
    {
      var startUtc = utcNow.UtcForDateRangeType(DateRangeType.CurrentWeek, ianaTimeZone, true);
      var endUtc = utcNow.UtcForDateRangeType(DateRangeType.CurrentWeek, ianaTimeZone, false);
      Assert.AreEqual(now.AddDays(-2).Date.AddHours(-offset), startUtc, "Wrong startUtc for current week");
      Assert.AreEqual(endToday.AddHours(-offset), endUtc, "Wrong endUtc for current week");
    }

    [TestMethod]
    public void ShouldGetUtcForPreviousWeek()
    {
      var startUtc = utcNow.UtcForDateRangeType(DateRangeType.PreviousWeek, ianaTimeZone, true);
      var endUtc = utcNow.UtcForDateRangeType(DateRangeType.PreviousWeek, ianaTimeZone, false);
      Assert.AreEqual(now.AddDays(-9).Date.AddHours(-offset), startUtc, "Wrong startUtc for previous week");
      Assert.AreEqual(now.Date.AddDays(-2).AddSeconds(-1).AddHours(-offset), endUtc, "Wrong endUtc for previous week");
    }

    [TestMethod]
    public void ShouldGetUtcForCurrentMonth()
    {
      var startUtc = utcNow.UtcForDateRangeType(DateRangeType.CurrentMonth, ianaTimeZone, true);
      var endUtc = utcNow.UtcForDateRangeType(DateRangeType.CurrentMonth, ianaTimeZone, false);
      Assert.AreEqual(new DateTime(now.Year, now.Month, 1).AddHours(-offset), startUtc, "Wrong startUtc for current month");
      Assert.AreEqual(endToday.AddHours(-offset), endUtc, "Wrong endUtc for current month");
    }

    [TestMethod]
    public void ShouldGetUtcForPreviousMonth()
    {
      var startUtc = utcNow.UtcForDateRangeType(DateRangeType.PreviousMonth, ianaTimeZone, true);
      var endUtc = utcNow.UtcForDateRangeType(DateRangeType.PreviousMonth, ianaTimeZone, false);
      var prevMonth = now.AddMonths(-1);
      Assert.AreEqual(new DateTime(prevMonth.Year, prevMonth.Month, 1).AddHours(-offset), startUtc, "Wrong startUtc for previous month");
      Assert.AreEqual(new DateTime(now.Year, now.Month, 1).AddSeconds(-1).AddHours(-offset), endUtc, "Wrong endUtc for previous month");
    }

    [TestMethod]
    public void ShouldGetUtcForProjectExtents()
    {
      var startUtc = utcNow.UtcForDateRangeType(DateRangeType.ProjectExtents, ianaTimeZone, true);
      var endUtc = utcNow.UtcForDateRangeType(DateRangeType.ProjectExtents, ianaTimeZone, false);
      Assert.IsNull(startUtc, "Wrong startUtc for project extents");
      Assert.IsNull(endUtc, "Wrong endUtc for project extents");
    }

    [TestMethod]
    public void ShouldGetUtcForCustom()
    {
      var startUtc = utcNow.UtcForDateRangeType(DateRangeType.Custom, ianaTimeZone, true);
      var endUtc = utcNow.UtcForDateRangeType(DateRangeType.Custom, ianaTimeZone, false);
      Assert.IsNull(startUtc, "Wrong startUtc for custom");
      Assert.IsNull(endUtc, "Wrong endUtc for custom");
    }
    
    [TestMethod]
    public void UtcForDateRangeType_Should_return_null_When_timeZoneName_is_null()
    {
      Assert.IsNull(utcNow.UtcForDateRangeType(DateRangeType.CurrentWeek, null, true));
    }

    private DateTime now = new DateTime(2017, 1, 18, 10, 25, 12);//Wednesday
    private DateTime endToday = new DateTime(2017, 1, 18, 10, 25, 12).Date.AddDays(1).AddSeconds(-1);
    private DateTime utcNow = new DateTime(2017, 1, 18, 10, 25, 12).AddHours(-13);// -13 = offset for NZ time zone for 'now' datetime
    private string ianaTimeZone = "Pacific/Auckland";
    private int offset = 13; //time zone offset for NZ for the 'now' datetime
  }
}