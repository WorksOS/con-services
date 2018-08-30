using System;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// Represents result returned by Speed Summary request for compaction
  /// </summary>
  public class CompactionSpeedSummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// The Speed summary data results
    /// </summary>
    [JsonProperty(PropertyName = "speedSummaryData")]
    public SpeedSummaryData SummaryData { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CompactionSpeedSummaryResult()
    { }

    public static CompactionSpeedSummaryResult CreateEmptyResult() => new CompactionSpeedSummaryResult();

    /// <summary>
    /// SpeedSummaryResult create instance
    /// </summary>
    /// <param name="result">The speed results from Raptor</param>
    /// <param name="speedTarget">The speed target from Raptor</param>
    /// <returns>An instance of CompactionSpeedSummaryResult</returns>
    public static CompactionSpeedSummaryResult CreateSpeedSummaryResult(SpeedSummaryResult result, MachineSpeedTarget speedTarget)
    {
      if (result == null || !result.HasData())
      {
        return CreateEmptyResult();
      }

      return new CompactionSpeedSummaryResult
      {
        SummaryData = new SpeedSummaryData
        {
          PercentEqualsTarget = result.MatchTarget,
          PercentGreaterThanTarget = result.AboveTarget,
          PercentLessThanTarget = result.BelowTarget,
          TotalAreaCoveredSqMeters = result.CoverageArea,
          MinTargetMachineSpeed =
            Math.Round(speedTarget.MinTargetMachineSpeed * 0.036, 1,
              MidpointRounding.AwayFromZero), // cm per second converted to km per hour...
          MaxTargetMachineSpeed =
            Math.Round(speedTarget.MaxTargetMachineSpeed * 0.036, 1,
              MidpointRounding.AwayFromZero) // cm per second converted to km per hour...
        }
      };
    }
  }
}
