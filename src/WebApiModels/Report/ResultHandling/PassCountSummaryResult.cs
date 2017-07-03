using System;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApiModels.Report.ResultHandling
{
  /// <summary>
  /// The represenation of the results of a summary pass count request
  /// </summary>
  public class PassCountSummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// Value of the target pass count if all target pass counts relevant to analysed cell passes are the same.
    /// </summary>
    public TargetPassCountRange constantTargetPassCountRange { get; private set; }

    /// <summary>
    /// Are all target pass counts relevant to analysed cell passes are the same?
    /// </summary>
    public bool isTargetPassCountConstant { get; private set; }

    /// <summary>
    /// The percentage of pass counts that match the target pass count specified in the passCountTarget member of the request
    /// </summary>
    public double percentEqualsTarget { get; private set; }

    /// <summary>
    /// The percentage of pass counts that are greater than the target pass count specified in the passCountTarget member of the request
    /// </summary>
    public double percentGreaterThanTarget { get; private set; }

    /// <summary>
    /// The percentage of pass counts that are less than the target pass count specified in the passCountTarget member of the request
    /// </summary>
    public double percentLessThanTarget { get; private set; }

    /// <summary>
    /// The internal returnCode returned by the internal request. Documented elsewhere.
    /// </summary>
    public short returnCode { get; private set; }

    /// <summary>
    /// The total area covered by non-null cells in the request area
    /// </summary>
    public double totalAreaCoveredSqMeters { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private PassCountSummaryResult() 
    {}

    /// <summary>
    /// Create instance of PassCountSummaryResult
    /// </summary>
    public static PassCountSummaryResult CreatePassCountSummaryResult(
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
        constantTargetPassCountRange = constantTargetPassCountRange,
        isTargetPassCountConstant = isTargetPassCountConstant,
        percentEqualsTarget = percentEqualsTarget,
        percentGreaterThanTarget = percentGreaterThanTarget,
        percentLessThanTarget = percentLessThanTarget,
        returnCode = returnCode,
        totalAreaCoveredSqMeters = totalAreaCoveredSqMeters,
      };
    }

    /// <summary>
    /// Create example instance of PassCountSummaryResult to display in Help documentation.
    /// </summary>
    public static PassCountSummaryResult HelpSample
    {
      get
      {
        return new PassCountSummaryResult
        {
        };
      }
    }

    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A string representation of the values in the summary pass count result.</returns>
    public override string ToString()
    {
      return String.Format("constantTargetPassCountRange:({0}, {1}), isTargetPassCountConstant:{2}, percentEqualsTarget:{3}, percentGreaterThanTarget:{4}, percentLessThanTarget:{5}, totalAreaCoveredSqMeters:{6}, returnCode:{7}",
                            constantTargetPassCountRange.min, constantTargetPassCountRange.max, isTargetPassCountConstant, percentEqualsTarget, percentGreaterThanTarget, percentLessThanTarget, totalAreaCoveredSqMeters, returnCode);
    }

  

  }
}