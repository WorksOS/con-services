using System;

namespace VSS.Productivity3D.Models.Utilities
{
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

    /// <summary>
    /// Value to convert from km/h to cm/s
    /// </summary>
    public static readonly double KM_HR_TO_CM_SEC = 27.77777778; //1.0 / 3600 * 100000;
  }
}
