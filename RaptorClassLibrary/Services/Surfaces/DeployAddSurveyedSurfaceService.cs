using Apache.Ignite.Core;
using Apache.Ignite.Core.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Caches;

namespace VSS.VisionLink.Raptor.Services.Surfaces
{
    /// <summary>
    /// Class responsible for deploying the add surveyed surface service
    /// </summary>
    public class DeployAddSurveyedSurfaceService
    {
        /// <summary>
        /// The cluster wide name of the deployed service
        /// </summary>
        public const string ServiceName = "AddSurveyedSurface";

        /// <summary>
        /// Injected Ignite instance
        /// </summary>
        [InstanceResource]
        private readonly IIgnite _ignite;

        public void Deploy()
        {
            var cacheGrp = _ignite.GetCluster().ForCacheNodes(RaptorCaches.MutableNonSpatialCacheName());

            // Get an instance of IServices for the cluster group.
            var services = cacheGrp.GetServices();

            // Deploy per-node singleton. An instance of the service
            // will be deployed on every node within the cluster group.
            //            services.DeployNodeSingleton("myCounter", new CounterService());
            services.DeployClusterSingleton(ServiceName, new AddSurveyedSurfaceService());
        }

        /// <summary>
        /// Invoke proxy for calling the add surveyed surface service
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="designDescriptor"></param>
        /// <param name="asAtDate"></param>
        public void Invoke(long SiteModelID, DesignDescriptor designDescriptor, DateTime asAtDate)
        {
            var cacheGrp = _ignite.GetCluster().ForCacheNodes(RaptorCaches.MutableNonSpatialCacheName());

            // Get an instance of IServices for the cluster group.
            var services = cacheGrp.GetServices();

            IAddSurveyedSurfaceService proxy = services.GetServiceProxy<IAddSurveyedSurfaceService>(ServiceName);

            proxy.Add(SiteModelID, designDescriptor, asAtDate);
        }
    }
}
