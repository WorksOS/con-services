using Newtonsoft.Json;
using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Productivity3D.Models.Designs
{
  public class AlignmentGeometryResult : ContractExecutionResult, IMasterDataModel
  {
    /// <summary>
    /// Alignment master geometry data.
    /// </summary>
    [JsonProperty(PropertyName = "alignmentGeometry", Required = Required.Always)]
    public AlignmentGeometry AlignmentGeometry { get; set; }

    /// <summary>
    /// Constructs an alignment master geometry result from supplied geometry data.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="alignmentGeometry"></param>
    /// <param name="message"></param>
    public AlignmentGeometryResult(int code, AlignmentGeometry alignmentGeometry, string message = DefaultMessage) : base(code, message)
    {
      AlignmentGeometry = alignmentGeometry;
    }

    /// <summary>
    /// No caching currently required
    /// </summary>
    /// <returns></returns>
    public List<string> GetIdentifiers() => new List<string>() { AlignmentGeometry.DesignUid.ToString() };
  }
}
