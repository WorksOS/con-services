
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;


namespace VSS.Raptor.Service.WebApiModels.Compaction.ResultHandling
{
  /// <summary>
  /// Represents result returned by Speed Summary request for compaction
  /// </summary>
  public class CompactionSpeedSummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// The Speed summary data results
    /// </summary>
    [JsonProperty(PropertyName = "speedSummaryData")]
    public SpeedSummaryData SummaryData { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CompactionSpeedSummaryResult()
    { }


    /// <summary>
    /// SpeedSummaryResult create instance
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static CompactionSpeedSummaryResult CreateSpeedSummaryResult(SummarySpeedResult result)
    {
      var speedResult = new CompactionSpeedSummaryResult
      {
        SummaryData = new SpeedSummaryData
        {
          PercentEqualsTarget = result.MatchTarget/result.CoverageArea*100.0,
          PercentGreaterThanTarget = result.AboveTarget/result.CoverageArea*100.0,
          PercentLessThanTarget = result.BelowTarget/result.CoverageArea*100.0,
          TotalAreaCoveredSqMeters = result.CoverageArea
        }
      };
      return speedResult;
    }

    /// <summary>
    /// Speed summary data returned
    /// </summary>
    public class SpeedSummaryData
    {
      /// <summary>
      /// The percentage of cells within speed target
      /// </summary>
      [JsonProperty(PropertyName = "percentEqualsTarget")]
      public double PercentEqualsTarget { get; set; }
      /// <summary>
      /// The percentage of the cells over speed target
      /// </summary>
      [JsonProperty(PropertyName = "percentGreaterThanTarget")]
      public double PercentGreaterThanTarget { get; set; }
      /// <summary>
      /// The percentage of the cells under speed target
      /// </summary>
      [JsonProperty(PropertyName = "percentLessThanTarget")]
      public double PercentLessThanTarget { get; set; }
      /// <summary>
      /// The total area covered by non-null cells in the request area
      /// </summary>
      [JsonProperty(PropertyName = "totalAreaCoveredSqMeters")]
      public double TotalAreaCoveredSqMeters { get; set; }
 

    }

  }
}
