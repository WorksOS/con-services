using Apache.Ignite.Core.Cache.Affinity;
using Apache.Ignite.Core.Cluster;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VSS.TRex.GridFabric.Affinity
{
    /// <summary>
    /// The affinity function used by TRex to spread data amongst processing servers
    /// </summary>
    [Serializable]
    public class AffinityFunctionBase : IAffinityFunction
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // Set NumPartitions to the default number of partitions
        protected int NumPartitions = (int)TRexConfig.NumPartitionsPerDataCache;

        /// <summary>
        /// Return the number of partitions to use for affinity. 
        /// </summary>
        public int Partitions => NumPartitions; 

        /// <summary>
        /// Determine how the nodes in the grid are to be assigned into the partitions configured in the cache
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<IEnumerable<IClusterNode>> AssignPartitions(AffinityFunctionContext context)
        {
            // Create the (empty) list of node mappings for the affinity partition assignment
            List<List<IClusterNode>> result = Enumerable.Range(0, NumPartitions).Select(x => new List<IClusterNode>()).ToList();

            try
            {
                Log.Info("Assigning partitions");

                /* Debug code to dumo the attributes assigned to nodes being looked at
                foreach (var node in context.CurrentTopologySnapshot)
                {
                    Log.Info($"Topology Node {node.Id}:");
                    foreach (KeyValuePair<string, object> pair in node.GetAttributes())
                        Log.Info($"Attributes Pair: {pair.Key} -> {pair.Value}");
                } */

                List<IClusterNode> Nodes = context.CurrentTopologySnapshot.ToList();

                // Assign all nodes to affinity partitions. Spare nodes will be mapped as backups. 
                if (Nodes.Count > 0)
                {
                    /* Debug code to dumo the attributes assigned to nodes being looked at
                    foreach (var a in Nodes.First().GetAttributes())
                        Log.Info($"Attribute: {a.ToString()}");
                    */

                    Log.Info("Assigning partitions to nodes");
                    for (int partitionIndex = 0; partitionIndex < NumPartitions; partitionIndex++)
                    {
                        result[partitionIndex].Add(Nodes[NumPartitions % Nodes.Count]);

                        Log.Debug($"--> Assigned node:{Nodes[NumPartitions % Nodes.Count].ConsistentId} nodes to partition {partitionIndex}");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Exception: {e}");
                return new List<List<IClusterNode>>();
            }

            return result;
        }

        /// <summary>
        /// Given a cache key, determine which partition the cache item should reside
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual int GetPartition(object key)
        {
            // No-op in base class
            return 0;
        }

        /// <summary>
        /// Remove a node from the topology. There is no special logic required here; the AssignPartitions method should be called again
        /// to reassign the remaining nodes into the partitions
        /// </summary>
        /// <param name="nodeId"></param>
        public void RemoveNode(Guid nodeId)
        {
            Log.Info($" Removing node {nodeId}");
            // Don't care at this point, I think...
            // throw new NotImplementedException();
        }
    }
}
