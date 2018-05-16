using System;
using Apache.Ignite.Core.Cluster;
using System.Collections.Generic;
using VSS.TRex.Servers;

namespace VSS.TRex.GridFabric.NodeFilters
{
    /// <summary>
    /// Defines a node filter that filters nodes based on a defined role attribute
    /// </summary>
    [Serializable]
    public abstract class RoleBasedNodeFilter : IClusterNodeFilter
    {
        /// <summary>
        /// The node role
        /// </summary>
        protected string Role { get; set; } = "";

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
        /// Implementation of the filter that is provided with node references to determine if they match the filter
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual bool Invoke(IClusterNode node)
        {
            // No implementation in base class, reject the node
            return node.Attributes.Contains(new KeyValuePair<string, object>($"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{Role}", "True")); 
        }
    }
}
