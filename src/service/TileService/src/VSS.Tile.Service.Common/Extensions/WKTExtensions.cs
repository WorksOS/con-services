using System;
using System.Collections.Generic;
using VSS.MasterData.Models.Models;

namespace VSS.Tile.Service.Common.Extensions
{
  public static class WKTExtensions
  {
    public static IEnumerable<WGSPoint> GeometryToPoints(this string geometryWKT)
    {
      const double DEGREES_TO_RADIANS = Math.PI / 180;

      List<WGSPoint> latlngs = new List<WGSPoint>();
      //Trim off the "POLYGON((" and "))" - allow for white space
      geometryWKT = geometryWKT.ToUpper().Trim().Substring("POLYGON".Length).Trim().Trim(new[] { '(', ')' });
      var points = geometryWKT.Split(',');
      foreach (var point in points)
      {
        var parts = point.Trim().Split(' ');
        var lng = double.Parse(parts[0]) * DEGREES_TO_RADIANS;
        var lat = double.Parse(parts[1]) * DEGREES_TO_RADIANS;
        //Latitude Must be in range -pi/2 to pi/2 and longitude in the range -pi to pi
        if (lat < -Math.PI / 2)
        {
          lat += Math.PI;
        }
        else if (lat > Math.PI / 2)
        {
          lat -= Math.PI;
        }
        if (lng < -Math.PI)
        {
          lng += 2 * Math.PI;
        }
        else if (lng > Math.PI)
        {
          lng -= 2 * Math.PI;
        }
        latlngs.Add(new WGSPoint(lat, lng));
      }
      return latlngs;
    }
  }
}
