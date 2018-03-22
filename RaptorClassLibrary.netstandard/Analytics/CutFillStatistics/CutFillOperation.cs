using System;
using System.Collections.Generic;
using System.Text;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Requests;
using VSS.VisionLink.Raptor.Analytics.Models;

namespace VSS.VisionLink.Analytics.Operations
{
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
            result.PopulateFromClusterComputeResponse(request.Execute(arg));

            return result;
        }
    }
}
