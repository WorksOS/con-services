using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using TemperatureSummaryData = VSS.Productivity3D.WebApi.Models.Compaction.Models.TemperatureSummaryData;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
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
    /// <param name="result"></param>
    public CompactionTemperatureSummaryResult(TemperatureSummaryResult result)
    {
      if (result != null && result.HasData())
      {
        SummaryData = new TemperatureSummaryData
        {
          PercentEqualsTarget = result.WithinTemperaturePercent,
          PercentGreaterThanTarget = result.AboveTemperaturePercent,
          PercentLessThanTarget = result.BelowTemperaturePercent,
          TotalAreaCoveredSqMeters = result.TotalAreaCoveredSqMeters,
          TemperatureTarget = new TemperatureTargetData
          {
            MinTemperatureMachineTarget = result.TargetData.MinTemperatureMachineTarget / 10,
            MaxTemperatureMachineTarget = result.TargetData.MaxTemperatureMachineTarget / 10,
            TargetVaries = result.TargetData.TargetVaries
          }
        };
      }
    }
  }
}
