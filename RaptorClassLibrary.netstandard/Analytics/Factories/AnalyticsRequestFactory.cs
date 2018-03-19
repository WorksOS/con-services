using Apache.Ignite.Core.Compute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Analytics.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Requests;
using VSS.VisionLink.Raptor.Analytics.Models;
using VSS.VisionLink.Raptor.GridFabric.Requests;

namespace VSS.VisionLink.Raptor.Analytics.Factories
{
    /// <summary>
    /// Factory that creates requests for analytics style operations where the requests are IComputeFunc derivatives
    /// </summary>
    public static class AnalyticsRequestFactory
    {
        /// <summary>
        /// Creates a new request capable of performing cut fill statistics analytics requests
        /// </summary>
        /// <returns></returns>
        public static CutFillStatisticsComputeFunc_ClusterCompute NewCutFillStatisticsRequest() => new CutFillStatisticsComputeFunc_ClusterCompute();
    }
}
