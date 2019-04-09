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
  /// <remarks>
  /// We are not altering the coordinates in any way; they are in the order received from Raptor/Trex. Please note, this likely means
  /// they do not conform with the 2016 IETF specification on 'right hand winding'; see section 3.1.6 of the spec; https://tools.ietf.org/html/rfc7946#section-3.1.6.
  ///
  /// If you need a linter for the 3DP response that supports the 2008 informal spec see http://geojson.io/#map=2/20.0/0.0.
  /// </remarks>
  public class DxfLineworkFileResult : ContractExecutionResult
  {
    public TWGS84LineworkBoundary[] LineworkBoundaries { get; }

    public DxfLineworkFileResult(TASNodeErrorStatus code, string message, TWGS84LineworkBoundary[] lineworkBoundaries)
    {
      LineworkBoundaries = lineworkBoundaries;
      Message = message;
      Code = (int)code;
    }

    public GeoJson ConvertToGeoJson(bool convertLineStringCoordsToPolygon, int maxPointsToApproximateTo)
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
          Geometry = GetCoordinatesFromFencePoints(boundary, convertLineStringCoordsToPolygon)
        });
      }

      return geoJson;
    }

    private static Geometry GetCoordinatesFromFencePoints(TWGS84LineworkBoundary boundary, bool convertLineStringCoordsToPolygon)
    {
      var boundaries = boundary.Boundary.FencePoints.Select(point => new[] { point.Lon, point.Lat }).ToList(); // GeoJSON is lon/lat.
      var boundaryType = Geometry.Types.POLYGON;

      if (boundaries.First()[0] != boundaries.Last()[0] && boundaries.First()[1] != boundaries.Last()[1])
      {
        if (convertLineStringCoordsToPolygon)
        {
          boundaries.Add(boundaries.First());
        }
        else
        {
          boundaryType = Geometry.Types.LINESTRING;
        }
      }

      return new Geometry
      {
        Type = boundaryType,
        Coordinates = new List<List<double[]>> { boundaries }
      };
    }
  }
}
