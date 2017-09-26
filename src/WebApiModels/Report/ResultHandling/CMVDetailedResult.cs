using System;
using VSS.Common.ResultsHandling;

namespace VSS.Productivity3D.WebApiModels.Report.ResultHandling
{
  /// <summary>
  /// The result representation of a detailed CMV request
  /// </summary>
  public class CMVDetailedResult : ContractExecutionResult
  {
    /// <summary>
    /// An array of percentages relating to the CMV values encountered in the processed cells.
    /// The percentages are for CMV values below the minimum, between the minimum and target, on target, between the target and the maximum and above the maximum CMV.
    /// </summary>
    public double[] percents { get; private set; }

       /// <summary>
    /// Private constructor
    /// </summary>
    private CMVDetailedResult() 
    {}

    /// <summary>
    /// Create instance of CMVDetailedResult
    /// </summary>
    public static CMVDetailedResult CreateCMVDetailedResult(
      double[] percents
    )
    {
      return new CMVDetailedResult
      {
        percents = percents
      };
    }

    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A comma separated list of the percentages in the array.</returns>
    public override string ToString()
    {
      return String.Join("%, ", percents) + "%";
    }
  }
}