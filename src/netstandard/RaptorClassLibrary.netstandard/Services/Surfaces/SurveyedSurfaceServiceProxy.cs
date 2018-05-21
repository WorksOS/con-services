using Apache.Ignite.Core;
using Apache.Ignite.Core.Services;
using System;
using VSS.TRex.Designs;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.NodeFilters;
using VSS.TRex.Storage;
using VSS.TRex.Surfaces;

namespace VSS.TRex.Services.Surfaces
{
    /// <summary>
    /// Class responsible for deploying the add surveyed surface service
    /// </summary>
    public class SurveyedSurfaceServiceProxy
    {
        /// <summary>
        /// The cluster wide name of the deployed service
        /// </summary>
        public const string ServiceName = "AddSurveyedSurface";

        /// <summary>
        /// Services interface for the clustergroup projection
        /// </summary>
        private IServices services;

        /// <summary>
        /// The proxy to the deployed service
        /// </summary>
        private ISurveyedSurfaceService proxy;

        /// <summary>
        /// No-arg constructor that instantiates the Ignite instance, cluster, service and proxy members
        /// </summary>
        public SurveyedSurfaceServiceProxy()
        {
            IIgnite _ignite = TRexGridFactory.Grid(TRexGrids.ImmutableGridName());

            // Get an instance of IServices for the cluster group.
            services = _ignite.GetCluster().GetServices();
        }

        public void Deploy()
        {
            // Attempt to cancel any previously deployed service
            services.Cancel(ServiceName);

            services.Deploy(new ServiceConfiguration()
            {
                Name = ServiceName,
                Service = new SurveyedSurfaceService(StorageMutability.Immutable),
                TotalCount = 1,
                MaxPerNodeCount = 1,
                NodeFilter = new PSNodeRoleBasedNodeFilter()
            });

            proxy = services.GetServiceProxy<ISurveyedSurfaceService>(ServiceName);
        }

        /// <summary>
        /// Invoke proxy for calling the add surveyed surface service
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="designDescriptor"></param>
        /// <param name="asAtDate"></param>
        /// <param name="extents"></param>
        public void Invoke_Add(Guid SiteModelID, DesignDescriptor designDescriptor, DateTime asAtDate, BoundingWorldExtent3D extents)
        {
            proxy.Add(SiteModelID, designDescriptor, asAtDate.Date, extents);
        }

        public bool Invoke_Remove(Guid SiteModelID, Guid SurveyedSurfaceID)
        {
            return proxy.Remove(SiteModelID, SurveyedSurfaceID);
        }

        /// <summary>
        /// Invoke proxy for calling the list surveyed surface service
        /// </summary>
        /// <param name="SiteModelID"></param>
        public SurveyedSurfaces Invoke_List(Guid SiteModelID)
        {
            return proxy.List(SiteModelID);
        }
    }
}
