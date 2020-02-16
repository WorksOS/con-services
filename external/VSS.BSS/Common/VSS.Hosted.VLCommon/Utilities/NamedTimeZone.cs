using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Microsoft.Win32;
using VSS.Hosted.VLCommon.Utilities;

namespace VSS.Hosted.VLCommon
{
  public class TimeZonesRegistry
  {
    public static string RootKey
    {
      get { return @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones\"; }  // for WinNT, 2000, etc
    }

    /// <summary>
    /// Return the time zone registry key that matches the standard time zone name parameter.
    /// </summary>
    /// <param name="standardName">Standard time zone name.</param>
    /// <returns>Time zone registry key, or null of standardName does not match an OS time zone.</returns>
    internal static RegistryKey GetTimeZoneKey( string standardName )
    {
      if ( standardName == string.Empty )
      {
        return null;
      }

      RegistryKey zoneKey = Registry.LocalMachine.OpenSubKey( RootKey + standardName );

      if ( zoneKey == null )
      {
        RegistryKey zoneListKey = Registry.LocalMachine.OpenSubKey( RootKey );

        if ( zoneListKey != null )
        {
          string[] zones = zoneListKey.GetSubKeyNames();
          foreach ( string zone in zones )
          {
            RegistryKey tempZoneKey = zoneListKey.OpenSubKey(zone);
            if (tempZoneKey != null)
            {
              string stdName = tempZoneKey.GetValue("Std") as string;

              if (stdName.ToUpper() == standardName.ToUpper())
              {
                zoneKey = tempZoneKey;
                break;
              }
            }
          }
        }
      }
      
      return zoneKey;
    }
  }


  [DataContract]
  public class NamedTimeZone : TimeZone
  {
    public NamedTimeZone( string standardName )
    {
      Configure( standardName );
    }

    private void Configure( string standardName )
    {
      if ( standardName == string.Empty )
      {
        throw new ArgumentOutOfRangeException( "standardName", standardName, "Time zone standard name string is empty");
      }

      RegistryKey zoneKey = TimeZonesRegistry.GetTimeZoneKey( standardName );

      if (zoneKey == null)
      {
        throw new ArgumentOutOfRangeException( "standardName", standardName, "The specified time zone standard name is unknown by the OS");
      }
      else
      {
        SetTziFromRegistry( zoneKey );
      }
      zoneKey.Close();
    }

    #region TimeZone overrides
    public override string DaylightName
    {
      get
      {
        if (tzi == null)
          throw new InvalidOperationException( "Cannot access DaylightName because time zone object failed to construct." );

        if (tzi.daylightDate.month == 0)
          return "";
        else
          return daylightName;
      }
    }

    public override string StandardName 
    {
      get
      {
        if (tzi == null)
          throw new InvalidOperationException( "Cannot access StandardName because time zone object failed to construct." );

        return standardName;
      }
    }

    public override DaylightTime GetDaylightChanges(int year)
    {
      if(tzi == null)
        throw new InvalidOperationException("Cannot access GetDaylightChanges because time zone object failed to construct.");

      if(year < 1 || year > 9999)
        throw new ArgumentOutOfRangeException("year",year,"year should be between the year 0001 and 9999");

      if(tzi.daylightDate.month == 0 || tzi.standardDate.month == 0)
        return null;

      DateTime daylightDate = TziSystemTimeToDateTime(year,tzi.daylightDate);
      DateTime standardDate = TziSystemTimeToDateTime(year,tzi.standardDate);
      TimeSpan bias = TimeSpan.FromMinutes(-tzi.daylightBias);

      return new DaylightTime(daylightDate,standardDate,bias);
    }

    public override TimeSpan GetUtcOffset(DateTime localTime)
    {
      if(tzi == null)
        throw new InvalidOperationException("Cannot access GetUtcOffset because time zone object failed to construct.");

      TimeSpan offset;

      if(IsDaylightSavingTime(localTime))
      {
        offset = TimeSpan.FromMinutes(-(tzi.bias + tzi.daylightBias));
      }
      else
      {
        offset = TimeSpan.FromMinutes(-(tzi.bias + tzi.standardBias));
      }         

      return offset;
    }

    public override bool IsDaylightSavingTime(DateTime localTime)
    {
      if(tzi == null)
        throw new InvalidOperationException("Cannot access IsDaylightSavingTime because time zone object failed to construct.");

      bool isDaylightSaving = false;
      DaylightTime dt = GetDaylightChanges(localTime.Year);

      if(dt != null)
      {
        if(dt.Start.Ticks < dt.End.Ticks) // Northern Hemisphere
        {
          isDaylightSaving = (localTime.Ticks >= dt.Start.Ticks) && 
            (localTime.Ticks < dt.End.Ticks);
        }
        else
        {
          isDaylightSaving = !((localTime.Ticks >= dt.End.Ticks) && 
            (localTime.Ticks < dt.Start.Ticks));
        }
      }

      return isDaylightSaving;
    }

