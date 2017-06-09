using Apache.Ignite.Core.Cache.Affinity;
using Apache.Ignite.Core.Cluster;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.GridFabric.Affinity
{
    /// <summary>
    /// The affinity function used by Raptor to spread spatial data amongst the PSNode processing servers
    /// </summary>
    [Serializable]
    public class RaptorSpatialAffinityFunction : IAffinityFunction
    {
        /// <summary>
        /// Return the number of partitions to use for affinity. For this affinity function, the number of partitions
        /// is governed by the configured number of Raptor spatial processing divisions
        /// </summary>
        public int Partitions
        {
            get
            {
                return (int)RaptorConfig.numSpatialProcessingDivisions;
            }
        }

        public IEnumerable<IEnumerable<IClusterNode>> AssignPartitions(AffinityFunctionContext context)
        {
            // Given the set of nodes in the cluster, determine that there is (at least) <n> nodes marked with
            // the PSNode role. If not, then throw an exception. If there are exactly that many nodes, then assign
            // one node to to each partition (where a partition is a Raptor spatial processing subdivision), based
            // on the order the cluster nodes occur in the provided topology. If there are more than n nodes, then
            // assign them in turn to the partitions as backup nodes.

            List<IClusterNode> PSNodes = context.CurrentTopologySnapshot.Where(x => { string role; return x.TryGetAttribute<string>("Role", out role) && role == "PSNode"; }).ToList();

            if (PSNodes.Count < RaptorConfig.numSpatialProcessingDivisions)
            {
                throw new ArgumentException(String.Format("RaptorSpatialAffinityFunction: Insufficient PS nodes to establish affinity. {0} nodes available with {1} configured spatial subdivisions", PSNodes.Count, RaptorConfig.numSpatialProcessingDivisions));
            }

            // Create the (empty) list of node mappings for the affinity partition assignment
            List<List<IClusterNode>> result = Enumerable.Range(0, (int)RaptorConfig.numSpatialProcessingDivisions - 1).Select(x => new List<IClusterNode>()).ToList();

            // Assign all nodes to affinity partitions. Spare nodes will be mapped as backups
            int index = 0;
            foreach (var node in PSNodes)
            {
                result[index++ % PSNodes.Count].Add(node);
            }

            return result;
        }

        public int GetPartition(object key)
        {
            // Pull the subgrid origin location for the subgrid or segment represented in key and calculate the 
            // spatial processing division descriptor to use as the partition affinity key

            if (key is ISubGrid)
            {
                ISubGrid value = (ISubGrid)key;
                return (int)SubGridCellAddress.ToSpatialDivisionDescriptor(value.OriginX, value.OriginY, RaptorConfig.numSpatialProcessingDivisions);
            }

            throw new ArgumentException(String.Format("Unknown key type to compute spatial affinity partition key for: {0}", key.ToString()));
        }

        public void RemoveNode(Guid nodeId)
        {
            // Don't care at this point, I think...
            // throw new NotImplementedException();
        }
    }
}
