using Newtonsoft.Json.Linq;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  public class AlignmentResult: ContractExecutionResult
  {
    /// <summary>
    /// Array of c in GeoJson format.
    /// </summary>
    /// 
    public JObject[] AlignmentGeometries { get; private set; }

    /// <summary>
    /// Private constructor.
    /// </summary>
    /// 
    private AlignmentResult()
    {
      // ...
    }

    /// <summary>
    /// Creates an instance of the AlignmentResult class.
    /// </summary>
    /// <param name="alignmentGeometries">Array of alignmentGeometriess in GeoJson format.</param>
    /// <returns>A created instance of the AlignmentResult class.</returns>
    /// 
    public AlignmentResult(JObject[] alignmentGeometries)
    {
      AlignmentGeometries = alignmentGeometries;
    }
  }
}
