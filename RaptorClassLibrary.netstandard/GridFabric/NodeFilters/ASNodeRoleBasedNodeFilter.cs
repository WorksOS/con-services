using VSS.TRex.Servers;

namespace VSS.TRex.GridFabric.NodeFilters
{
    /// <summary>
    /// Defines a node filter that filters nodes based on membership of the "PSNode" role
    /// </summary>
    public class ASNodeRoleBasedNodeFilter : RoleBasedClientNodeFilter
    {
        /// <summary>
        /// Default no-arg constructor that instantiate the appropriate role
        /// </summary>
        public ASNodeRoleBasedNodeFilter() : base(ServerRoles.ASNODE)
        {
        }
    }
}
