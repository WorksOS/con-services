using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Resource;
using Apache.Ignite.Core.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Surfaces;

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
//        [InstanceResource]
        private readonly IIgnite _ignite;

        /// <summary>
        /// The cluster group the service id deployed onto
        /// </summary>
        private IClusterGroup cacheGrp = null;

        /// <summary>
        /// Services interface for the clustergroup projection
        /// </summary>
        private IServices services = null;

        /// <summary>
        /// The proxy to the deploy service
        /// </summary>
        private IAddSurveyedSurfaceService proxy = null;

        /// <summary>
        /// No-arg constructor that instantiates the Ignitre instance, cluster, service and proxy members
        /// </summary>
        public DeployAddSurveyedSurfaceService()
        {
            _ignite = Ignition.TryGetIgnite(RaptorGrids.RaptorGridName());

            if (_ignite == null)
            {
                _ignite = Ignition.Start();
            }

            cacheGrp = _ignite.GetCluster().ForCacheNodes(RaptorCaches.MutableNonSpatialCacheName()).ForAttribute("Role", "PSNode");

            // Get an instance of IServices for the cluster group.
            services = cacheGrp.GetServices();
        }

        public void Deploy()
        {
            // Attempt to cancel any previously deployed service
            services.Cancel(ServiceName);

            // Deploy per-node singleton. An instance of the service
            // will be deployed on every node within the cluster group.
            //services.DeployNodeSingleton(ServiceName, new AddSurveyedSurfaceService(RaptorGrids.RaptorGridName(), RaptorCaches.MutableNonSpatialCacheName()));
            services.DeployClusterSingleton(ServiceName, new AddSurveyedSurfaceService(RaptorGrids.RaptorGridName(), RaptorCaches.MutableNonSpatialCacheName()));

            proxy = services.GetServiceProxy<IAddSurveyedSurfaceService>(ServiceName, true);
        }

        /// <summary>
        /// Invoke proxy for calling the add surveyed surface service
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="designDescriptor"></param>
        /// <param name="asAtDate"></param>
        public void Invoke_Add(long SiteModelID, DesignDescriptor designDescriptor, DateTime asAtDate)
        {
            proxy.Add(SiteModelID, designDescriptor, asAtDate);
        }

        /// <summary>
        /// Invoke proxy for calling the list surveyed surface service
        /// </summary>
        /// <param name="SiteModelID"></param>
        public SurveyedSurfaces Invoke_List(long SiteModelID)
        {
            return proxy.List(SiteModelID);
        }
    }
}