    public override DateTime ToLocalTime(DateTime utcTime)
    {
      if ( utcTime == DateTime.MinValue || utcTime == DateTime.MaxValue || utcTime.Kind == DateTimeKind.Local)
        return utcTime;

      if(tzi == null)
        throw new InvalidOperationException("Cannot access ToLocalTime because time zone object failed to construct.");

      if (utcTime.Ticks == 0)
        return new DateTime(0, DateTimeKind.Local);

      DateTime localTime = new DateTime(utcTime.Ticks, DateTimeKind.Local);
      localTime = localTime.Add(TimeSpan.FromMinutes(-tzi.bias));

      if(IsDaylightSavingTime(localTime))
      {
        localTime = localTime.Add(TimeSpan.FromMinutes(-tzi.daylightBias));
      }

      return localTime;
    }

    public override DateTime ToUniversalTime(DateTime localTime)
    {
      if ( localTime == DateTime.MinValue || localTime == DateTime.MaxValue || localTime.Kind == DateTimeKind.Utc)
        return localTime;

      if(tzi == null)
        throw new InvalidOperationException("Cannot access ToUniversalTime because time zone object failed to construct.");

      DateTime utcTime = new DateTime( localTime.Ticks, DateTimeKind.Utc );
      TimeSpan utcOffset = GetUtcOffset( localTime );
      utcTime = utcTime.Subtract( utcOffset );

      return utcTime;
    }

    #endregion

    public string DisplayName
    {
      get { return displayName; }
    }

    public int BiasMinutes
    {
      get
      {
        return tzi.bias;
      }
    }

    public int DaylightBiasMinutes
    {
      get
      {
        return tzi.daylightBias;
      }
    }

    public int StandardBiasMinutes
    {
      get
      {
        return tzi.standardBias;
      }
    }

    public SystemTime StandardDate
    {
      get
      {
        return tzi.standardDate;
      }
    }

    public SystemTime DaylightDate
    {
      get
      {
        return tzi.daylightDate;
      }
    }

    public DateTime GetNextDaylightChange( DateTime localTime )
    {
        return this.GetNextOrPreviousDaylightChange( localTime, true );
    }

    private DateTime GetNextOrPreviousDaylightChange( DateTime localTime, bool Forward )
    {
        if (tzi == null)
            throw new InvalidOperationException( "Cannot access GetNextDaylightChange() because time zone object failed to construct." );

        DaylightTime dt = GetDaylightChanges( localTime.Year );
        DateTime NextDaylightChange = localTime;
        DateTime LastDaylightChange = localTime;

        if (dt == null)
        {
            // So - this timezone did not have daylight savings...
            throw new System.Exception("This timezone does not support daylight savings.");
        }
        else
        {
            bool NextYear = false;
            bool LastYear = false;
            if (dt.Start.Ticks < dt.End.Ticks) // Northern Hemisphere
            {
                if (localTime.Ticks < dt.Start.Ticks)
                {
                    NextDaylightChange = dt.Start;
                    LastYear = true;
                }
                else if (localTime.Ticks < dt.End.Ticks)
                {
                    NextDaylightChange = dt.End;
                    LastDaylightChange = dt.Start;
                }
                else
                {
                    LastDaylightChange = dt.End;
                    NextYear = true;
                }
            }
            else // Southern Hemisphere
            {
                if (localTime.Ticks < dt.End.Ticks)
                {
                    NextDaylightChange = dt.End;
                    LastYear = true;
                }
                else if (localTime.Ticks < dt.Start.Ticks)
                {
                    NextDaylightChange = dt.Start;
                    LastDaylightChange = dt.End;
                }
                else
                {
                    LastDaylightChange = dt.Start;
                    NextYear = true;
                }
            }
            if (NextYear && Forward)
            {
                dt = GetDaylightChanges( localTime.Year + 1 );
                if (dt.Start.Ticks < dt.End.Ticks)
                    NextDaylightChange = dt.Start;
                else
                    NextDaylightChange = dt.End;
            }
            else if (LastYear && !Forward)
            {
                dt = GetDaylightChanges( localTime.Year - 1 );
                if (dt.Start.Ticks < dt.End.Ticks)
                    LastDaylightChange = dt.End;
                else
                    LastDaylightChange = dt.Start;
            }
        }
        return Forward ? NextDaylightChange : LastDaylightChange;
    }
    
    /// <summary>
    /// Creates an array holding the DateTimes of the next 'ChangesToGet' Daylight Saving changes.
    /// Changes to get must be between 1 and 10 inclusive.
    /// </summary>
    /// <param name="localTime"></param>
    /// <param name="ChangesToGet"></param>
    /// <returns></returns>
    public DateTime[] GetNextDaylightChanges(DateTime localTime, int ChangesToGet)
    {
        return this.GetNextOrPreviousDaylightChanges( localTime, ChangesToGet, true );
    }

