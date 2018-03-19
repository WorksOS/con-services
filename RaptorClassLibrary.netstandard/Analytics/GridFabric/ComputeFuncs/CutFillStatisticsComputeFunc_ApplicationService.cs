using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Requests;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Responses;
using VSS.VisionLink.Raptor.Analytics.Models;

namespace VSS.VisionLink.Raptor.Analytics.GridFabric.ComputeFuncs
{
    public class CutFillStatisticsComputeFunc_ApplicationService : AnalyticsComputeFunc_ApplicationService<CutFillStatisticsArgument, CutFillStatisticsResponse, CutFillStatisticsComputeFunc_ClusterCompute /*, CutFillResult*/>
    {
    }
}
