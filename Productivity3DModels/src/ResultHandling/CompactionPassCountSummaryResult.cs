using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// Represents result returned by Pass Count Summary request for compaction
  /// </summary>
  public class CompactionPassCountSummaryResult : ContractExecutionResult
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
    /// <param name="result"></param>
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
  }
}
