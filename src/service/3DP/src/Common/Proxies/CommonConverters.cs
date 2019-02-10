using System.Collections.Generic;
using VSS.MasterData.Models.Converters;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Common.Proxies
{
  public static class CommonConverters
  {
    public static IEnumerable<WGSPoint> GeometryToPoints(string geometry)
    {
      var latlngs = new List<WGSPoint>();
      //Trim off the "POLYGON((" and "))"
      geometry = geometry.Substring(9, geometry.Length - 11);
      var points = geometry.Split(',');

      foreach (var point in points)
      {
        var parts = point.Trim().Split(' ');
        var lng = double.Parse(parts[0]);
        var lat = double.Parse(parts[1]);
        latlngs.Add(new WGSPoint(lat * Coordinates.DEGREES_TO_RADIANS, lng * Coordinates.DEGREES_TO_RADIANS));
      }

      return latlngs;
    }
  }
}
