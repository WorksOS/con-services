using System;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApiModels.Report.ResultHandling
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
    /// Create example instance of PassCountDetailedResult to display in Help documentation.
    /// </summary>
    public static PassCountDetailedResult HelpSample
    {
      get
      {
        return new PassCountDetailedResult
        {
          percents = new double[2] {0.1,0.2},
          TotalCoverageArea = 100,
          constantTargetPassCountRange = TargetPassCountRange.HelpSample
        };
      }
    }

    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A string representation of the array of passcount percentages</returns>
    public override string ToString()
    {
      return String.Format("constantTargetPassCountRange:({0}, {1}), isTargetPassCountConstant:{2}, percents:{3}",
                            constantTargetPassCountRange.min, constantTargetPassCountRange.max, isTargetPassCountConstant, string.Join("%, ", percents) + "%");
    }

  }
}