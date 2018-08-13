using System;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// The represenation of the results of a summary pass count request
  /// </summary>
  public class PassCountSummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// Value of the target pass count if all target pass counts relevant to analysed cell passes are the same.
    /// </summary>
    [JsonProperty(PropertyName = "constantTargetPassCountRange")]
    public TargetPassCountRange ConstantTargetPassCountRange { get; private set; }

    /// <summary>
    /// Are all target pass counts relevant to analysed cell passes are the same?
    /// </summary>
    [JsonProperty(PropertyName = "isTargetPassCountConstant")]
    public bool IsTargetPassCountConstant { get; private set; }

    /// <summary>
    /// The percentage of pass counts that match the target pass count specified in the passCountTarget member of the request
    /// </summary>
    [JsonProperty(PropertyName = "percentEqualsTarget")]
    public double PercentEqualsTarget { get; private set; }

    /// <summary>
    /// The percentage of pass counts that are greater than the target pass count specified in the passCountTarget member of the request
    /// </summary>
    [JsonProperty(PropertyName = "percentGreaterThanTarget")]
    public double PercentGreaterThanTarget { get; private set; }

    /// <summary>
    /// The percentage of pass counts that are less than the target pass count specified in the passCountTarget member of the request
    /// </summary>
    [JsonProperty(PropertyName = "percentLessThanTarget")]
    public double PercentLessThanTarget { get; private set; }

    /// <summary>
    /// The internal returnCode returned by the internal request. Documented elsewhere.
    /// </summary>
    [JsonProperty(PropertyName = "returnCode")]
    public short ReturnCode { get; private set; }

    /// <summary>
    /// The total area covered by non-null cells in the request area
    /// </summary>
    [JsonProperty(PropertyName = "totalAreaCoveredSqMeters")]
    public double TotalAreaCoveredSqMeters { get; private set; }

    public bool HasData() => Math.Abs(this.TotalAreaCoveredSqMeters) > 0.001;

    /// <summary>
    /// Defaullt private constructor.
    /// </summary>
    private PassCountSummaryResult()
    { }

    /// <summary>
    /// Create instance of PassCountSummaryResult
    /// </summary>
    public static PassCountSummaryResult Create(
      TargetPassCountRange constantTargetPassCountRange,
      bool isTargetPassCountConstant,
      double percentEqualsTarget,
      double percentGreaterThanTarget,
      double percentLessThanTarget,
      short returnCode,
      double totalAreaCoveredSqMeters)
    {
      return new PassCountSummaryResult
      {
        ConstantTargetPassCountRange = constantTargetPassCountRange,
        IsTargetPassCountConstant = isTargetPassCountConstant,
        PercentEqualsTarget = percentEqualsTarget,
        PercentGreaterThanTarget = percentGreaterThanTarget,
        PercentLessThanTarget = percentLessThanTarget,
        ReturnCode = returnCode,
        TotalAreaCoveredSqMeters = totalAreaCoveredSqMeters,
      };
    }

    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A string representation of the values in the summary pass count result.</returns>
    public override string ToString()
    {
      return
        $"constantTargetPassCountRange:({this.ConstantTargetPassCountRange.min}, {this.ConstantTargetPassCountRange.max}), isTargetPassCountConstant:{this.IsTargetPassCountConstant}, percentEqualsTarget:{this.PercentEqualsTarget}, percentGreaterThanTarget:{this.PercentGreaterThanTarget}, percentLessThanTarget:{this.PercentLessThanTarget}, totalAreaCoveredSqMeters:{this.TotalAreaCoveredSqMeters}, returnCode:{this.ReturnCode}";
    }
  }
}
