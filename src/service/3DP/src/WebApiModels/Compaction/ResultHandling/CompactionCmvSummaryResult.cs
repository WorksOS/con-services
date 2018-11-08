using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using CmvSummaryData = VSS.Productivity3D.WebApi.Models.Compaction.Models.CmvSummaryData;

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
    /// Default constructor.
    /// </summary>
    public CompactionCmvSummaryResult()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="settings"></param>
    public CompactionCmvSummaryResult(CMVSummaryResult result, CMVSettings settings)
    {
      if (result != null && result.HasData())
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
          MinCMVPercent = settings.MinCMVPercent,
          MaxCMVPercent = settings.MaxCMVPercent
        };
      }
    }
  }
}
