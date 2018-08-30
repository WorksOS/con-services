using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// Represents result returned by temperature Summary request for compaction
  /// </summary>
  public class CompactionTemperatureSummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// The temperature summary data results
    /// </summary>
    [JsonProperty(PropertyName = "temperatureSummaryData")]
    public TemperatureSummaryData SummaryData { get; private set; }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionTemperatureSummaryResult()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public CompactionTemperatureSummaryResult(TemperatureSummaryResult result)
    {
      if (result != null && !result.HasData())
      {
        SummaryData = new TemperatureSummaryData
        {
          PercentEqualsTarget = result.WithinTemperaturePercent,
          PercentGreaterThanTarget = result.AboveTemperaturePercent,
          PercentLessThanTarget = result.BelowTemperaturePercent,
          TotalAreaCoveredSqMeters = result.TotalAreaCoveredSqMeters,
          TemperatureTarget = new TemperatureTargetData
          {
            MinTemperatureMachineTarget = result.MinimumTemperature / 10,
            MaxTemperatureMachineTarget = result.MaximumTemperature / 10,
            TargetVaries = !result.IsTargetTemperatureConstant
          }
        };
      }
    }
  }
}