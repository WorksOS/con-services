using System;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// The result representation of a summary CMV request
  /// </summary>
  public class CMVSummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// The percentage of cells that are compacted within the target bounds
    /// </summary>
    [JsonProperty(PropertyName = "compactedPercent")]
    public double CompactedPercent { get; private set; }

    /// <summary>
    /// If the CMV value is constant, this is the constant value of all CMV targets in the processed data.
    /// </summary>
    [JsonProperty(PropertyName = "constantTargetCMV")]
    public short ConstantTargetCmv { get; private set; }

    /// <summary>
    /// Are the CMV target values applying to all processed cells constant?
    /// </summary>
    [JsonProperty(PropertyName = "isTargetCMVConstant")]
    public bool IsTargetCmvConstant { get; private set; }

    /// <summary>
    /// The percentage of the cells that are over-compacted
    /// </summary>
    [JsonProperty(PropertyName = "overCompactedPercent")]
    public double OverCompactedPercent { get; private set; }

    /// <summary>
    /// The internal result code of the request. Documented elsewhere.
    /// </summary>
    /// The total area covered by non-null cells in the request area
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
    private CMVSummaryResult() 
    {}

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static CMVSummaryResult Create(
      double compactedPercent,
      short constantTargetCmv,
      bool isTargetCmvConstant,
      double overCompactedPercent,
      short returnCode,
      double totalAreaCoveredSqMeters,
      double underCompactedPercent
      )
    {
      return new CMVSummaryResult
      {
        CompactedPercent = compactedPercent,
        ConstantTargetCmv = constantTargetCmv,
        IsTargetCmvConstant = isTargetCmvConstant,
        OverCompactedPercent = overCompactedPercent,
        ReturnCode = returnCode,
        TotalAreaCoveredSqMeters = totalAreaCoveredSqMeters,
        UnderCompactedPercent = underCompactedPercent
      };
    }
  }
}