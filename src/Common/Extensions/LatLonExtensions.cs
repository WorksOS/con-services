using System;

namespace VSS.Productivity3D.Common.Extensions
{
  public static class LatLonExtensions
  {

    /// <summary>
    /// Latitude(y) must be in range -pi/2 to pi/2
    /// </summary>
    /// <returns>converted lat else throws exception</returns>
    public static double LatDegreesToRadians(this double lat)
    {
      lat = lat * Math.PI / 180.0;
      if (lat < -Math.PI / 2)
      {
        lat = lat + Math.PI;
      }
      else if (lat > Math.PI / 2)
      {
        lat = lat - Math.PI;
      }

      return lat;
    }

    /// <summary>
    /// Longitude(x) must be in the range -pi to pi
    /// </summary>
    /// <returns>converted lon else throws exception</returns>
    public static double LonDegreesToRadians(this double lon)
    {
      lon = lon * Math.PI / 180.0;
      if (lon < -Math.PI)
      {
        lon = lon + 2 * Math.PI;
      }
      else if (lon > Math.PI)
      {
        lon = lon - 2 * Math.PI;
      }
      return lon;
    }

    /// <summary>
    /// Convert latitude in radians to degrees
    /// </summary>
    /// <param name="lat"></param>
    /// <returns></returns>
    public static double LatRadiansToDegrees(this double lat)
    {
      return lat.RadiansToDegrees();
    }

    /// <summary>
    /// Convert longitude in radians to degrees
    /// </summary>
    /// <param name="lon"></param>
    /// <returns></returns>
    public static double LonRadiansToDegrees(this double lon)
    {
      return lon.RadiansToDegrees();
    }

    /// <summary>
    /// Convert a value in radians to degrees
    /// </summary>
    /// <param name="rad"></param>
    /// <returns></returns>
    private static double RadiansToDegrees(this double rad)
    {
      return rad / (Math.PI / 180.0);
    }
  }
}
