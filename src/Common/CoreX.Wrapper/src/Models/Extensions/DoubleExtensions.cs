using System;

namespace CoreX.Wrapper.Extensions
{
  public static class DoubleExtensions
  {
    /// <summary>
    /// Converts a given angle to it's value in radians.
    /// </summary>
    public static double DegreesToRadians(this double degree) => Math.PI * degree / 180.0;

    /// <summary>
    /// Converts a given angle to it's value in degrees.
    /// </summary>
    public static double RadiansToDegrees(this double radian) => radian * (180.0 / Math.PI);
  }
}
