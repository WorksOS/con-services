using System;

namespace TagFiles.Utils
{
  public static class TagUtils
  {

    public static string MakeTagfileName(string deviceID, string machineName)
    {
      if (deviceID == "")
        deviceID = "unknown";
      if (machineName == "")
        machineName = "Megalodon";
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
      switch (upperValue)
      {
        case "HEX": return 25;
        case "WCN": return 50; // todo assign real id
        case "CSD": return 51; // todo
        default:
          {
            MegalodonLogger.LogError($"Unknown machine type supplied. value:{value}");
            return 0;
          }
      }
    }

  }
}
