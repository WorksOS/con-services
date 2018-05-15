using VSS.TRex.Analytics.GridFabric.Arguments;
using VSS.TRex.Analytics.GridFabric.Requests;
using VSS.TRex.Analytics.GridFabric.Responses;

namespace VSS.TRex.Analytics.GridFabric.ComputeFuncs
{
    /// <summary>
    /// Cut/fill statistics specific request to make to the application service context
    /// </summary>
    public class CutFillStatisticsComputeFunc_ApplicationService : AnalyticsComputeFunc_ApplicationService<CutFillStatisticsArgument, CutFillStatisticsResponse, CutFillStatisticsRequest_ClusterCompute>
    {
    }
}
