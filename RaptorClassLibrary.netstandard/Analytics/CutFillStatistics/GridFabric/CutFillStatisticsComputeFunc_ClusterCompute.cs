using VSS.VisionLink.Raptor.Analytics.Coordinators;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.Analytics.GridFabric.ComputeFuncs
{
    /// <summary>
    /// Cut/fill statistics specific request to make to the cluster compute context
    /// </summary>
    public class CutFillStatisticsComputeFunc_ClusterCompute : AnalyticsComputeFunc_ClusterCompute<CutFillStatisticsArgument, CutFillStatisticsResponse, CutFillCoordinator>
    {
    }
}
