using System;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// The result representation of a summary MDP request
  /// </summary>
  public class MDPSummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// The percentage of cells that are compacted within the target bounds
    /// </summary>
    [JsonProperty(PropertyName = "compactedPercent")]
    public double CompactedPercent { get; private set; }

    /// <summary>
    /// If the MDP value is constant, this is the constant value of all MDP targets in the processed data.
    /// </summary>
    [JsonProperty(PropertyName = "constantTargetMDP")]
    public short ConstantTargetMDP { get; private set; }

    /// <summary>
    /// Are the MDP target values applying to all processed cells constant?
    /// </summary>
    [JsonProperty(PropertyName = "isTargetMDPConstant")]
    public bool IsTargetMDPConstant { get; private set; }

    /// <summary>
    /// The percentage of the cells that are over-compacted
    /// </summary>
    [JsonProperty(PropertyName = "overCompactedPercent")]
    public double OverCompactedPercent { get; private set; }

    /// <summary>
    /// The internal result code of the request. Documented elsewhere.
    /// </summary>
    [JsonProperty(PropertyName = "returnCode")]
    public short ReturnCode { get; private set; }

    /// <summary>
    /// The total area covered by non-null cells in the request area
    /// </summary>
    [JsonProperty(PropertyName = "totalAreaCoveredSqMeters")]
    public double TotalAreaCoveredSqMeters { get; private set; }

    /// <summary>
    /// The percentage of the cells that are under compacted
    /// </summary>
    [JsonProperty(PropertyName = "underCompactedPercent")]
    public double UnderCompactedPercent { get; private set; }

    public bool HasData() => Math.Abs(this.TotalAreaCoveredSqMeters) > 0.001;

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private MDPSummaryResult()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="compactedPercent"></param>
    /// <param name="constantTargetMDP"></param>
    /// <param name="isTargetMDPConstant"></param>
    /// <param name="overCompactedPercent"></param>
    /// <param name="returnCode"></param>
    /// <param name="totalAreaCoveredSqMeters"></param>
    /// <param name="underCompactedPercent"></param>
    /// <returns></returns>
    public MDPSummaryResult(
      double compactedPercent,
      short constantTargetMDP,
      bool isTargetMDPConstant,
      double overCompactedPercent,
      short returnCode,
      double totalAreaCoveredSqMeters,
      double underCompactedPercent
      )
    {
      CompactedPercent = compactedPercent;
      ConstantTargetMDP = constantTargetMDP;
      IsTargetMDPConstant = isTargetMDPConstant;
      OverCompactedPercent = overCompactedPercent;
      ReturnCode = returnCode;
      TotalAreaCoveredSqMeters = totalAreaCoveredSqMeters;
      UnderCompactedPercent = underCompactedPercent;
    }
  }
}
