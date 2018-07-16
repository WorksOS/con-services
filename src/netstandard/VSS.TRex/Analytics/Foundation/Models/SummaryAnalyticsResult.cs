namespace VSS.TRex.Analytics.Foundation.Models
{
  /// <summary>
  /// Base class for summary results sent to client calling contexts for analytics functions
  /// </summary>
  public class SummaryAnalyticsResult : AnalyticsResult
  {
    /// <summary>
    /// The percentage of the cells that are below the target data range
    /// </summary>
    public double BelowTargetPercent { get; set; }

    /// <summary>
    /// The percentage of cells that are within the target data range
    /// </summary>
    public double WithinTargetPercent { get; set; }

    /// <summary>
    /// The percentage of the cells that are above the target data range
    /// </summary>
    public double AboveTargetPercent { get; set; }

    /// <summary>
    /// The total area covered by non-null cells in the request area
    /// </summary>
    public double TotalAreaCoveredSqMeters { get; set; }
  }
}
