using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// The result representation of a summary CMV request
  /// </summary>
  public class CMVSummaryResult : CMVBaseResult
  {
    /// <summary>
    /// The percentage of cells that are compacted within the target bounds
    /// </summary>
    [JsonProperty(PropertyName = "compactedPercent")]
    public double CompactedPercent { get; private set; }

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
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="compactedPercent"></param>
    /// <param name="constantTargetCmv"></param>
    /// <param name="isTargetCmvConstant"></param>
    /// <param name="overCompactedPercent"></param>
    /// <param name="returnCode"></param>
    /// <param name="totalAreaCoveredSqMeters"></param>
    /// <param name="underCompactedPercent"></param>
    public CMVSummaryResult(
      double compactedPercent,
      short constantTargetCmv,
      bool isTargetCmvConstant,
      double overCompactedPercent,
      short returnCode,
      double totalAreaCoveredSqMeters,
      double underCompactedPercent
      )
    {
      CompactedPercent = compactedPercent;
      ConstantTargetCmv = constantTargetCmv;
      IsTargetCmvConstant = isTargetCmvConstant;
      OverCompactedPercent = overCompactedPercent;
      ReturnCode = returnCode;
      TotalAreaCoveredSqMeters = totalAreaCoveredSqMeters;
      UnderCompactedPercent = underCompactedPercent;
    }
  }
}