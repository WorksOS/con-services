using VSS.VisionLink.Raptor.Analytics.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Requests;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.Analytics.GridFabric.ComputeFuncs
{
    /// <summary>
    /// Cut/fill statistics specific request to make to the application service context
    /// </summary>
    public class CutFillStatisticsComputeFunc_ApplicationService : AnalyticsComputeFunc_ApplicationService<CutFillStatisticsArgument, CutFillStatisticsResponse, CutFillStatisticsRequest_ClusterCompute>
    {
    }
}
