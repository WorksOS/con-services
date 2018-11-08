using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.Report.ResultHandling
{
  /// <summary>
  /// Represents result returned by Summary Volumes request
  /// </summary>
  public class SummaryThicknessResult : ContractExecutionResult
  {
    protected SummaryThicknessResult(string message)
        : base(message)
    { }

    /// <summary>
    /// Private constructor
    /// </summary>
    private SummaryThicknessResult()
    { }

    /// <summary>
    /// Zone boundaries
    /// </summary>
    public BoundingBox3DGrid BoundingExtents { get; private set; }
    /// <summary>
    /// Cut volume in m3
    /// </summary>
    public double AboveTarget { get; private set; }
    /// <summary>
    /// Fill volume in m3
    /// </summary>
    public double BelowTarget { get; private set; }
    /// <summary>
    /// Cut area in m2
    /// </summary>
    public double MatchTarget { get; private set; }

    /// <summary>
    /// Total coverage area (cut + fill + no change) in m2. No Coverage occurs where one of the design or production data pair being compared has no elevation. Where both of the pair have no elevation, nothing will be returned.
    /// </summary>
    public double NoCoverageArea { get; private set; }

    public static SummaryThicknessResult Create(BoundingBox3DGrid convertExtents, double aboveTarget,
        double belowTarget, double matchTarget, double noCoverageArea)
    {
      return new SummaryThicknessResult
      {
        BoundingExtents = convertExtents,
        AboveTarget = aboveTarget,
        BelowTarget = belowTarget,
        NoCoverageArea = noCoverageArea,
        MatchTarget = matchTarget,
      };
    }
  }
}
