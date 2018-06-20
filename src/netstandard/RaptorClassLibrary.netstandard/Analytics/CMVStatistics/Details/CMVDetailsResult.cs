using VSS.TRex.Analytics.Foundation.Models;

namespace VSS.TRex.Analytics.CMVStatistics.Details
{
  /// <summary>
  /// The result obtained from performing a CMV details analytics request
  /// </summary>
  public class CMVDetailsResult : DetailsAnalyticsResult
  {
    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CMVDetailsResult()
    {
    }

    public CMVDetailsResult(long[] counts) : this()
    {
      Counts = counts;
    }
  }
}
