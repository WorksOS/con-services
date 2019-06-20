using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.MapHandling;

namespace VSS.Productivity3D.Models.ResultHandling.Designs
{
  /// <summary>
  /// Design boundary in GeoJSON format.
  /// </summary>
  public class DesignBoundaryResult : ContractExecutionResult
  {
    /// <summary>
    /// Design boundary as GeoJSON string.
    /// </summary>
    public GeoJson GeoJSON { get; private set; }

    /// <summary>
    /// Override constructor with parameters.
    /// </summary>
    /// <param name="geoJSONStr"></param>
    public DesignBoundaryResult(GeoJson geoJSON)
    {
      GeoJSON = geoJSON;
    }
  }
}
