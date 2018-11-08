using VSS.TRex.Analytics.Foundation.GridFabric.ComputeFuncs;

namespace VSS.TRex.Analytics.CutFillStatistics.GridFabric
{
    /// <summary>
    /// Cut/fill statistics specific request to make to the application service context
    /// </summary>
    public class CutFillStatisticsComputeFunc_ApplicationService : AnalyticsComputeFunc_ApplicationService<CutFillStatisticsArgument, CutFillStatisticsResponse, CutFillStatisticsRequest_ClusterCompute>
    {
    }
}
