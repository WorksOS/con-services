using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Productivity3D.Models.Designs
{
  public class AlignmentGeometriesResult : ContractExecutionResult
  {
    /// <summary>
    /// Multiple alignment master geometry data.
    /// </summary>
    [JsonProperty(PropertyName = "alignmentGeometries", Required = Required.Always)]
    public AlignmentGeometry[] AlignmentGeometries { get; set; }

    /// <summary>
    /// Constructs alignment master geometries result from supplied multiple geometry data.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="alignmentGeometries"></param>
    /// <param name="message"></param>
    public AlignmentGeometriesResult(int code, AlignmentGeometry[] alignmentGeometries, string message = DefaultMessage) : base(code, message)
    {
      AlignmentGeometries = alignmentGeometries;
    }
  }
}
