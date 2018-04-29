using System;
using VSS.VisionLink.Raptor.Servers;

namespace VSS.VisionLink.Raptor.GridFabric.NodeFilters
{
    /// <summary>
    /// Defines a node filter that filters nodes based on membership of the "TAG Processing" role
    /// </summary>
    [Serializable]
    public class TAGProcessorRoleBasedNodeFilter : RoleBasedNodeFilter
    {
        /// <summary>
        /// Default no-arg constructor that instantiate the appropriate role
        /// </summary>
        public TAGProcessorRoleBasedNodeFilter() : base(ServerRoles.TAG_PROCESSING_NODE)
        {
        }
    }
}
