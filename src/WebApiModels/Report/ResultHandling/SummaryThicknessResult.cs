using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.WebApiModels.Report.ResultHandling
{
  /// <summary>
  /// Represents result returned by Summary Volumes request
  /// </summary>
  public class SummaryThicknessResult : ContractExecutionResult
  {
    protected SummaryThicknessResult(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Private constructor
    /// </summary>
    private SummaryThicknessResult()
    {
    }
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

    public static SummaryThicknessResult CreateSummaryThicknessResult(BoundingBox3DGrid convertExtents, double aboveTarget,
        double belowTarget, double matchTarget, double noCoverageArea)
    {
      return new SummaryThicknessResult()
             {
                 BoundingExtents = convertExtents,
                 AboveTarget = aboveTarget,
                 BelowTarget = belowTarget,
                 NoCoverageArea = noCoverageArea,
                 MatchTarget = matchTarget,
             };
    }

    /// <summary>
    /// Create example instance of SummaryVolumesResult to display in Help documentation.
    /// </summary>
    public static SummaryThicknessResult HelpSample
    {
      get
      {
        return new SummaryThicknessResult()
               {
                   BoundingExtents = BoundingBox3DGrid.HelpSample,
                   AboveTarget = 13.2,
                   BelowTarget = 11.3,
                   NoCoverageArea = 32,
                   MatchTarget = 57.5
               };
      }

    }
  }
}