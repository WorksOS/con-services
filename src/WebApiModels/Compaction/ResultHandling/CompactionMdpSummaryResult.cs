using System;
using Newtonsoft.Json;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents result returned by MDP Summary request for compaction
  /// </summary>
  public class CompactionMdpSummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// The MDP summary data results
    /// </summary>
    [JsonProperty(PropertyName = "mdpSummaryData")]
    public MdpSummaryData SummaryData { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CompactionMdpSummaryResult()
    { }

    /// <summary>
    /// MdpSummaryResult create instance
    /// </summary>
    /// <param name="result">The MDP results from Raptor</param>
    /// <param name="settings">The MDP settings used by Raptor</param>
    /// <returns>An instance of CompactionMdpSummaryResult</returns>
    public static CompactionMdpSummaryResult CreateMdpSummaryResult(MDPSummaryResult result, MDPSettings settings)
    {
      const int noMdpData = 0;

      if (Math.Abs(result.totalAreaCoveredSqMeters - noMdpData) < 0.001)
      {
        return new CompactionMdpSummaryResult { SummaryData = new MdpSummaryData { MdpTarget = new MdpTargetData() } };
      }

      return new CompactionMdpSummaryResult
      {
        SummaryData = new MdpSummaryData
        {
          PercentEqualsTarget = result.compactedPercent,
          PercentGreaterThanTarget = result.overCompactedPercent,
          PercentLessThanTarget = result.underCompactedPercent,
          TotalAreaCoveredSqMeters = result.totalAreaCoveredSqMeters,
          MdpTarget = new MdpTargetData
          {
            MdpMachineTarget = result.constantTargetMDP / 10,
            TargetVaries = !result.isTargetMDPConstant
          },
          MinMDPPercent = settings.minMDPPercent,
          MaxMDPPercent = settings.maxMDPPercent
        }
      };
    }

    /// <summary>
    /// MDP summary data returned
    /// </summary>
    public class MdpSummaryData
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
      /// MDP machine target and whether it is constant or varies.
      /// </summary>
      [JsonProperty(PropertyName = "mdpTarget")]
      public MdpTargetData MdpTarget { get; set; }
      /// <summary>
      /// The minimum percentage the measured MDP may be compared to the mdpTarget from the machine
      /// </summary>
      [JsonProperty(PropertyName = "minMDPPercent", Required = Required.Default)]
      public double MinMDPPercent { get; set; }
      /// <summary>
      /// The maximum percentage the measured MDP may be compared to the mdpTarget from the machine
      /// </summary>
      [JsonProperty(PropertyName = "maxMDPPercent")]
      public double MaxMDPPercent { get; set; }

    }

    /// <summary>
    /// MDP target data returned
    /// </summary>
    public class MdpTargetData
    {
      /// <summary>
      /// If the MDP value is constant, this is the constant value of all MDP targets in the processed data.
      /// </summary>
      [JsonProperty(PropertyName = "mdpMachineTarget")]
      public double MdpMachineTarget { get; set; }
      /// <summary>
      /// Are the MDP target values applying to all processed cells varying?
      /// </summary>
      [JsonProperty(PropertyName = "targetVaries")]
      public bool TargetVaries { get; set; }
    }
  }
}
