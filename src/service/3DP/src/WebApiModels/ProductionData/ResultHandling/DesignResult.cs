using System.Collections.Generic;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.MapHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  public class DesignResult : ContractExecutionResult
  {
    /// <summary>
    /// Array of design boundaries in GeoJson format.
    /// </summary>
    /// 
    public List<GeoJson> DesignBoundaries { get; private set; }

    /// <summary>
    /// Creates an instance of the DesignResult class.
    /// </summary>
    public DesignResult (List<GeoJson> designBoundaries)
    {
      DesignBoundaries = designBoundaries;
    }
  }
}
