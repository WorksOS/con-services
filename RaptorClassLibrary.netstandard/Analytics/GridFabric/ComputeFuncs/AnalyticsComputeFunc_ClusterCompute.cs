using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Analytics.Coordinators;
using VSS.VisionLink.Raptor.Analytics.Models;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.GridFabric.Requests.Interfaces;

namespace VSS.VisionLink.Raptor.Analytics.GridFabric.ComputeFuncs
{
    public class AnalyticsComputeFunc_ClusterCompute<TArgument, TResponse, TCoordinator> : BaseRaptorComputeFunc, IComputeFunc<TArgument, TResponse>
        where TArgument : class //, new()
        where TResponse : class, IResponseAggregateWith<TResponse> //, new()
        where TCoordinator : BaseAnalyticsCoordinator<TArgument, TResponse>, new()
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AnalyticsComputeFunc_ClusterCompute(string gridName, string role) : base(gridName, role)
        {
        }

        /// <summary>
        /// Invoke the statistics request locally on this node
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public TResponse Invoke(TArgument arg)
        {
            Log.Info("In AnalyticsComputeFunc_ClusterCompute.Invoke()");

            try
            {
                Log.Info("Executing AnalyticsComputeFunc_ClusterCompute.Execute()");

                TCoordinator coordinator = new TCoordinator();
                return coordinator.Execute(arg);
            }
            finally
            {
                Log.Info("Exiting AnalyticsComputeFunc_ClusterCompute.Invoke()");
            }
        }
    }
}
