using VSS.TRex.Types;

namespace VSS.TRex.Analytics.GridFabric.Responses
{
    /// <summary>
    /// Base class for analytics response. Defines common state such as ResutlStatus for each response type
    /// </summary>
    public class BaseAnalyticsResponse
    {
        /// <summary>
        /// The result status of the analytics request
        /// </summary>
        public RequestErrorStatus ResultStatus { get; set; } = RequestErrorStatus.Unknown;
    }
}
