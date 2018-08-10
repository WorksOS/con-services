using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
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
    /// Private constructor
    /// </summary>
    private CompactionPassCountSummaryResult()
    { }

    public static CompactionPassCountSummaryResult CreateEmptyResult() => new CompactionPassCountSummaryResult();

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static CompactionPassCountSummaryResult CreatePassCountSummaryResult(PassCountSummaryResult result)
    {
      if (result == null || !result.HasData())
      {
        return CreateEmptyResult();
      }

      return new CompactionPassCountSummaryResult
      {
        SummaryData = new PassCountSummaryData
        {
          PercentEqualsTarget = result.PercentEqualsTarget,
          PercentGreaterThanTarget = result.PercentGreaterThanTarget,
          PercentLessThanTarget = result.PercentLessThanTarget,
          TotalAreaCoveredSqMeters = result.TotalAreaCoveredSqMeters,
          PassCountTarget = new PassCountTargetData
          {
            MinPassCountMachineTarget = result.ConstantTargetPassCountRange.min,
            MaxPassCountMachineTarget = result.ConstantTargetPassCountRange.max,
            TargetVaries = !result.IsTargetPassCountConstant
          }
        }
      };
    }
  }
}
