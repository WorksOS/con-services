using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.NodeFilters;

namespace VSS.TRex.SiteModelChangeMaps.GridFabric.NodeFilters
{
    /// <summary>
    /// Defines a node filter that filters nodes based on membership of the "TAG Processing" role
    /// </summary>
    public class SiteModelChangeProcessorRoleBasedNodeFilter : RoleBasedServerNodeFilter
    {
        /// <summary>
        /// Default no-arg constructor that instantiate the appropriate role
        /// </summary>
        public SiteModelChangeProcessorRoleBasedNodeFilter() : base(ServerRoles.TAG_PROCESSING_NODE)
        {
        }
    }
}
