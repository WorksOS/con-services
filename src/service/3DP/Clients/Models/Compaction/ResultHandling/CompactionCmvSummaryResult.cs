using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents result returned by CMV Summary request for compaction
  /// </summary>
  public class CompactionCmvSummaryResult : ContractExecutionResult, IMasterDataModel
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

    public List<string> GetIdentifiers() => new List<string>();
  }
}
