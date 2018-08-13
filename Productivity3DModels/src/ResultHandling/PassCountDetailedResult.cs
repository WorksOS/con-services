using System;
using System.Linq;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// The represenation of the results of a detailed pass count request
  /// </summary>
  public class PassCountDetailedResult : ContractExecutionResult
  {
    /// <summary>
    /// Range of the target pass count values if all target pass counts relevant to analysed cell passes are the same.
    /// </summary>
    [JsonProperty(PropertyName = "constantTargetPassCountRange")]
    public TargetPassCountRange ConstantTargetPassCountRange { get; private set; }

    /// <summary>
    /// Are all target pass counts relevant to analysed cell passes are the same?
    /// </summary>
    [JsonProperty(PropertyName = "isTargetPassCountConstant")]
    public bool IsTargetPassCountConstant { get; private set; }

    /// <summary>
    /// Collection of passcount percentages where each element represents the percentage of the matching index passcount number provided in the 
    /// passCounts member of the pass count request representation.
    /// </summary>
    [JsonProperty(PropertyName = "percents")]
    public double[] Percents { get; private set; }

    /// <summary>
    /// Gets the total coverage area for the production data - not the total area specified in filter
    /// </summary>
    /// <value>
    /// The total coverage area in sq meters.
    /// </value>
    [JsonProperty(PropertyName = "totalCoverageArea")]
    public double TotalCoverageArea { get; private set; }

    /// <summary>
    /// Gets whether the Pass Count result object contains data.
    /// </summary>
    /// <remarks>
    /// It's not enough to check the coverage area, if the Percents array contains non zero data then that affects the result.
    /// </remarks>
    /// <returns></returns>
    public bool HasData() => Math.Abs(this.TotalCoverageArea) > 0.001 || (Percents?.Any(d => Math.Abs(d) > 0.001) ?? false);

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private PassCountDetailedResult()
    { }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static PassCountDetailedResult Create(TargetPassCountRange constantTargetPassCountRange, bool isTargetPassCountConstant, double[] percents, double totalArea)
    {
      return new PassCountDetailedResult
      {
        ConstantTargetPassCountRange = constantTargetPassCountRange,
        IsTargetPassCountConstant = isTargetPassCountConstant,
        Percents = percents,
        TotalCoverageArea = totalArea
      };
    }
    
    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A string representation of the array of passcount percentages</returns>
    public override string ToString()
    {
      return
        $"constantTargetPassCountRange:({this.ConstantTargetPassCountRange.min}, {this.ConstantTargetPassCountRange.max}), isTargetPassCountConstant:{this.IsTargetPassCountConstant}, percents:{string.Join("%, ", this.Percents) + "%"}";
    }
  }
}
