using System;
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
    public class CutFillStatisticsRequest_ApplicationService : GenericASNodeRequest<CutFillStatisticsArgument, CutFillStatisticsComputeFunc_ApplicationService, CutFillStatisticsResponse>
    {
    }
}

