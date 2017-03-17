
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
    public CmvChangeSummaryData[] SummaryData { get; private set; }

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
    public static CompactionCmvPercentChangeResult CreateCmvPercentChangeResult(CMVChangeSummaryResult result, double[] cmvChangeSummarySettings)
    {
      var summaryData = new CmvChangeSummaryData[cmvChangeSummarySettings.Length];
      for (int i = 0; i < summaryData.Length; i++)
      {
        summaryData[i].PercentRange = new double[]
        {
          i == 0 ? 0 : cmvChangeSummarySettings[i-1],
          i == summaryData.Length-1 ? 100 : cmvChangeSummarySettings[i]
        };
        summaryData[i].PercentValue = result.Values[i];
      }
      var cmvPercentChangeResult = new CompactionCmvPercentChangeResult
      {
        SummaryData = summaryData
      };
      return cmvPercentChangeResult;
    }

    /// <summary>
    /// CMV % change summary data returned
    /// </summary>
    public class CmvChangeSummaryData
    {
      /// <summary>
      /// The range that the CMV % change value is for
      /// </summary>
      [JsonProperty(PropertyName = "percentRange")]
      public double[] PercentRange { get; set; }

      /// <summary>
      /// The CMV % change value
      /// </summary>
      [JsonProperty(PropertyName = "percentValue")]
      public double PercentValue { get; set; }
    }
  }
}
