using Newtonsoft.Json;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// Temperature target data returned
  /// </summary>
  public class TemperatureTargetData
  {
    /// <summary>
    /// If the Temperature range is constant, this is the minimum constant value of all temperature target ranges in the processed data.
    /// </summary>
    [JsonProperty(PropertyName = "minTemperatureMachineTarget")]
    public double MinTemperatureMachineTarget { get; set; }
    /// <summary>
    /// If the Temperature range is constant, this is the maximum constant value of all temperature target ranges in the processed data.
    /// </summary>
    [JsonProperty(PropertyName = "maxTemperatureMachineTarget")]
    public double MaxTemperatureMachineTarget { get; set; }
    /// <summary>
    /// Are the temperature target ranges applying to all processed cells varying?
    /// </summary>
    [JsonProperty(PropertyName = "targetVaries")]
    public bool TargetVaries { get; set; }
  }
}