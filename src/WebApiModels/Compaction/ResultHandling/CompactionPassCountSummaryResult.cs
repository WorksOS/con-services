using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.Compaction.ResultHandling
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


    /// <summary>
    /// PassCountSummaryResult create instance
    /// </summary>
    /// <param name="result"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static CompactionPassCountSummaryResult CreatePassCountSummaryResult(PassCountSummaryResult result)
    {
      var passCountResult = new CompactionPassCountSummaryResult
      {
        SummaryData = new PassCountSummaryData
        {
          PercentEqualsTarget = result.percentEqualsTarget,
          PercentGreaterThanTarget = result.percentGreaterThanTarget,
          PercentLessThanTarget = result.percentLessThanTarget,
          TotalAreaCoveredSqMeters = result.totalAreaCoveredSqMeters,
          PassCountTarget = new PassCountTargetData
          {
            MinPassCountMachineTarget = result.constantTargetPassCountRange.min,
            MaxPassCountMachineTarget = result.constantTargetPassCountRange.max,
            TargetVaries = !result.isTargetPassCountConstant
          }         
        }
      };
      return passCountResult;
    }

    /// <summary>
    /// Pass Count summary data returned
    /// </summary>
    public class PassCountSummaryData
    {
      /// <summary>
      /// The percentage of pass counts that match the target pass count specified in the passCountTarget member of the request
      /// </summary>
      [JsonProperty(PropertyName = "percentEqualsTarget")]
      public double PercentEqualsTarget { get; set; }
      /// <summary>
      /// The percentage of pass counts that are greater than the target pass count specified in the passCountTarget member of the request
      /// </summary>
      [JsonProperty(PropertyName = "percentGreaterThanTarget")]
      public double PercentGreaterThanTarget { get; set; }
      /// <summary>
      /// The percentage of pass counts that are less than the target pass count specified in the passCountTarget member of the request
      /// </summary>
      [JsonProperty(PropertyName = "percentLessThanTarget")]
      public double PercentLessThanTarget { get; set; }
      /// <summary>
      /// The total area covered by non-null cells in the request area
      /// </summary>
      [JsonProperty(PropertyName = "totalAreaCoveredSqMeters")]
      public double TotalAreaCoveredSqMeters { get; set; }
      /// <summary>
      /// Pass count machine target and whether it is constant or varies.
      /// </summary>
      [JsonProperty(PropertyName = "PassCountTarget")]
      public PassCountTargetData PassCountTarget { get; set; }
    }

  }
}
