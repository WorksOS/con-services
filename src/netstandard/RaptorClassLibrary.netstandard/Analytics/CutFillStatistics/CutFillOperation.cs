using VSS.TRex.Analytics.Foundation;
using VSS.TRex.Analytics.GridFabric.Arguments;
using VSS.TRex.Analytics.GridFabric.Requests;
using VSS.TRex.Analytics.GridFabric.Responses;
using VSS.TRex.Analytics.Models;

namespace VSS.TRex.Analytics.Operations
{

    public class CutFillOperation : AnalyticsOperation<CutFillStatisticsRequest_ApplicationService, CutFillStatisticsArgument, CutFillStatisticsResponse, CutFillResult>
    { }

    /*
    /// <summary>
    /// Provides a client onsumable operation for performing cut fill analytics that returns a client model space cut fill result.
    /// </summary>
    public class CutFillOperation
    {
        /// <summary>
        /// Execute the cut fill operation with the supplied argument
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public CutFillResult Execute(CutFillStatisticsArgument arg)
        {
            var request = new CutFillStatisticsRequest_ApplicationService();

            CutFillResult result = new CutFillResult();

            return result;
        }
    }
    */
}
