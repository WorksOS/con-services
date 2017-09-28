using VSS.Common.ResultsHandling;

namespace VSS.Productivity3D.WebApiModels.Report.ResultHandling
{
  /// <summary>
  /// Represents result returned by Summary Volumes request
  /// </summary>
  public class SummarySpeedResult : ContractExecutionResult
  {
    protected SummarySpeedResult(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Private constructor
    /// </summary>
    private SummarySpeedResult()
    {
    }

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

    public static SummarySpeedResult CreateSummarySpeedResult(double aboveTarget,
        double belowTarget, double matchTarget, double CoverageArea)
    {
      return new SummarySpeedResult
      {
        AboveTarget = aboveTarget,
        BelowTarget = belowTarget,
        CoverageArea = CoverageArea,
        MatchTarget = matchTarget,
      };
    }
  }
}