using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Reponses;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Servers;

namespace VSS.VisionLink.Raptor.Analytics.GridFabric.ComputeFuncs
{
    /// <summary>
    /// This compute func operates in the context of an application server that reaches out to the compute cluster to 
    /// perform subgrid processing.
    /// </summary>
    public class CutFillStatisticsComputeFunc_ApplicationService : BaseRaptorComputeFunc, IComputeFunc<CutFillStatisticsArgument, CutFillStatisticsResponse>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// Default no-arg constructor that orients the request to the available ASNODE servers on the immutable grid projection
        public CutFillStatisticsComputeFunc_ApplicationService() : base(RaptorGrids.RaptorImmutableGridName(), ServerRoles.ASNODE)
        {
        }

        public CutFillStatisticsResponse Invoke(CutFillStatisticsArgument arg)
        {
            Log.Info("In CutFillStatisticsComputeFunc_ApplicationService.Invoke()");

            try
            {
                CutFillStatisticsComputeFunc_ClusterCompute request = new CutFillStatisticsComputeFunc_ClusterCompute();

                Log.Info("Executing CutFillStatisticsComputeFunc_ApplicationService.Execute()");

                return request.Invoke(arg);
            }
            finally
            {
                Log.Info("Exiting CutFillStatisticsComputeFunc_ApplicationService.Invoke()");
            }
        }
    }
}
