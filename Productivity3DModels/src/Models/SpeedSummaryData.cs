using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Speed summary data returned
  /// </summary>
  public class SpeedSummaryData
  {
    /// <summary>
    /// The percentage of cells within speed target
    /// </summary>
    [JsonProperty(PropertyName = "percentEqualsTarget")]
    public double PercentEqualsTarget { get; set; }

    /// <summary>
    /// The percentage of the cells over speed target
    /// </summary>
    [JsonProperty(PropertyName = "percentGreaterThanTarget")]
    public double PercentGreaterThanTarget { get; set; }

    /// <summary>
    /// The percentage of the cells under speed target
    /// </summary>
    [JsonProperty(PropertyName = "percentLessThanTarget")]
    public double PercentLessThanTarget { get; set; }

    /// <summary>
    /// The total area covered by non-null cells in the request area
    /// </summary>
    [JsonProperty(PropertyName = "totalAreaCoveredSqMeters")]
    public double TotalAreaCoveredSqMeters { get; set; }

    /// <summary>
    /// Sets the minimum target machine speed. The value should be specified in km\h
    /// </summary>
    /// <value>
    /// The minimum target machine speed.
    /// </value>
    [JsonProperty(PropertyName = "minTarget")]
    public double MinTargetMachineSpeed { get; set; }

    /// <summary>
    /// Sets the maximum target machine speed. The value should be specified in km\h
    /// </summary>
    /// <value>
    /// The maximum target machine speed.
    /// </value>
    [JsonProperty(PropertyName = "maxTarget")]
    public double MaxTargetMachineSpeed { get; set; }
  }
}