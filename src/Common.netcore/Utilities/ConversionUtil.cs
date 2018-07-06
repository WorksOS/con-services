using System;
using System.Collections.Generic;
using System.Globalization;
using LandfillService.Common.Models;

namespace Common.Utilities
{
  public class ConversionUtil
  {
    public static IEnumerable<WGSPoint> GeometryToPoints(string geometry, bool convertToRadians)
    {
      const double DEGREES_TO_RADIANS = Math.PI / 180;
      var latlngs = new List<WGSPoint>();
      //Trim off the "POLYGON((" and "))"
      var indexOfFirstBracket = geometry.LastIndexOf('(');
      var indexOfLastBracket = geometry.IndexOf(')');
      geometry = geometry.Substring(indexOfFirstBracket + 1, indexOfLastBracket - indexOfFirstBracket - 1);
      var points = geometry.Trim().Split(',');
      foreach (var point in points)
      {
        var parts = point.Trim().Split(' ');
        var lat = double.Parse(parts[1], NumberFormatInfo.InvariantInfo);
        var lng = double.Parse(parts[0], NumberFormatInfo.InvariantInfo);
        latlngs.Add(new WGSPoint
        {
          Lat = convertToRadians ? lat * DEGREES_TO_RADIANS : lat,
          Lon = convertToRadians ? lng * DEGREES_TO_RADIANS : lng
        });
      }

      return latlngs;
    }
  }
}