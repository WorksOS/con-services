using Apache.Ignite.Core.Compute;
using System.Drawing;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Responses;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Requests;

namespace VSS.VisionLink.Raptor.Rendering.Servers.Client
{
    public class CutFillStatistics_AnalyticsServer : AnalyticsServer<CutFillStatisticsRequest_ApplicationService, CutFillStatisticsArgument, CutFillStatisticResponse>
    {
    }
}
