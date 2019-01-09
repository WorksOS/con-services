using System.Collections.Generic;
using System.Linq;
using ASNodeDecls;
using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApi.Models.MapHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents the response to a request to get boundaries from a DXF linework file.
  /// </summary>
  public class DxfLineworkFileResult : ContractExecutionResult
  {
    public TWGS84LineworkBoundary[] LineworkBoundaries { get; }

    public DxfLineworkFileResult(TASNodeErrorStatus code, string message, TWGS84LineworkBoundary[] lineworkBoundaries)
    {
      LineworkBoundaries = lineworkBoundaries;
      Message = message;
      Code = (int)code;
    }

    public GeoJson ConvertToGeoJson()
    {
      if (LineworkBoundaries == null) return null;

      var geoJson = new GeoJson
      {
        Type = GeoJson.FeatureType.FEATURE_COLLECTION,
        Features = new List<Feature>()
      };

      foreach (var boundary in LineworkBoundaries)
      {
        geoJson.Features.Add(new Feature
        {
          Type = GeoJson.FeatureType.FEATURE,
          Properties = new Properties { Name = boundary.BoundaryName },
          Geometry = new Geometry
          {
            Type = Geometry.Types.MULTI_LINE_STRING,
            Coordinates = GetCoordinatesFromFencePoints(boundary)
          }
        });
      }

      return geoJson;
    }

    private static List<List<double[]>> GetCoordinatesFromFencePoints(TWGS84LineworkBoundary boundary)
    {
      var result = new List<List<double[]>>();
      var boundaries = boundary.Boundary.FencePoints.Select(point => new[] {point.Lon, point.Lat}).ToList(); // GeoJSON is lon/lat.

      result.Add(boundaries);

      return result;
    }
  }
}
