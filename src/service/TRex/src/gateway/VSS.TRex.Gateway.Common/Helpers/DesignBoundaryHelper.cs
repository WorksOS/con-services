using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreX.Interfaces;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Models.MapHandling;
using VSS.Productivity3D.Models.ResultHandling.Designs;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using FenceGeometry = VSS.Productivity3D.Models.Models.MapHandling.Geometry;

namespace VSS.TRex.Gateway.Common.Helpers
{
  public static class DesignBoundaryHelper
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger("DesignBoundaryHelper");

    /// <summary>
    /// Converts DesignBoundaryResponse into DesignBoundaryResult data.
    /// </summary>
    public static async Task<DesignBoundaryResult> ConvertBoundary(List<Fence> boundary, double tolerance, double cellSize, string csib, string fileName)
    {
      const int VERTICES_LIMIT = 10000;

      var vertsCount = 0;

      // Create a main header...
      var geoJson = new GeoJson
      {
        Type = GeoJson.FeatureType.FEATURE_COLLECTION,
        Features = new List<Feature>()
      };

      // Could be multiple fences...
      foreach (var fence in boundary)
      {
        // Create a header for each polygon...
        var geo = new FenceGeometry
        {
          Type = FenceGeometry.Types.POLYGON,
          Coordinates = new List<List<double[]>>()
        };

        var feature = new Feature
        {
          Type = GeoJson.FeatureType.FEATURE,
          Geometry = geo,
          Properties = new Properties { Name = fileName }
        };

        // Reduce vertices if too large...
        if (fence.Points.Count > VERTICES_LIMIT || tolerance > 0)
        {
          var toler = tolerance > 0 ? tolerance : cellSize;

          do
          {
            Log.LogInformation($"{nameof(ConvertBoundary)}: Reducing fence verts. Tolerance: {toler}");
            fence.Compress(toler);
            toler = toler * 2;
          } while (fence.Points.Count >= VERTICES_LIMIT);
        }

        var arrayCount = fence.Points.Count;
        vertsCount += arrayCount; // running total...

        var neeCoords = new XYZ[arrayCount];

        // Winding must be anticlockwise (right hand rule) which is worked out be calculating area...
        if (fence.IsWindingClockwise())
        {
          Log.LogInformation($"{nameof(ConvertBoundary)}: Winding Clockwise.");
          // Reverse ordering...
          for (var i = fence.Points.Count - 1; i >= 0; i--)
            neeCoords[arrayCount - i - 1] = new XYZ(fence.Points[i].Y, fence.Points[i].X, 0.0); // Note: This is a 2D conversion, elevation is set to 0
        }
        else
        {
          Log.LogInformation($"{nameof(ConvertBoundary)}: Winding AntiClockwise.");

          for (var i = 0; i < fence.Points.Count; i++)
            neeCoords[i] = new XYZ(fence.Points[i].Y, fence.Points[i].X, 0.0);
        }

        var llhCoords = DIContext.Obtain<IConvertCoordinates>().NEEToLLH(csib, neeCoords.ToCoreX_XYZ());
        var fencePoints = new List<double[]>();

        for (var fencePointIdx = 0; fencePointIdx < llhCoords.Length; fencePointIdx++)
          AddPoint(llhCoords[fencePointIdx].X, llhCoords[fencePointIdx].Y, fencePoints);

        geo.Coordinates.Add(fencePoints);

        geoJson.Features.Add(feature);
      }

      Log.LogInformation($"Boundary conversion completed with number of vertices: {vertsCount}");

      return new DesignBoundaryResult(geoJson);
    }

    private static void AddPoint(double x, double y, List<double[]> fencePoints)
    {
      const int DECIMALS = 6;

      var point = new[]
      {
        Math.Round(x, DECIMALS),
        Math.Round(y, DECIMALS)
      };

      fencePoints.Add(point);
    }
  }
}
