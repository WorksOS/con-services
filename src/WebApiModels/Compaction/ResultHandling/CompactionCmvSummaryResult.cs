using MasterDataProxies.ResultHandling;
using Newtonsoft.Json;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Compaction.ResultHandling
{
  /// <summary>
  /// Represents result returned by CMV Summary request for compaction
  /// </summary>
  public class CompactionCmvSummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// The CMV summary data results
    /// </summary>
    [JsonProperty(PropertyName = "cmvSummaryData")]
    public CmvSummaryData SummaryData { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CompactionCmvSummaryResult()
    { }


    /// <summary>
    /// CmvSummaryResult create instance
    /// </summary>
    /// <param name="result"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static CompactionCmvSummaryResult CreateCmvSummaryResult(CMVSummaryResult result, CMVSettings settings)
    {
      var cmvResult = new CompactionCmvSummaryResult
      {
        SummaryData = new CmvSummaryData
        {
          PercentEqualsTarget = result.compactedPercent,
          PercentGreaterThanTarget = result.overCompactedPercent,
          PercentLessThanTarget = result.underCompactedPercent,
          TotalAreaCoveredSqMeters = result.totalAreaCoveredSqMeters,
          CmvTarget = new CmvTargetData
          {
            CmvMachineTarget = result.constantTargetCMV / 10,
            TargetVaries = !result.isTargetCMVConstant
          },
          MinCMVPercent = settings.minCMVPercent,
          MaxCMVPercent = settings.maxCMVPercent
        }
      };
      return cmvResult;
    }

    /// <summary>
    /// CMV summary data returned
    /// </summary>
    public class CmvSummaryData
    {
      /// <summary>
      /// The percentage of cells that are compacted within the target bounds
      /// </summary>
      [JsonProperty(PropertyName = "percentEqualsTarget")]
      public double PercentEqualsTarget { get; set; }
      /// <summary>
      /// The percentage of the cells that are over-compacted
      /// </summary>
      [JsonProperty(PropertyName = "percentGreaterThanTarget")]
      public double PercentGreaterThanTarget { get; set; }
      /// <summary>
      /// The percentage of the cells that are under compacted
      /// </summary>
      [JsonProperty(PropertyName = "percentLessThanTarget")]
      public double PercentLessThanTarget { get; set; }
      /// <summary>
      /// The total area covered by non-null cells in the request area
      /// </summary>
      [JsonProperty(PropertyName = "totalAreaCoveredSqMeters")]
      public double TotalAreaCoveredSqMeters { get; set; }
      /// <summary>
      /// CMV machine target and whether it is constant or varies.
      /// </summary>
      [JsonProperty(PropertyName = "cmvTarget")]
      public CmvTargetData CmvTarget { get; set; }
      /// <summary>
      /// The minimum percentage the measured CMV may be compared to the cmvTarget from the machine
      /// </summary>
      [JsonProperty(PropertyName = "minCMVPercent", Required = Required.Default)]
      public double MinCMVPercent { get; set; }
      /// <summary>
      /// The maximum percentage the measured CMV may be compared to the cmvTarget from the machine
      /// </summary>
      [JsonProperty(PropertyName = "maxCMVPercent")]
      public double MaxCMVPercent { get; set; }

    }

    /// <summary>
    /// CMV target data returned
    /// </summary>
    public class CmvTargetData
    {
      /// <summary>
      /// If the CMV value is constant, this is the constant value of all CMV targets in the processed data.
      /// </summary>
      [JsonProperty(PropertyName = "cmvMachineTarget")]
      public double CmvMachineTarget { get; set; }
      /// <summary>
      /// Are the CMV target values applying to all processed cells varying?
      /// </summary>
      [JsonProperty(PropertyName = "targetVaries")]
      public bool TargetVaries { get; set; }
    }
  }
}
