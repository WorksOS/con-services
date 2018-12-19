using System;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Services;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.GridFabric.NodeFilters;

namespace VSS.TRex.TAGFiles.GridFabric.Services
{

  /// <summary>
  /// Class responsible for deploying the TAG file buffered queue service
  /// </summary>
  public class TAGFileBufferQueueServiceProxy
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<TAGFileBufferQueueServiceProxy>();

        /// <summary>
        /// The cluster wide name of the deployed service
        /// </summary>
        public const string ServiceName = "TAGFileBufferQueueService";

        /// <summary>
        /// Services interface for the cluster group projection
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
            IIgnite _ignite = DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Mutable);

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
                Log.LogError(E, "Exception thrown while attempting to cancel service");
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
                Log.LogError(E, "Exception thrown while attempting to deploy service");
                throw;
            }

            try
            {
                Log.LogInformation($"Obtaining service proxy for {ServiceName}");
                proxy = services.GetServiceProxy<ITAGFileBufferQueueService>(ServiceName);
            }
            catch (Exception E)
            {
                Log.LogError(E, "Exception thrown while attempting to get service proxy");
                throw;
            }
        }
    }
}
