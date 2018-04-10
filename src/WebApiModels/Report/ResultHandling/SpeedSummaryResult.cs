using System;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.Report.ResultHandling
{
  /// <summary>
  /// Represents result returned by Summary Volumes request
  /// </summary>
  public class SpeedSummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// Default private constructor.
    /// </summary>
    private SpeedSummaryResult()
    { }

    /// <summary>
    /// Area above speed target
    /// </summary>
    public double AboveTarget { get; private set; }

    /// <summary>
    /// Area below speed target
    /// </summary>
    public double BelowTarget { get; private set; }

    /// <summary>
    /// Area within speed target
    /// </summary>
    public double MatchTarget { get; private set; }

    /// <summary>
    /// Total coverage area 
    /// </summary>
    public double CoverageArea { get; private set; }

    public bool HasData() => Math.Abs(this.CoverageArea) > 0.001;

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static SpeedSummaryResult Create(double aboveTarget, double belowTarget, double matchTarget, double CoverageArea)
    {
      return new SpeedSummaryResult
      {
        AboveTarget = aboveTarget,
        BelowTarget = belowTarget,
        CoverageArea = CoverageArea,
        MatchTarget = matchTarget,
      };
    }
  }
}