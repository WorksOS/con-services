using Newtonsoft.Json.Linq;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  public class DesignResult : ContractExecutionResult
  {
    /// <summary>
    /// Array of design boundaries in GeoJson format.
    /// </summary>
    /// 
    public JObject[] designBoundaries { get; private set; }

    /// <summary>
    /// Private constructor.
    /// </summary>
    /// 
    private DesignResult()
    {
      // ...
    }

    /// <summary>
    /// Creates an instance of the DesignResult class.
    /// </summary>
    /// <param name="designBoundaries">Array of design boundaries in GeoJson format.</param>
    /// <returns>A created instance of the SurveyedSurfaceResult class.</returns>
    /// 
    public static DesignResult CreateDesignResult(JObject[] designBoundaries)
    {
      return new DesignResult { designBoundaries = designBoundaries };
    }
  }
}
