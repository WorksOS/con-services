using Apache.Ignite.Core.Compute;
using System.Drawing;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Reponses;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Requests;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.GridFabric.Requests.Interfaces;
using VSS.VisionLink.Raptor.Rendering.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Rendering.GridFabric.Requests;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.Servers.Client;

namespace VSS.VisionLink.Raptor.Rendering.Servers.Client
{
    public class CutFillStatistics_AnalyticsServer : AnalyticsServer<CutFillStatisticsRequest_ApplicationService, CutFillStatisticsArgument, CutFillStatisticsResponse>
    {
    }
}
