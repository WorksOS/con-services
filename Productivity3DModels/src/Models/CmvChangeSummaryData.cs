using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// CMV % change summary data returned
  /// </summary>
  public class CmvChangeSummaryData
  {
    /// <summary>
    /// The CMV % change values
    /// </summary>
    [JsonProperty(PropertyName = "percents")]
    public double[] Percents { get; set; }

    /// <summary>
    /// The total area covered by non-null cells in the request area
    /// </summary>
    [JsonProperty(PropertyName = "totalAreaCoveredSqMeters")]
    public double TotalAreaCoveredSqMeters { get; set; }
  }
}