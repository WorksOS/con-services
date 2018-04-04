using System;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.Report.ResultHandling
{
  /// <summary>
  /// Represents result returned by Summary CMV Change request
  /// </summary>
  public class CMVChangeSummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// Default private constructor.
    /// </summary>
    private CMVChangeSummaryResult()
    { }

    /// <summary>
    /// Percent of the cells meeting values request conditions
    /// </summary>
    public double[] Values { get; private set; }

    /// <summary>
    /// Gets the coverage area where we have not null measured CCV
    /// </summary>
    public double CoverageArea { get; private set; }

    public bool HasData() => Math.Abs(this.CoverageArea) > 0.001;

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static CMVChangeSummaryResult Create(double[] values, double coverageArea)
    {
      return new CMVChangeSummaryResult
      {
        Values = values,
        CoverageArea = coverageArea
      };
    }
  }
}