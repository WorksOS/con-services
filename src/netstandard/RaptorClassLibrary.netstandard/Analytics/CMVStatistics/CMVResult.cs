using VSS.TRex.Analytics.Foundation.Models;

namespace VSS.TRex.Analytics.CMVStatistics
{
  public class CMVResult : SummaryAnalyticsResult
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
    public short ReturnCode { get; set; }
  }
}
