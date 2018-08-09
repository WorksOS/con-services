using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// CMV summary data returned
  /// </summary>
  public class CmvSummaryData
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
    /// CMV machine target and whether it is constant or varies.
    /// </summary>
    [JsonProperty(PropertyName = "cmvTarget")]
    public CmvTargetData CmvTarget { get; set; }
    /// <summary>
    /// The minimum percentage the measured CMV may be compared to the cmvTarget from the machine
    /// </summary>
    [JsonProperty(PropertyName = "minCMVPercent", Required = Required.Default)]
    public double MinCMVPercent { get; set; }
    /// <summary>
    /// The maximum percentage the measured CMV may be compared to the cmvTarget from the machine
    /// </summary>
    [JsonProperty(PropertyName = "maxCMVPercent")]
    public double MaxCMVPercent { get; set; }
  }
}