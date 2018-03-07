using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.ResultHandling
{
  /// <summary>
  /// The represenation of the results of a detailed pass count request
  /// </summary>
  public class PassCountDetailedResult : ContractExecutionResult
  {
    /// <summary>
    /// Range of the target pass count values if all target pass counts relevant to analysed cell passes are the same.
    /// </summary>
    public TargetPassCountRange constantTargetPassCountRange { get; private set; }

    /// <summary>
    /// Are all target pass counts relevant to analysed cell passes are the same?
    /// </summary>
    public bool isTargetPassCountConstant { get; private set; }

    /// <summary>
    /// Collection of passcount percentages where each element represents the percentage of the matching index passcount number provided in the 
    /// passCounts member of the pass count request representation.
    /// </summary>
    public double[] percents { get; private set; }


    /// <summary>
    /// Gets the total coverage area for the production data - not the total area specified in filter
    /// </summary>
    /// <value>
    /// The total coverage area in sq meters.
    /// </value>
    public double TotalCoverageArea { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private PassCountDetailedResult()
    {}

    /// <summary>
    /// Create instance of PassCountSummaryResult
    /// </summary>
    public static PassCountDetailedResult CreatePassCountDetailedResult(
      TargetPassCountRange constantTargetPassCountRange,
      bool isTargetPassCountConstant,
      double[] percents, double totalArea)
    {
      return new PassCountDetailedResult
      {
        constantTargetPassCountRange = constantTargetPassCountRange,
        isTargetPassCountConstant = isTargetPassCountConstant,
        percents = percents,
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
        $"constantTargetPassCountRange:({this.constantTargetPassCountRange.min}, {this.constantTargetPassCountRange.max}), isTargetPassCountConstant:{this.isTargetPassCountConstant}, percents:{string.Join("%, ", this.percents) + "%"}";
    }

  }
}