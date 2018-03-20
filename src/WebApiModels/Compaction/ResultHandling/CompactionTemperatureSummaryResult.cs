using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

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
    /// Default private constructor.
    /// </summary>
    private CompactionTemperatureSummaryResult()
    { }

    public static CompactionTemperatureSummaryResult CreateEmptyResult() => new CompactionTemperatureSummaryResult();

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static CompactionTemperatureSummaryResult CreateTemperatureSummaryResult(TemperatureSummaryResult result)
    {
      if (result == null || !result.HasData())
      {
        return CreateEmptyResult();
      }

      return new CompactionTemperatureSummaryResult
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
        }
      };
    }
  }
}