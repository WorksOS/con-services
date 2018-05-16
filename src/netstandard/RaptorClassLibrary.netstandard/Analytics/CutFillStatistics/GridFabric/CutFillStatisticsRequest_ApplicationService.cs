using System;
using VSS.TRex.Analytics.GridFabric.Arguments;
using VSS.TRex.Analytics.GridFabric.ComputeFuncs;
using VSS.TRex.Analytics.GridFabric.Responses;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.GridFabric.Requests
{
    /// <summary>
    /// Sends a request to the grid for a cut fill statistics request to be executed
    /// </summary>
    public class CutFillStatisticsRequest_ApplicationService : GenericASNodeRequest<CutFillStatisticsArgument, CutFillStatisticsComputeFunc_ApplicationService, CutFillStatisticsResponse>
    {
    }
}

