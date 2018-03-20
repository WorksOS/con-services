using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.Report.ResultHandling
{
  /// <summary>
  /// The result representation of a summary CCA request
  /// </summary>
  public class CCASummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// The percentage of cells that are complete within the target bounds
    /// </summary>
    public double completePercent { get; private set; }

    /// <summary>
    /// The percentage of the cells that are over-complete
    /// </summary>
    public double overCompletePercent { get; private set; }

    /// <summary>
    /// The internal result code of the request. Documented elsewhere.
    /// </summary>
    public short returnCode { get; private set; }

    /// <summary>
    /// The total area covered by non-null cells in the request area
    /// </summary>
    public double totalAreaCoveredSqMeters { get; private set; }

    /// <summary>
    /// The percentage of the cells that are under complete
    /// </summary>
    public double underCompletePercent { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CCASummaryResult()
    { }

    /// <summary>
    /// Create instance of CCASummaryResult
    /// </summary>
    public static CCASummaryResult Create(
      double completePercent,
      double overCompactedPercent,
      short returnCode,
      double totalAreaCoveredSqMeters,
      double underCompactedPercent
      )
    {
      return new CCASummaryResult
      {
        completePercent = completePercent,
        overCompletePercent = overCompactedPercent,
        returnCode = returnCode,
        totalAreaCoveredSqMeters = totalAreaCoveredSqMeters,
        underCompletePercent = underCompactedPercent
      };
    }
  }
}