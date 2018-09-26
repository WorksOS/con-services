using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
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
    /// <param name="result"></param>
    /// <param name="settings"></param>
    public CompactionCmvDetailedResult(CMVDetailedResult result, CMVSettings settings)
    {
      if (result != null && result.HasData())
      {
        Percents = result.Percents;
        MinCMVPercent = settings.MinCMVPercent;
        MaxCMVPercent = settings.MaxCMVPercent;
        CmvTarget = new CmvTargetData
        {
          CmvMachineTarget = result.ConstantTargetCmv / 10,
          TargetVaries = !result.IsTargetCmvConstant
        };
      }
    }

  }
}
