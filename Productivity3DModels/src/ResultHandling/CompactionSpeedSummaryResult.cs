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
    private const double CMSEC_TO_KMHOUR_CONVERSION_FACTOR = 0.036;
    private const int PRECISION = 1;

    /// <summary>
    /// The Speed summary data results
    /// </summary>
    [JsonProperty(PropertyName = "speedSummaryData")]
    public SpeedSummaryData SummaryData { get; private set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public CompactionSpeedSummaryResult()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="result">The speed results from Raptor</param>
    /// <param name="speedTarget">The speed target from Raptor</param>
    /// <returns>An instance of CompactionSpeedSummaryResult</returns>
    public CompactionSpeedSummaryResult(SpeedSummaryResult result, MachineSpeedTarget speedTarget)
    {
      if (result != null && result.HasData())
      {
        SummaryData = new SpeedSummaryData
        {
          PercentEqualsTarget = result.MatchTarget,
          PercentGreaterThanTarget = result.AboveTarget,
          PercentLessThanTarget = result.BelowTarget,
          TotalAreaCoveredSqMeters = result.CoverageArea,
          MinTargetMachineSpeed =
            Math.Round(speedTarget.MinTargetMachineSpeed * CMSEC_TO_KMHOUR_CONVERSION_FACTOR, PRECISION,
              MidpointRounding.AwayFromZero), // cm per second converted to km per hour...
          MaxTargetMachineSpeed =
            Math.Round(speedTarget.MaxTargetMachineSpeed * CMSEC_TO_KMHOUR_CONVERSION_FACTOR, PRECISION,
              MidpointRounding.AwayFromZero) // cm per second converted to km per hour...
        };
      }
    }
  }
}
