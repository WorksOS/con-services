using Apache.Ignite.Core.Compute;
using System.Drawing;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Responses;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Requests;
using VSS.VisionLink.Raptor.Analytics.GridFabric.ComputeFuncs;

namespace VSS.VisionLink.Raptor.Rendering.Servers.Client
{
    public class CutFillStatistics_AnalyticsServer : AnalyticsServer<CutFillStatisticsComputeFunc_ClusterCompute, CutFillStatisticsArgument, CutFillStatisticsResponse>
    {
    }
}
