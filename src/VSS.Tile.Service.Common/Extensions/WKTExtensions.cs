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
      //Trim off the "POLYGON((" and "))"
      geometryWKT = geometryWKT.Substring(9, geometryWKT.Length - 11);
      var points = geometryWKT.Split(',');
      foreach (var point in points)
      {
        var parts = point.Trim().Split(' ');
        var lng = double.Parse(parts[0]);
        var lat = double.Parse(parts[1]);
        latlngs.Add(WGSPoint.CreatePoint(lat * DEGREES_TO_RADIANS, lng * DEGREES_TO_RADIANS));
      }
      return latlngs;
    }
  }
}
