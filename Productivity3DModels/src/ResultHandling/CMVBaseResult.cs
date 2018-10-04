using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// The base result representation of detailed/summary CMV request
  /// </summary>
  public class CMVBaseResult : ContractExecutionResult
  {
    /// <summary>
    /// If the CMV value is constant, this is the constant value of all CMV targets in the processed data.
    /// </summary>
    [JsonProperty(PropertyName = "constantTargetCMV")]
    public short ConstantTargetCmv { get; set; }

    /// <summary>
    /// Are the CMV target values applying to all processed cells constant?
    /// </summary>
    [JsonProperty(PropertyName = "isTargetCMVConstant")]
    public bool IsTargetCmvConstant { get; set; }
  }
}
