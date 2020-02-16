using System;

namespace VSS.Hosted.VLCommon
{
  public struct UtcSeconds
  {
    static UtcSeconds()
    {
      kDriftAllowanceMinutes = 10;
    }

    public static readonly Int32 kDriftAllowanceMinutes;

    public UInt32 Seconds;

    public DateTime GetDateRelativeToAnother(DateTime relativeNow)
    {
      GpsDate utcNow = new GpsDate();
      DateTime result = new DateTime(relativeNow.Year, relativeNow.Month, relativeNow.Day, relativeNow.Hour, relativeNow.Minute, relativeNow.Second, 0);

      utcNow.Date = result;

      // Now adjust given the UtcSeconds.  Basically subtract from the result
      // the number of seconds between the current time's GPS seconds and the 
      // given seconds.  If the given seconds are ahead of the current time's
      // GPS seconds, then we actually have to go back a week as well.

      int secondsAgo = (int)utcNow.GpsSecond - (int)Seconds;

      // Normally, we'd compare "< 0" however we want to allow the device to
      // be slightly ahead of our timebase and still get a good calculation.
      // Since a 'reset' device will generally be 13 seconds ahead of 'now'
      // (a constant in the world of GPS time), I allow for 10 minutes since
      // we may drift and this is what the legacy code supported.

      if (secondsAgo < -(kDriftAllowanceMinutes * 60))
      {
        secondsAgo += (int)GpsDate.Constants.SecondsPerWeek;
      }

      return result.AddSeconds(-secondsAgo);
    }

    public static implicit operator DateTime(UtcSeconds utcSeconds)
    {
      return utcSeconds.Date;
    }

    public DateTime Date
    {
      get
      {
        return GetDateRelativeToAnother(DateTime.UtcNow);
      }

      set
      {
        // Assign the seconds from the date.  We use a UtcDate to get the seconds.

        GpsDate newTime = new GpsDate();

        newTime.Date = value;

        Seconds = newTime.GpsSecond;
      }
    }

    public override string ToString()
    {
      return Seconds.ToString();
    }
  }
}
