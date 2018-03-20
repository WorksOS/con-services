using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Responses;
using VSS.VisionLink.Raptor.Analytics.Coordinators;
using VSS.VisionLink.Raptor.Analytics.Models;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Servers;

namespace VSS.VisionLink.Raptor.Analytics.GridFabric.ComputeFuncs
{
    public class CutFillStatisticsComputeFunc_ClusterCompute : AnalyticsComputeFunc_ClusterCompute<CutFillStatisticsArgument, CutFillStatisticsResponse, CutFillCoordinator>
    {
        public CutFillStatisticsComputeFunc_ClusterCompute(string gridName, string role) : base(gridName, role)
        {
        }

        public CutFillStatisticsComputeFunc_ClusterCompute() : base(RaptorGrids.RaptorImmutableGridName(), ServerRoles.PSNODE)
        {
        }
    }
}
