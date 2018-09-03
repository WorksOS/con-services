using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// Represents result returned by CMV Details request for compaction.
  /// </summary>
  public class CompactionCmvDetailedResult : ContractExecutionResult
  {
    /// <summary>
    /// An array of percentages relating to the CMV values encountered in the processed cells.
    /// The percentages are for CMV values between the minimum and target, on target, between the target and the maximum and above the maximum CMV.
    /// </summary>
    [JsonProperty(PropertyName = "percents")]
    public double[] Percents { get; private set; }
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

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private CompactionCmvDetailedResult()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="result1"></param>
    /// <param name="result2"></param>
    /// <param name="settings"></param>
    public CompactionCmvDetailedResult(CMVDetailedResult result1, CMVSummaryResult result2, CMVSettings settings)
    {
      if (result1 != null && result1.HasData())
      {
        Percents = result1.Percents;

        if (result2 == null || !result2.HasData()) return;

        MinCMVPercent = settings.MinCMVPercent;
        MaxCMVPercent = settings.MaxCMVPercent;
        CmvTarget = new CmvTargetData
        {
          CmvMachineTarget = result2.ConstantTargetCmv / 10,
          TargetVaries = !result2.IsTargetCmvConstant
        };
      }
    }

  }
}
