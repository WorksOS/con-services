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
    public class CutFillStatisticsComputeFunc_ClusterCompute : BaseRaptorComputeFunc, IComputeFunc<CutFillStatisticsArgument, CutFillStatisticsResponse>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Default no-arg constructor that orients the request to the available PSNODE servers on the immutable grid projection
        /// </summary>
        public CutFillStatisticsComputeFunc_ClusterCompute() : base(RaptorGrids.RaptorImmutableGridName(), ServerRoles.PSNODE)
        {
        }

        /// <summary>
        /// Invoke the cut fill statistics request locally on this node
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public CutFillStatisticsResponse Invoke(CutFillStatisticsArgument arg)
        {
            Log.Info("In CutFillStatisticsComputeFunc_ClusterCompute.Invoke()");

            try
            {
                Log.Info("Executing CutFillStatisticsComputeFunc_ClusterCompute.Execute()");

                return new CutFillStatisticsResponse()
                {
                    Counts = new long[7] { 0, 0, 0, 0, 0, 0, 0 }
                };
            }
            finally
            {
                Log.Info("Exiting CutFillStatisticsComputeFunc_ClusterCompute.Invoke()");
            }
        }
    }
}
