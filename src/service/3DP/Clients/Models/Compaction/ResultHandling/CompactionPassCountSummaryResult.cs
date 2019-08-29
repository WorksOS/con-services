using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents result returned by Pass Count Summary request for compaction
  /// </summary>
  public class CompactionPassCountSummaryResult : ContractExecutionResult, IMasterDataModel
  {
    /// <summary>
    /// The Pass Count summary data results
    /// </summary>
    [JsonProperty(PropertyName = "passCountSummaryData")]
    public PassCountSummaryData SummaryData { get; private set; }

    /// <summary>
    /// Defaullt constructor
    /// </summary>
    public CompactionPassCountSummaryResult()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public CompactionPassCountSummaryResult(PassCountSummaryResult result)
    {
      if (result != null && result.HasData())
      {
        SummaryData = new PassCountSummaryData
        {
          PercentEqualsTarget = result.PercentEqualsTarget,
          PercentGreaterThanTarget = result.PercentGreaterThanTarget,
          PercentLessThanTarget = result.PercentLessThanTarget,
          TotalAreaCoveredSqMeters = result.TotalAreaCoveredSqMeters,
          PassCountTarget = new PassCountTargetData
          {
            MinPassCountMachineTarget = result.ConstantTargetPassCountRange.Min,
            MaxPassCountMachineTarget = result.ConstantTargetPassCountRange.Max,
            TargetVaries = !result.IsTargetPassCountConstant
          }
        };
      }
    }

    public List<string> GetIdentifiers() => new List<string>();
  }
}
