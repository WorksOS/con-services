using VSS.TRex.Analytics.Foundation.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CMVStatistics.Summary
{
  /// <summary>
  /// The result obtained from performing a CMV summary analytics request
  /// </summary>
  public class CMVSummaryResult : SummaryAnalyticsResult
  {
    /// <summary>
    /// Is the CMV target value applying to all processed cells constant?
    /// </summary>
    public bool IsTargetCMVConstant { get; set; }

    /// <summary>
    /// The CMV target value applied to all processed cells.
    /// </summary>
    public short ConstantTargetCMV { get; set; }

    /// <summary>
    /// The internal result code of the request. Documented elsewhere.
    /// </summary>
    public MissingTargetDataResultType ReturnCode { get; set; }
  }
}
