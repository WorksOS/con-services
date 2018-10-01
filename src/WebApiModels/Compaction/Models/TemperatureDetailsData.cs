using Newtonsoft.Json;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// Temperature details data returned.
  /// </summary>
  public class TemperatureDetailsData
  {
    /// <summary>
    /// Collection of temperature percentages where each element represents the percentage of the matching index temperature target range provided in the 
    /// temperatutre list member of the temperature details request representation.
    /// </summary>
    [JsonProperty(PropertyName = "percents")]
    public double[] Percents { get; set; }

    /// <summary>
    /// Gets the total coverage area for the production data - not the total area specified in filter
    /// </summary>
    /// <value>
    /// The total coverage area in sq meters.
    /// </value>
    [JsonProperty(PropertyName = "totalAreaCoveredSqMeters")]
    public double TotalAreaCoveredSqMeters { get; set; }

  }
}
