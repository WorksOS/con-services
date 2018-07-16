using VSS.TRex.Analytics.Foundation.Models;

namespace VSS.TRex.Analytics.CutFillStatistics
{
  /// <summary>
  /// The result obtained from performing a CutFill analytics request
  /// </summary>
  public class CutFillResult : DetailsAnalyticsResult
  {
    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CutFillResult()
    {
    }

    public CutFillResult(long [] counts) : this()
    {
      Counts = counts;
    }
  }
}

