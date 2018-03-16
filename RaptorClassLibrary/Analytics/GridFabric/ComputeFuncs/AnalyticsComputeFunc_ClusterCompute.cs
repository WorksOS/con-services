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
using VSS.VisionLink.Raptor.GridFabric.Requests.Interfaces;
using VSS.VisionLink.Raptor.Servers;

namespace VSS.VisionLink.Raptor.Analytics.GridFabric.ComputeFuncs
{
    public class AnalyticsComputeFunc_ClusterCompute<TArgument, TResponse> : BaseRaptorComputeFunc_Aggregative<TArgument, TResponse>
        where TArgument : class, new()
        where TResponse : class, IResponseAggregateWith<TResponse>, new()
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Invoke the statistics request locally on this node
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public override TResponse Invoke(TArgument arg)
        {
            Log.Info("In AnalyticsComputeFunc_ClusterCompute.Invoke()");

            try
            {
                Log.Info("Executing AnalyticsComputeFunc_ClusterCompute.Execute()");

                // Execute the coordinator and return the response
                return new TResponse();
            }
            finally
            {
                Log.Info("Exiting AnalyticsComputeFunc_ClusterCompute.Invoke()");
            }
        }
    }
}
