using System;

namespace VSS.Productivity3D.WebApiModels.Extensions
{
    public static class LatLonExtensions
    {

      /// <summary>
      /// Latitude(y) must be in range -pi/2 to pi/2
      /// </summary>
      /// <returns>converted lat else throws exception</returns>
      public static double latDegreesToRadians(this double lat)
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
      public static double lonDegreesToRadians(this double lon)
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
  }
}
