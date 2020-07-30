using System;

namespace TagFiles.Utils
{
  public static class TagUtils
  {

    private static readonly DateTime UnixEpoch =
        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static long GetCurrentUnixTimestampMillis()
    {
      return (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
    }

    public static long GetCustomTimestampMillis(DateTime customDate)
    {
      return (long)(customDate - UnixEpoch).TotalMilliseconds;
    }

    public static DateTime DateTimeFromUnixTimestampMillis(long millis)
    {
      return UnixEpoch.AddMilliseconds(millis);
    }

    public static long GetCurrentUnixTimestampSeconds()
    {
      return (long)(DateTime.UtcNow - UnixEpoch).TotalSeconds;
    }

    public static DateTime DateTimeFromUnixTimestampSeconds(long seconds)
    {
      return UnixEpoch.AddSeconds(seconds);
    }

    public static double ToRadians(double val)
    {
      return (Math.PI / 180) * val;
    }

    public static string MakeTagfileName(string deviceID, string machineName)
    {
      if (deviceID == "")
        deviceID = "unknowndeviceid";
      if (machineName == "")
        machineName = "unknownname";
      return $"{deviceID}--{machineName}--{DateTime.UtcNow.ToString("yyMMddHHmmss")}.tag"; 
    }

    public static DateTime UnixTimeStampToUTCDateTime(double unixTimeStamp)
    {
      // Unix timestamp is seconds past epoch
      DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
      dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
      return dtDateTime;
    }

    public static byte ConvertToMachineType(string value)
    {
      var upperValue = value.ToUpper();

      /*
        Unknown = 0,
        Dozer = 23,
        Grader = 24,
        Excavator = 25,
        MotorScraper = 26,
        TowedScraper = 27,
        CarryAllScraper = 28,
        RubberTyreDozer = 29,
        WheelLoader = 30,
        WheelTractor = 31,
        SoilCompactor = 32,
        ForemansTruck = 33,
        Generic = 34,
        MillerPlaner = 36,
        BackhoeLoader = 37,
        AsphaltCompactor = 39,
        KerbandGutter = 41,
        AsphaltPaver = 42,
        FourDrumLandfillCompactor = 43,
        Trimmer = 44,
        ConcretePaver = 45,
        CutterSuctionDredge = 70,
        BargeMountedExcavator = 71        
       */


      switch (upperValue)
      {
        case "DOZ": return 23;
        case "GRD": return 24;
        case "LHX": return 25;
        case "MSC": return 26;
        case "TSC": return 27;
        case "CSC": return 28;
        case "RTD": return 29;
        case "WLD": return 30;
        case "WTR": return 31;
        case "COM": return 32;
        case "TRK": return 33;
        case "GEN": return 34;
        case "MPL": return 36;
        case "BLD": return 37;
        case "ACM": return 39;
        case "KBG": return 41;
        case "APV": return 42;
        case "4CM": return 43;
        case "TRM": return 44;
        case "CPV": return 45;
        case "CSD": return 70;
        case "HEX": return 71;

        default:
          {
            throw new System.ArgumentException($"Unknown machine type supplied. value:{value}");
          }
      }
    }

  }
}
