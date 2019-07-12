using System;

namespace VSS.Common.Abstractions.Extensions
{
  public static class DateTimeExtensions
  {
    public static string ToIso8601DateTimeString(this DateTime dateTimeUtc)
    {
      //Allow Unspecified as well as UTC as DateTime is serialized as unspecified.
      if (dateTimeUtc.Kind == DateTimeKind.Local)
      {
        throw new ArgumentException("Datetime parameter must be UTC");
      }

      return dateTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ");
    }
  }
}