    /// <summary>
    /// Creates an array holding the DateTimes of the previous 'ChangesToGet' Daylight Saving changes.
    /// Changes to get must be between 1 and 10 inclusive. Order is most recent first.
    /// </summary>
    /// <param name="localTime"></param>
    /// <param name="ChangesToGet"></param>
    /// <returns></returns>
    public DateTime[] GetPreviousDaylightChanges(DateTime localTime, int ChangesToGet)
    {
        return this.GetNextOrPreviousDaylightChanges( localTime, ChangesToGet, false );
    }

    private DateTime[] GetNextOrPreviousDaylightChanges(DateTime localTime, int ChangesToGet, bool Forward)
    {
        if( (ChangesToGet < 1) || (ChangesToGet > 10) )
        {
            throw new ArgumentOutOfRangeException("ChangesToGet",ChangesToGet,"ChangesToGet should be between the 1 and 10.");
        }

        DateTime[] returnVal = new DateTime[ChangesToGet];

        DateTime myTime = localTime;
        for ( int i = 0; i < ChangesToGet; ++i )
        {
            returnVal[i] = this.GetNextOrPreviousDaylightChange(localTime, Forward);
            localTime = returnVal[i] + System.TimeSpan.FromHours( Forward ? 1.0 : -1.0 );
        }
        return returnVal;
    }

    public string ToLocalTimeString(string utcTime, string format)
    {
      DateTime utc = DateTime.Parse(utcTime);
      DateTime local = ToLocalTime(utc);

      return local.ToString(format);
    }

    public string ToLocalTimeString(DateTime? utcTime, string format)
    {
      if (!utcTime.HasValue)
        return null;

      DateTime local = ToLocalTime(utcTime.Value);

      return local.ToString(format);
    }
    
    public int GetUtcOffsetMinutes(DateTime localTime)
    {
      TimeSpan offset = GetUtcOffset(localTime);
      return (int)offset.TotalMinutes;
    }

    public bool RecognisesDaylightSaving()
    {
      if(tzi == null)
        throw new InvalidOperationException("Cannot access RecognisesDaylightSaving() because time zone object failed to construct.");

      return (tzi.daylightDate.month != 0);
    }

    #region Win32APITypes
    [StructLayout(LayoutKind.Explicit, Size=16, CharSet=CharSet.Ansi)]
      public class SystemTime 
    {
      [FieldOffset(0)]public UInt16 year = 0; 
      [FieldOffset(2)]public UInt16 month = 0;
      [FieldOffset(4)]public UInt16 dayOfWeek = 0; 
      [FieldOffset(6)]public UInt16 day = 0; 
      [FieldOffset(8)]public UInt16 hour = 0; 
      [FieldOffset(10)]public UInt16 minute = 0; 
      [FieldOffset(12)]public UInt16 second = 0; 
      [FieldOffset(14)]public UInt16 milliseconds = 0; 
    }

    [StructLayout(LayoutKind.Sequential,Pack=2)]
      private class TimeZoneInformation
    {
      public Int32 bias = 0;
      public Int32 standardBias = 0;
      public Int32 daylightBias = 0;
      public SystemTime standardDate = null;
      public SystemTime daylightDate = null;
    }
    #endregion

    #region privates
    private TimeZoneInformation tzi = null;
    private string standardName = "";
    private string daylightName = "";
    private string displayName  = "";

    private void SetTziFromRegistry(RegistryKey zoneKey)
    {
      byte[] bytes = zoneKey.GetValue("Tzi") as byte[];
      GCHandle handle = GCHandle.Alloc( bytes, GCHandleType.Pinned );

      IntPtr buffer = handle.AddrOfPinnedObject();
      tzi = (TimeZoneInformation)Marshal.PtrToStructure( buffer, typeof( TimeZoneInformation ) );
      handle.Free();

      daylightName = zoneKey.GetValue("Dlt") as string;
      standardName = zoneKey.GetValue("Std") as string;
      displayName  = zoneKey.GetValue("Display") as string;
    }

    private DateTime TziSystemTimeToDateTime(int year, SystemTime tziTime)
    {
      int dayOfMonth = 0;

      if(tziTime.year > 0)
      {
        dayOfMonth = tziTime.day;
        year = tziTime.year;
      }
      else
      {
        // Pick a reference date as the first of the month.
        DateTime refDate = new DateTime(year,tziTime.month,1,1,0,0);
        GregorianCalendar cal = new GregorianCalendar();
        int refDayOfWeek = (int)cal.GetDayOfWeek(refDate);

        // This logic 'borrowed' from Charles Petzold, from his article
        // 'ClockRack', published in PC Magazine, September 1, 2000.
        // (ask him how it works)
        dayOfMonth = refDate.Day + tziTime.dayOfWeek + 7 - refDayOfWeek;
        dayOfMonth = (dayOfMonth - 1) % 7 + 1 ;
        dayOfMonth += 7 * (tziTime.day - 1);
        if (dayOfMonth > DateTime.DaysInMonth(year,tziTime.month))
          dayOfMonth -= 7 ;
      }

      return new DateTime(
        year,
        tziTime.month,
        dayOfMonth,
        tziTime.hour,
        tziTime.minute,
        tziTime.second,
        tziTime.milliseconds);
    }
    #endregion
  }
}
