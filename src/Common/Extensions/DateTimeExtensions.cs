using NodaTime;
using System;
using System.Globalization;
using VSS.MasterData.Models.Internal;

namespace VSS.Productivity3D.Common.Extensions
{
  public static class DateTimeExtensions
  {
    /// <summary>
    /// Determines if the string contains an ISO8601 date time
    /// </summary>
    /// <param name="inputStringUtc">The string to check</param>
    /// <param name="format">The format to use when checking</param>
    /// <returns>The date time from the string if ISO8601 else DateTime.MinDate</returns>
    public static DateTime IsDateTimeISO8601(this string inputStringUtc, string format)
    {
      DateTime utcDate = DateTime.MinValue;
      if (!string.IsNullOrWhiteSpace(inputStringUtc))
      {
        if (!DateTime.TryParseExact(inputStringUtc, format, new CultureInfo("en-US"), DateTimeStyles.AdjustToUniversal,
          out utcDate))
        {
          utcDate = DateTime.MinValue;
        }
      }
      return utcDate;
    }

    /// <summary>
    /// Construct the Iso8601 formatted date time for a UTC date time.
    /// </summary>
    /// <param name="dateTimeUtc">The date time in UTC</param>
    /// <returns>Iso8601 formatted string</returns>
    public static string ToIso8601DateTimeString(this DateTime dateTimeUtc)
    {
      // CAUTION - this assumes the DateTime passed in is already UTC!!
      return $"{dateTimeUtc:yyyy-MM-ddTHH:mm:ssZ}";
    }

  }
}