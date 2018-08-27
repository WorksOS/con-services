using System;
using System.Reflection;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.TAGFiles.GridFabric.NodeFilters;

namespace VSS.TRex.TAGFiles.GridFabric.Services
{

  /// <summary>
  /// Class responsible for deploying the TAG file buffered queue service
  /// </summary>
  public class TAGFileBufferQueueServiceProxy
    {
        [NonSerialized]
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// The cluster wide name of the deployed service
        /// </summary>
        public const string ServiceName = "TAGFileBufferQueueService";

        /// <summary>
        /// Services interface for the clustergroup projection
        /// </summary>
        private IServices services;

        /// <summary>
        /// The proxy to the deployed service
        /// </summary>
        private ITAGFileBufferQueueService proxy;

        /// <summary>
        /// No-arg constructor that instantiates the Ignite instance, cluster, service and proxy members
        /// </summary>
        public TAGFileBufferQueueServiceProxy()
        {
            IIgnite _ignite = TRexGridFactory.Grid(TRexGrids.MutableGridName());

            //var cacheGroup = _ignite.GetCluster().ForCacheNodes(TRexCaches.TAGFileBufferQueueCacheName());

            // Get an instance of IServices for the cluster group.
            services = _ignite.GetServices();
        }

        /// <summary>
        /// Deploys the TAG file buffer queue service on to each TAG file processor node in the mjtable grid.
        /// </summary>
        public void Deploy()
        {
            // Attempt to cancel any previously deployed service
            try
            {
                Log.LogInformation($"Cancelling deployed service {ServiceName}");
                services.Cancel(ServiceName);
            }
            catch (Exception E)
            {
                Log.LogError($"Exception {E} thrown while attempting to cancel service");
                throw;
            }

            try
            {
                Log.LogInformation("Deploying new service");

                services.Deploy(new ServiceConfiguration()
                {
                    Name = ServiceName,
                    Service = new TAGFileBufferQueueService(),
                    TotalCount = 0,
                    MaxPerNodeCount = 1,
                    NodeFilter = new TAGProcessorRoleBasedNodeFilter()
                });
            }
            catch (Exception E)
            {
                Log.LogError($"Exception {E} thrown while attempting to deploy service");
                throw;
            }

            try
            {
                Log.LogInformation($"Obtaining service proxy for {ServiceName}");
                proxy = services.GetServiceProxy<ITAGFileBufferQueueService>(ServiceName);
            }
            catch (Exception E)
            {
                Log.LogError($"Exception {E} thrown while attempting to get service proxy");
                throw;
            }
        }
    }
}
