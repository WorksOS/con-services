using VSS.VisionLink.Raptor.Servers;

namespace VSS.VisionLink.Raptor.GridFabric.NodeFilters
{
    /// <summary>
    /// Defines a node filter that filters nodes based on membership of the "PSNode" role
    /// </summary>
    public class PSNodeRoleBasedNodeFilter : RoleBasedServerNodeFilter
    {
        /// <summary>
        /// Default no-arg constructor that instantiate the appropriate role
        /// </summary>
        public PSNodeRoleBasedNodeFilter() : base(ServerRoles.PSNODE)
        {
        }
    }
}
