using Apache.Ignite.Core.Cache.Affinity;
using Apache.Ignite.Core.Cluster;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Servers;
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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

        /// <summary>
        /// Determine how the PSNodes in the Raptor grid are to be assigned into the spatial divisions configured in the system
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<IEnumerable<IClusterNode>> AssignPartitions(AffinityFunctionContext context)
        {
            // Create the (empty) list of node mappings for the affinity partition assignment
            List<List<IClusterNode>> result = Enumerable.Range(0, (int)RaptorConfig.numSpatialProcessingDivisions).Select(x => new List<IClusterNode>()).ToList();

            try
            {
                // Given the set of nodes in the cluster, determine that there is (at least) <n> nodes marked with
                // the PSNode role. If not, then throw an exception. If there are exactly that many nodes, then assign
                // one node to to each partition (where a partition is a Raptor spatial processing subdivision), based
                // on the order the cluster nodes occur in the provided topology. If there are more than n nodes, then
                // assign them in turn to the partitions as backup nodes.

                Log.InfoFormat("RaptorSpatialAffinityFunction: Assigning partitions from topology");

                List<IClusterNode> PSNodes = context.CurrentTopologySnapshot.Where(x => { string role; return x.TryGetAttribute<string>(ServerRoles.ROLE_ATTRIBUTE_NAME, out role) && role == ServerRoles.PSNODE; }).ToList();

                if (PSNodes.Count < RaptorConfig.numSpatialProcessingDivisions)
                {
                    Log.InfoFormat("RaptorSpatialAffinityFunction: Insufficient PS nodes to establish affinity. {0} nodes available with {1} configured spatial subdivisions, will return partial affinity map.", PSNodes.Count, RaptorConfig.numSpatialProcessingDivisions);                 
//                    return result;  // Return the empty list - no partitioning possible
                }

                // Assign all nodes to affinity partitions. Spare nodes will be mapped as backups. 

                Log.Info("Assigning Raptor spatial partitions");
                for (int divisionIndex = 0; divisionIndex < RaptorConfig.numSpatialProcessingDivisions; divisionIndex++)
                {
                    List<IClusterNode> spatialDivisionNodes = PSNodes.Where(x => { int division; return x.TryGetAttribute<int>("SpatialDivision", out division) && division == divisionIndex; }).ToList();

                    foreach (IClusterNode node in spatialDivisionNodes)
                    {
                        Log.InfoFormat("Assigned node {0} to division {1}", node.Id, divisionIndex);
                        result[divisionIndex].Add(node);
                    }

                    Log.InfoFormat("--> Assigned {0} nodes (out of {1}) to spatial division {2}", spatialDivisionNodes.Count, PSNodes.Count, divisionIndex);
                }
            }
            catch (Exception e)
            {
                Log.ErrorFormat("RaptorSpatialAffinityFunction: Exception: {0}", e);
                return new List<List<IClusterNode>>();
            }

            return result;
        }

        /// <summary>
        /// Given a cache key, determine which partition the cache item should reside
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetPartition(object key)
        {
            // Pull the subgrid origin location for the subgrid or segment represented in the cache key and calculate the 
            // spatial processing division descriptor to use as the partition affinity key

            if (!(key is SubGridSpatialAffinityKey ))
            {
                Log.InfoFormat("Unknown key type to compute spatial affinity partition key for: {0}", key.ToString());
                throw new ArgumentException(String.Format("Unknown key type to compute spatial affinity partition key for: {0}", key.ToString()));
            }

            SubGridSpatialAffinityKey value = (SubGridSpatialAffinityKey)key;

            return (int)SubGridCellAddress.ToSpatialDivisionDescriptor(value.SubGridX, value.SubGridY, RaptorConfig.numSpatialProcessingDivisions);
        }

        /// <summary>
        /// Remove a node from the topology. There is no special logic required here; the AssignPartitions method should be called again
        /// to reassign the remaining nodes into the spatial partitions
        /// </summary>
        /// <param name="nodeId"></param>
        public void RemoveNode(Guid nodeId)
        {
            Log.InfoFormat("RaptorSpatialAffinityFunction: Removing node {0}", nodeId);
            // Don't care at this point, I think...
            // throw new NotImplementedException();
        }
    }
}
