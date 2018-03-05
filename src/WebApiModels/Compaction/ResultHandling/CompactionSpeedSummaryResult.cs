using Newtonsoft.Json;
using System;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
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

    /// <summary>
    /// SpeedSummaryResult create instance
    /// </summary>
    /// <param name="result">The speed results from Raptor</param>
    /// <param name="speedTarget">The speed target from Raptor</param>
    /// <returns>An instance of CompactionSpeedSummaryResult</returns>
    public static CompactionSpeedSummaryResult CreateSpeedSummaryResult(SummarySpeedResult result, MachineSpeedTarget speedTarget)
    {
      const int noSpeedData = 0;

      if (Math.Abs(result.CoverageArea - noSpeedData) < 0.001)
      {
        return new CompactionSpeedSummaryResult { SummaryData = new SpeedSummaryData() };
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
}