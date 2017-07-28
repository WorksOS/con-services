using System;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;

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
    /// Create example instance of CMVDetailedResult to display in Help documentation.
    /// </summary>
    public static CMVDetailedResult HelpSample
    {
      get
      {
        return new CMVDetailedResult
        {
          percents = new double[] { 7.5, 21.2, 30.3, 24.1, 16.9 }
        };
      }
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