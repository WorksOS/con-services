using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.MapHandling;

namespace VSS.Productivity3D.Models.ResultHandling.Designs
{
  /// <summary>
  /// Alignment geometry result in GeoJSon format.
  /// </summary>
  public class AlignmentDesignGeometryResult: ContractExecutionResult, IMasterDataModel
  {
    /// <summary>
    /// Alignment geometry as GeoJSON string.
    /// </summary>
    public GeoJson GeoJSON { get; private set; }

    /// <summary>
    /// Override constructor with parameters.
    /// </summary>
    /// <param name="geoJSONStr"></param>
    public AlignmentDesignGeometryResult(GeoJson geoJSON)
    {
      GeoJSON = geoJSON;
    }

    public List<string> GetIdentifiers() => new List<string>();
  }
}
