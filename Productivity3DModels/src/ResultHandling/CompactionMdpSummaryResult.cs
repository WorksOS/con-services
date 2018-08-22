using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
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
    /// Default private constructor.
    /// </summary>
    private CompactionMdpSummaryResult()
    { }

    public static CompactionMdpSummaryResult CreateEmptyResult() => new CompactionMdpSummaryResult();

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static CompactionMdpSummaryResult CreateMdpSummaryResult(MDPSummaryResult result, MDPSettings settings)
    {
      if (result == null || !result.HasData())
      {
        return CreateEmptyResult();
      }

      return new CompactionMdpSummaryResult
      {
        SummaryData = new MdpSummaryData
        {
          PercentEqualsTarget = result.CompactedPercent,
          PercentGreaterThanTarget = result.OverCompactedPercent,
          PercentLessThanTarget = result.UnderCompactedPercent,
          TotalAreaCoveredSqMeters = result.TotalAreaCoveredSqMeters,
          MdpTarget = new MdpTargetData
          {
            MdpMachineTarget = result.ConstantTargetMDP / 10,
            TargetVaries = !result.IsTargetMDPConstant
          },
          MinMDPPercent = settings.minMDPPercent,
          MaxMDPPercent = settings.maxMDPPercent
        }
      };
    }
  }
}