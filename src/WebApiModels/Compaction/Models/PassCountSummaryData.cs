using Newtonsoft.Json;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// Pass Count summary data returned
  /// </summary>
  public class PassCountSummaryData
  {
    /// <summary>
    /// The percentage of pass counts that match the target pass count specified in the passCountTarget member of the request
    /// </summary>
    [JsonProperty(PropertyName = "percentEqualsTarget")]
    public double PercentEqualsTarget { get; set; }
    /// <summary>
    /// The percentage of pass counts that are greater than the target pass count specified in the passCountTarget member of the request
    /// </summary>
    [JsonProperty(PropertyName = "percentGreaterThanTarget")]
    public double PercentGreaterThanTarget { get; set; }
    /// <summary>
    /// The percentage of pass counts that are less than the target pass count specified in the passCountTarget member of the request
    /// </summary>
    [JsonProperty(PropertyName = "percentLessThanTarget")]
    public double PercentLessThanTarget { get; set; }
    /// <summary>
    /// The total area covered by non-null cells in the request area
    /// </summary>
    [JsonProperty(PropertyName = "totalAreaCoveredSqMeters")]
    public double TotalAreaCoveredSqMeters { get; set; }
    /// <summary>
    /// Pass count machine target and whether it is constant or varies.
    /// </summary>
    [JsonProperty(PropertyName = "passCountTarget")]
    public PassCountTargetData PassCountTarget { get; set; }
  }
}