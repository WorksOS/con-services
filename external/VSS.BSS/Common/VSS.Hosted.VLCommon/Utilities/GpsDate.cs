using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon
{
  public struct GpsDate
  {
    public UInt32 GpsRollOverCount;
    public UInt32 GpsWeek;
    public UInt32 GpsSecond;

    public UInt32 TotalGpsWeeks
    {
      get
      {
        return GpsWeek + (GpsRollOverCount * Constants.GpsWeeksPerRolloverPeriod);
      }

      set
      {
        GpsRollOverCount = value / Constants.GpsWeeksPerRolloverPeriod;
        GpsWeek = value % Constants.GpsWeeksPerRolloverPeriod;
      }
    }

    public GpsDate(DateTime date)
    {
      this.GpsRollOverCount = this.GpsWeek = this.GpsSecond = 0;
      this.Date = date;
    }
    public GpsDate(uint totalGpsWeeks, uint gpsSecond)
    {
      this.GpsRollOverCount = this.GpsWeek = 0;
      this.GpsSecond = gpsSecond;
      this.TotalGpsWeeks = totalGpsWeeks;
    }

    public class Constants
    {
      public const UInt32 DaysPerWeek = 7;
      public const UInt32 SecondsPerMinute = 60;
      public const UInt32 SecondsPerHour = 60 * SecondsPerMinute;
      public const UInt32 SecondsPerDay = 24 * SecondsPerHour;
      public const UInt32 SecondsPerWeek = DaysPerWeek * SecondsPerDay;

      public const UInt32 GpsWeeksPerRolloverPeriod = 1024;

      public static readonly DateTime GpsEpochDay = new DateTime(1980, 1, 6);
    }

    public void SetFromGpsSecond(UInt32 seconds)
    {
      // Assign the UtcDate from just the seconds.  Effectively take DateTime.Now to a UtcDate
      // then assign the seconds to what was passed in.  If the seconds are greater than the
      // GpsSeconds for DateTime.Now then it is actually from a week ago so set the seconds, but
      // decrement the week.  If the week goes negative, decrement the rollover count and set
      // the week to the rollovers per period minus 1.

      Date = DateTime.UtcNow;

      if (GpsSecond < seconds)
      {
        GpsWeek--;

        if (GpsWeek < 0)
        {
          unchecked { GpsWeek = (uint)((int)Constants.GpsWeeksPerRolloverPeriod - 1); }
          GpsRollOverCount--;
        }
      }

      GpsSecond = seconds;
    }

    public static UInt32 CalculateSecondsFromDate(DateTime date)
    {
      GpsDate tempDate = new GpsDate();

      tempDate.Date = date;

      return tempDate.GpsSecond;
    }

    public DateTime Date
    {
      get
      {
        DateTime result = Constants.GpsEpochDay;

        result = result.AddSeconds(GpsSecond);
        result = result.AddDays(TotalGpsWeeks * Constants.DaysPerWeek);

        return result;
      }

      set
      {
        // Get the number of days since January 6, 1980 (epoch day for GPS)

        TimeSpan span = value.Subtract(Constants.GpsEpochDay);
        UInt32 totalWeeks = (UInt32)span.Days / Constants.DaysPerWeek;

        TotalGpsWeeks = totalWeeks;
        GpsSecond = (UInt32)span.TotalSeconds % Constants.SecondsPerWeek;
      }
    }

    public override string ToString()
    {
      return Date.ToString();
    }
  }
}
