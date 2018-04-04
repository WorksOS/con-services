using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Reflection;
using VSS.VisionLink.Raptor.Analytics.Coordinators;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.GridFabric.Requests.Interfaces;
using VSS.VisionLink.Raptor.Servers;

namespace VSS.VisionLink.Raptor.Analytics.GridFabric.ComputeFuncs
{
    public class AnalyticsComputeFunc_ClusterCompute<TArgument, TResponse, TCoordinator> : BaseRaptorComputeFunc, IComputeFunc<TArgument, TResponse>
        where TArgument : class
        where TResponse : class, IAggregateWith<TResponse>
        where TCoordinator : BaseAnalyticsCoordinator<TArgument, TResponse>, new()
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AnalyticsComputeFunc_ClusterCompute(string gridName, string role) : base(gridName, role)
        {
        }

        public AnalyticsComputeFunc_ClusterCompute() : this(RaptorGrids.RaptorImmutableGridName(), ServerRoles.PSNODE)
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
