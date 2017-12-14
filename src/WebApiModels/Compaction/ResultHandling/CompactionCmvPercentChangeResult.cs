using Newtonsoft.Json;
using System;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
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
    { }

    /// <summary>
    /// CompactionCmvPercentChangeResult create instance
    /// </summary>
    /// <param name="result">The CMV results from Raptor</param>
    /// <returns>An instance of CompactionCmvPercentChangeResult</returns>
    public static CompactionCmvPercentChangeResult CreateCmvPercentChangeResult(CMVChangeSummaryResult result)
    {
      const int noCmvData = 0;

      if (Math.Abs(result.CoverageArea - noCmvData) < 0.001)
      {
        return new CompactionCmvPercentChangeResult { SummaryData = new CmvChangeSummaryData { Percents = new double[0] } };
      }

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