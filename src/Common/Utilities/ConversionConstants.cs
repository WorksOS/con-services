using System;

namespace VSS.Raptor.Service.Common.Utilities
{
  /// <summary>
  /// Utility class that defines constants used for data conversion.
  /// </summary>
  public class ConversionConstants
  {
    /// <summary>
    /// Value to convert from decimal degrees to radians.
    /// </summary>
    public const double DEGREES_TO_RADIANS = Math.PI / 180;

    /// <summary>
    /// Null date value returned by Raptor.
    /// </summary>
    public static readonly DateTime PDS_MIN_DATE = new DateTime(1899, 12, 30, 0, 0, 0);

  }
}
