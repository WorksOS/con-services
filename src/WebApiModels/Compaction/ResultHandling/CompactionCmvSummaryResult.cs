using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents result returned by CMV Summary request for compaction
  /// </summary>
  public class CompactionCmvSummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// The CMV summary data results
    /// </summary>
    [JsonProperty(PropertyName = "cmvSummaryData")]
    public CmvSummaryData SummaryData { get; private set; }

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private CompactionCmvSummaryResult()
    { }
    
    public static CompactionCmvSummaryResult CreateEmptyResult() => new CompactionCmvSummaryResult();

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static CompactionCmvSummaryResult Create(CMVSummaryResult result, CMVSettings settings)
    {
      if (result == null || !result.HasData())
      {
        return CreateEmptyResult();
      }

      return new CompactionCmvSummaryResult
      {
        SummaryData = new CmvSummaryData
        {
          PercentEqualsTarget = result.CompactedPercent,
          PercentGreaterThanTarget = result.OverCompactedPercent,
          PercentLessThanTarget = result.UnderCompactedPercent,
          TotalAreaCoveredSqMeters = result.TotalAreaCoveredSqMeters,
          CmvTarget = new CmvTargetData
          {
            CmvMachineTarget = result.ConstantTargetCmv / 10,
            TargetVaries = !result.IsTargetCmvConstant
          },
          MinCMVPercent = settings.minCMVPercent,
          MaxCMVPercent = settings.maxCMVPercent
        }
      };
    }
  }
}