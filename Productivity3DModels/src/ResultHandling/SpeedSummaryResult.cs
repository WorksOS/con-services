using System;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling
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
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="aboveTarget"></param>
    /// <param name="belowTarget"></param>
    /// <param name="matchTarget"></param>
    /// <param name="coverageArea"></param>
    public SpeedSummaryResult(double aboveTarget, double belowTarget, double matchTarget, double coverageArea)
    {
      AboveTarget = aboveTarget;
      BelowTarget = belowTarget;
      CoverageArea = coverageArea;
      MatchTarget = matchTarget;
    }
  }
}