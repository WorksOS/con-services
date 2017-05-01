
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.Compaction.ResultHandling
{

  /// <summary>
  /// Represents result returned by CMV % change request for compaction
  /// </summary>
  public class CompactionCmvPercentChangeResult : ContractExecutionResult
  {
    /// <summary>
    /// The CMV % change data results
    /// </summary>
    [JsonProperty(PropertyName = "cmvChangeData")]
    public CmvChangeSummaryData SummaryData { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CompactionCmvPercentChangeResult()
    {
    }


    /// <summary>
    /// CompactionCmvPercentChangeResult create instance
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static CompactionCmvPercentChangeResult CreateCmvPercentChangeResult(CMVChangeSummaryResult result)
    {
      return new CompactionCmvPercentChangeResult
      {
        SummaryData = new CmvChangeSummaryData
        {
          Percents = result.Values,
          TotalAreaCoveredSqMeters = result.CoverageArea
        }
      };
    }

    /// <summary>
    /// CMV % change summary data returned
    /// </summary>
    public class CmvChangeSummaryData
    { 
      /// <summary>
      /// The CMV % change values
      /// </summary>
      [JsonProperty(PropertyName = "percents")]
      public double[] Percents { get; set; }
      /// <summary>
      /// The total area covered by non-null cells in the request area
      /// </summary>
      [JsonProperty(PropertyName = "totalAreaCoveredSqMeters")]
      public double TotalAreaCoveredSqMeters { get; set; }
    }
  }
}
