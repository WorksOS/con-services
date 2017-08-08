using Newtonsoft.Json;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Compaction.ResultHandling
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
    /// <param name="result">The pass count results from Raptor</param>
    /// <param name="settings">The pass count settings used by Raptor</param>
    /// <returns>An instance of CompactionPassCountSummaryResult</returns>
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
      [JsonProperty(PropertyName = "passCountTarget")]
      public PassCountTargetData PassCountTarget { get; set; }
    }

  }
}
