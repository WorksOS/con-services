using Apache.Ignite.Core.Compute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Analytics.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Responses;
using VSS.VisionLink.Raptor.GridFabric.Requests;

namespace VSS.VisionLink.Raptor.Analytics.GridFabric.Requests
{
    /// <summary>
    /// Sends a request to the grid for a cut fill statistics request to be executed
    /// </summary>
    [Serializable]
    public class CutFillStatisticsRequest_ApplicationService : GenericASNodeRequest<CutFillStatisticsArgument, CutFillStatisticsComputeFunc_ApplicationService, CutFillStatisticsResponse> //, IComputeFunc<CutFillStatisticsArgument, CutFillResult>
    {
        /// <summary>
        /// Add specific behaviour here if needed
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public override CutFillStatisticsResponse Execute(CutFillStatisticsArgument arg)
        {
            return base.Execute(arg);
        }
    }
}

