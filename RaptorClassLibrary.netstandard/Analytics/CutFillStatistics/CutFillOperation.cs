using VSS.TRex.Analytics.GridFabric.Arguments;
using VSS.TRex.Analytics.GridFabric.Requests;
using VSS.TRex.Analytics.Models;

namespace VSS.TRex.Analytics.Operations
{
    /// <summary>
    /// Provides a client onsumable operation for performing cut fill analytics that returns a client model space cut fill result.
    /// </summary>
    public static class CutFillOperation
    {
        /// <summary>
        /// Execute the cut fill operation with the supplied argument
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static CutFillResult Execute(CutFillStatisticsArgument arg)
        {
            var request = new CutFillStatisticsRequest_ApplicationService();

            CutFillResult result = new CutFillResult();
            result.PopulateFromClusterComputeResponse(request.Execute(arg));

            return result;
        }
    }
}
