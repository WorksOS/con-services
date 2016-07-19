using System;
using System.Collections.Generic;
using LandfillService.Common.Models;

namespace Common.Utilities
{
  public class ConversionUtil
  {
    public static IEnumerable<WGSPoint> GeometryToPoints(string geometry)
    {
      const double DEGREES_TO_RADIANS = Math.PI / 180;

      List<WGSPoint> latlngs = new List<WGSPoint>();
      //Trim off the "POLYGON((" and "))"
      geometry = geometry.Substring(9, geometry.Length - 11);
      var points = geometry.Split(',');
      foreach (var point in points)
      {
        var parts = point.Split(' ');
        var lat = double.Parse(parts[1]);
        var lng = double.Parse(parts[0]);
        latlngs.Add(new WGSPoint { Lat = lat * DEGREES_TO_RADIANS, Lon = lng * DEGREES_TO_RADIANS });
      }
      return latlngs;
    }
  }
}
