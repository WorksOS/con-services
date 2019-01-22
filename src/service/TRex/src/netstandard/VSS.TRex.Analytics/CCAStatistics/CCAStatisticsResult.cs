using VSS.TRex.Analytics.Foundation.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CCAStatistics
{
  /// <summary>
  /// The result obtained from performing a CCA statistics analytics request
  /// </summary>
  public class CCAStatisticsResult : StatisticsAnalyticsResult
  {
    /// <summary>
    /// Is the CCA target value applying to all processed cells constant?
    /// </summary>
    public bool IsTargetCCAConstant { get; set; }

    /// <summary>
    /// The CCA target value applied to all processed cells.
    /// </summary>
    public short ConstantTargetCCA { get; set; }

    /// <summary>
    /// The internal result code of the request. Documented elsewhere.
    /// </summary>
    public MissingTargetDataResultType ReturnCode { get; set; }
  }
}
