using Newtonsoft.Json;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// MDP target data returned
  /// </summary>
  public class MdpTargetData
  {
    /// <summary>
    /// If the MDP value is constant, this is the constant value of all MDP targets in the processed data.
    /// </summary>
    [JsonProperty(PropertyName = "mdpMachineTarget")]
    public double MdpMachineTarget { get; set; }
    /// <summary>
    /// Are the MDP target values applying to all processed cells varying?
    /// </summary>
    [JsonProperty(PropertyName = "targetVaries")]
    public bool TargetVaries { get; set; }
  }
}