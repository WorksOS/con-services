using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.Models
{
    /// <summary>
    /// Base class for results sent to client calling contexts for analytics functions
    /// </summary>
    public class AnalyticsResult : IAnalyticsResult
  {
        public RequestErrorStatus ResultStatus { get; set; } = RequestErrorStatus.Unknown;
    }
}
