using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.CutFillStatistics.GridFabric
{
    /// <summary>
    /// Sends a request to the grid for a cut fill statistics request to be computed
    /// </summary>
    public class CutFillStatisticsRequest_ClusterCompute : GenericPSNodeBroadcastRequest<CutFillStatisticsArgument, CutFillStatisticsComputeFunc_ClusterCompute, CutFillStatisticsResponse>
    {
    }
}

