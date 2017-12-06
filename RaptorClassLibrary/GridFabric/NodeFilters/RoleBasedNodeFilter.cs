using Apache.Ignite.Core.Cluster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Servers;

namespace VSS.VisionLink.Raptor.GridFabric.NodeFilters
{
    /// <summary>
    /// Defines a node filter that filters nodes based on a defined role attribute
    /// </summary>
    public class RoleBasedNodeFilter : IClusterNodeFilter
    {
        /// <summary>
        /// The node role
        /// </summary>
        private string Role { get; set; } = "";

        /// <summary>
        ///  Default no-arg constructor
        /// </summary>
        public RoleBasedNodeFilter()
        {
        }

        /// <summary>
        /// Constructor accepting the name of the role to filter nodes with
        /// </summary>
        /// <param name="role"></param>
        public RoleBasedNodeFilter(string role) : this()
        {
            Role = role;
        }

        /// <summary>
        /// Implenentation of the filter that is provided with node references to determine if they match the filter
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool Invoke(IClusterNode node)
        {
            return !node.IsClient && node.GetAttributes().Contains(new KeyValuePair<string, object>(ServerRoles.ROLE_ATTRIBUTE_NAME, Role));
        }
    }
}
