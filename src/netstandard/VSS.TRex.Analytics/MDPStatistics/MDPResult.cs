using VSS.TRex.Analytics.Foundation.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.MDPStatistics
{
  /// <summary>
  /// The result obtained from performing a MDP analytics request
  /// </summary>
  public class MDPResult : SummaryAnalyticsResult
  {
    /// <summary>
    /// Is the MDP target value applying to all processed cells constant?
    /// </summary>
    public bool IsTargetMDPConstant { get; set; }

    /// <summary>
    /// The MDP target value applied to all processed cells.
    /// </summary>
    public short ConstantTargetMDP { get; set; }

    /// <summary>
    /// The internal result code of the request. Documented elsewhere.
    /// </summary>
    public MissingTargetDataResultType ReturnCode { get; set; }
  }
}
