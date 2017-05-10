using System;

namespace ProjectWebApiCommon.Utilities
{
  public static class DotNetExtensions
  {
     public static string ToIso8601DateTimeString(this DateTime dateTimeUtc)
    {
      // CAUTION - this assumes the DateTime passed in is already UTC!!
      return string.Format("{0:yyyy-MM-ddTHH:mm:ssZ}", dateTimeUtc);
    }
  }
}
