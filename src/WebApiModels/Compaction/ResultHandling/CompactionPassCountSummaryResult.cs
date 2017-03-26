
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.WebApiModels.Report.Models;
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
    public static CompactionPassCountSummaryResult CreatePassCountSummaryResult(PassCountSummaryResult result, PassCountSettings settings)
    {
      var passCountResult = new CompactionPassCountSummaryResult
      {
        SummaryData = new PassCountSummaryData
        {
          PercentEqualsTarget = result.percentEqualsTarget,
          PercentGreaterThanTarget = result.percentGreaterThanTarget,
          PercentLessThanTarget = result.percentLessThanTarget,
          TotalAreaCoveredSqMeters = result.totalAreaCoveredSqMeters,
          MinTarget = settings.passCounts[0],
          MaxTarget = settings.passCounts[1]
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
      /// The minimum percentage the measured PassCount may be compared to the cmvTarget from the machine
      /// </summary>
      [JsonProperty(PropertyName = "minTarget", Required = Required.Default)]
      public int MinTarget { get; set; }
      /// <summary>
      /// The maximum percentage the measured PassCount may be compared to the cmvTarget from the machine
      /// </summary>
      [JsonProperty(PropertyName = "maxTarget")]
      public int MaxTarget { get; set; }

    }

 

  }
}
