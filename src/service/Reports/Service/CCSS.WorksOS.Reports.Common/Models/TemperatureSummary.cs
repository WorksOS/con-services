using Newtonsoft.Json;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class TemperatureSummary : SummaryDataBase
  {
    public TemperatureSummaryData TemperatureSummaryData { get; set; }
    [JsonIgnore]
    public bool IsEmpty => TemperatureSummaryData == null;
  }
  public class TemperatureSummaryData
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
    /// Temperature machine target range and whether it is constant or varies.
    /// </summary>
    [JsonProperty(PropertyName = "temperatureTarget")]
    public TemperatureTargetData TemperatureTarget { get; set; }
  }

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
