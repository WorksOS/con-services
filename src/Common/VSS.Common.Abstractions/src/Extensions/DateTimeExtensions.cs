using System;

namespace VSS.Common.Abstractions.Extensions
{
  public static class DateTimeExtensions
  {
    /**
     * If a Datetime was originally constructed with an unspecified kind we assume that UTC was intended
     * and change the Kind to that to avoid date conversions based on current system locale
     */
    public static string ToIso8601DateTimeString(this DateTime dateTimeUtc)
    {
      dateTimeUtc = dateTimeUtc.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dateTimeUtc, DateTimeKind.Utc) : dateTimeUtc;
      return dateTimeUtc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
    }
  }
}
