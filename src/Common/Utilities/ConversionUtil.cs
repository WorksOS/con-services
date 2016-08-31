using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using LandfillService.Common;
using LandfillService.Common.Models;
using log4net;

namespace Common.Utilities
{


  public class ConversionUtil
  {

    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public static IEnumerable<WGSPoint> GeometryToPoints(string geometry, bool convertToRadians)
    {
      const double DEGREES_TO_RADIANS = Math.PI / 180;
      Log.DebugFormat("Geometry is: {0}", geometry);
      List<WGSPoint> latlngs = new List<WGSPoint>();
      //Trim off the "POLYGON((" and "))"
      var indexOfFirstBracket = geometry.LastIndexOf('(');
      var indexOfLastBracket = geometry.IndexOf(')');
      geometry = geometry.Substring(indexOfFirstBracket+1, indexOfLastBracket - indexOfFirstBracket - 1);
      var points = geometry.Trim().Split(',');
      foreach (var point in points)
      {
        Log.DebugFormat("Converting point is: {0}", point);
        var parts = point.Trim().Split(' ');
        Log.DebugFormat("Parsing point is: {0}", parts[1]);
        var lat = double.Parse(parts[1],NumberFormatInfo.InvariantInfo);
        Log.DebugFormat("Parsing point is: {0}", parts[0]);
        var lng = double.Parse(parts[0],NumberFormatInfo.InvariantInfo);
        latlngs.Add(new WGSPoint { Lat = convertToRadians ? lat * DEGREES_TO_RADIANS : lat, Lon = convertToRadians ? lng * DEGREES_TO_RADIANS : lng });
      }
      return latlngs;
    }
  }
}
