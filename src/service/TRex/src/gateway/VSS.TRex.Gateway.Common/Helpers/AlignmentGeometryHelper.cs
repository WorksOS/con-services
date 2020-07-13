using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Models.MapHandling;
using VSS.Productivity3D.Models.ResultHandling.Designs;
using VSS.Productivity3D.Productivity3D.Models.Designs;

namespace VSS.TRex.Gateway.Common.Helpers
{
  public class AlignmentGeometryHelper
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger("AlignmentGeometryHelper");

    /// <summary>
    /// Converts AlignmentGeometryResult into AlignmentDesignGeometryResult data.
    /// </summary>
    /// <param name="geometry"></param>
    /// <returns></returns>
    public static async Task<AlignmentDesignGeometryResult> ConvertGeometry(AlignmentGeometryResult geometry, string fileName)
    {
      // Create a main header...
      var geoJson = new GeoJson
      {
        Type = GeoJson.FeatureType.FEATURE_COLLECTION,
        Features = new List<Feature>()
      };

      // Create a header for centerline...
      var geo = new CenterlineGeometry();

      var feature = new Feature
      {
        Type = GeoJson.FeatureType.FEATURE,
        Geometry = geo,
        Properties = new Properties { Name = fileName }
      };

      // Process vertices...
      for (var i = 0; i < geometry.Vertices.Length; i++)
        for (var n = 0; n < geometry.Vertices[i].Length; n++)
          AddPoint(geometry.Vertices[i][n][0], geometry.Vertices[i][n][1], geo.CenterlineCoordinates);
     
      // Process arcs as series of vertices...
      for (var i = 0; i < geometry.Arcs.Length; i++)
      {
        AddPoint(geometry.Arcs[i].Lon1, geometry.Arcs[i].Lat1, geo.CenterlineCoordinates);
        AddPoint(geometry.Arcs[i].Lon2, geometry.Arcs[i].Lat2, geo.CenterlineCoordinates);
      }

      geoJson.Features.Add(feature);

      Log.LogInformation($"Alignment design geometry conversion completed with number of vertices: {geo.CenterlineCoordinates.Count}");

      return await Task.FromResult(new AlignmentDesignGeometryResult(geoJson));
    }

    private static void AddPoint(double x, double y, List<double[]> centerlinePoints)
    {
      const int DECIMALS = 6;

      var point = new[]
      {
        Math.Round(x, DECIMALS),
        Math.Round(y, DECIMALS)
      };

      centerlinePoints.Add(point);
    }
  }
}
