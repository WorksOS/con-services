using Newtonsoft.Json;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// CMV target data returned
  /// </summary>
  public class CmvTargetData
  {
    /// <summary>
    /// If the CMV value is constant, this is the constant value of all CMV targets in the processed data.
    /// </summary>
    [JsonProperty(PropertyName = "cmvMachineTarget")]
    public double CmvMachineTarget { get; set; }
    /// <summary>
    /// Are the CMV target values applying to all processed cells varying?
    /// </summary>
    [JsonProperty(PropertyName = "targetVaries")]
    public bool TargetVaries { get; set; }
  }
}