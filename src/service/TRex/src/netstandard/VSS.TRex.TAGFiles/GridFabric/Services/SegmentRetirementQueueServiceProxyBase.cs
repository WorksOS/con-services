using System;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Services;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.NodeFilters;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.TAGFiles.GridFabric.Services
{

  /// <summary>
  /// Class responsible for deploying the segment retirement queue service
  /// </summary>
  public class SegmentRetirementQueueServiceProxyBase
  {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<SegmentRetirementQueueServiceProxyBase>();

        /// <summary>
        /// The cluster wide name of the deployed service
        /// </summary>
        public const string ServiceName = "SegmentRetirementQueueService";

        /// <summary>
        /// Services interface for the cluster group projection
        /// </summary>
        private IServices services;

        /// <summary>
        /// The proxy to the deployed service
        /// </summary>
        private ISegmentRetirementQueueService proxy;

        /// <summary>
        /// The node filter to be used to control deployment of the segment retirement service
        /// </summary>
        private RoleBasedServerNodeFilter NodeFilter;

        public SegmentRetirementQueueServiceProxyBase(StorageMutability mutability, RoleBasedServerNodeFilter nodeFilter)
        {
            NodeFilter = nodeFilter;

            IIgnite _ignite = DIContext.Obtain<ITRexGridFactory>().Grid(mutability);

            // Get an instance of IServices for the cluster group.
            services = _ignite.GetServices();
        }

        /// <summary>
        /// Deploys the segment retirement queue service on to each node in the grid.
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
                Log.LogError("Exception thrown while attempting to cancel service", E);
                throw;
            }

            try
            {
                Log.LogInformation("Deploying new service");

                services.Deploy(new ServiceConfiguration()
                {
                    Name = ServiceName,
                    Service = new SegmentRetirementQueueService(),
                    TotalCount = 0,
                    MaxPerNodeCount = 1,
                    NodeFilter = NodeFilter
                });
            }
            catch (Exception E)
            {
                Log.LogError("Exception thrown while attempting to deploy service", E);
                throw;
            }

            try
            {
                Log.LogInformation($"Obtaining service proxy for {ServiceName}");
                proxy = services.GetServiceProxy<ISegmentRetirementQueueService>(ServiceName);
            }
            catch (Exception E)
            {
                Log.LogError("Exception thrown while attempting to get service proxy", E);
                throw;
            }
        }
    }
}
