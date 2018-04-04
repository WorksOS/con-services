using Newtonsoft.Json;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// MDP summary data returned
  /// </summary>
  public class MdpSummaryData
  {
    /// <summary>
    /// The percentage of cells that are compacted within the target bounds
    /// </summary>
    [JsonProperty(PropertyName = "percentEqualsTarget")]
    public double PercentEqualsTarget { get; set; }
    /// <summary>
    /// The percentage of the cells that are over-compacted
    /// </summary>
    [JsonProperty(PropertyName = "percentGreaterThanTarget")]
    public double PercentGreaterThanTarget { get; set; }
    /// <summary>
    /// The percentage of the cells that are under compacted
    /// </summary>
    [JsonProperty(PropertyName = "percentLessThanTarget")]
    public double PercentLessThanTarget { get; set; }
    /// <summary>
    /// The total area covered by non-null cells in the request area
    /// </summary>
    [JsonProperty(PropertyName = "totalAreaCoveredSqMeters")]
    public double TotalAreaCoveredSqMeters { get; set; }
    /// <summary>
    /// MDP machine target and whether it is constant or varies.
    /// </summary>
    [JsonProperty(PropertyName = "mdpTarget")]
    public MdpTargetData MdpTarget { get; set; }
    /// <summary>
    /// The minimum percentage the measured MDP may be compared to the mdpTarget from the machine
    /// </summary>
    [JsonProperty(PropertyName = "minMDPPercent", Required = Required.Default)]
    public double MinMDPPercent { get; set; }
    /// <summary>
    /// The maximum percentage the measured MDP may be compared to the mdpTarget from the machine
    /// </summary>
    [JsonProperty(PropertyName = "maxMDPPercent")]
    public double MaxMDPPercent { get; set; }
  }
}