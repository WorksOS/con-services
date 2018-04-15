using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core.Cache;

namespace VSS.TRex.GridFabric.Affinity
{
    /// <summary>
    /// Provides capabilities for determining partition maps to nodes for Ignite caches. It is templated on 
    /// the ket (TK) and value (TV) types of the cache being referenced.
    /// </summary>
    public class SpatialAffinityPartitionMap<TK, TV>
    {
        /// <summary>
        /// Backing variable for PrimaryPartitions
        /// </summary>
        public Dictionary<int, bool> primaryPartitions;

        /// <summary>
        /// Provides a map of primary partitions that this node is responsible for
        /// </summary>
        public Dictionary<int, bool> PrimaryPartitions
        {
            get => primaryPartitions ?? (primaryPartitions = GetPrimaryPartitions());
        }

        /// <summary>
        /// Backing variable for BackupPartitions
        /// </summary>
        public Dictionary<int, bool> backupPartitions;

        /// <summary>
        /// Provides a map of backup partitions that this node is responsible for
        /// </summary>
        public Dictionary<int, bool> BackupPartitions
        {
            get => backupPartitions ?? (backupPartitions = GetBackupPartitions());
        }

        /// <summary>
        /// The reference to the Ignite cache the parittion map relates
        /// </summary>
        private ICache<TK, TV> Cache { get; set; }

        /// <summary>
        /// Constructor accepting a cache reference to obtain the parition map information for
        /// </summary>
        /// <param name="cache"></param>
        public SpatialAffinityPartitionMap(ICache<TK, TV> cache)
        {
            Cache = cache ?? throw new ArgumentException("Supplied cache cannot be null", "cache");
        }

        /// <summary>
        /// Asks Ignite for the list of primary partitions this node is reponsible for in the provided cache 
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, bool> GetPrimaryPartitions()
        {
            return Cache.Ignite.GetAffinity(Cache.Name).GetPrimaryPartitions(Cache.Ignite.GetCluster().GetLocalNode())
                .ToDictionary(k => k, v => true);
        }

        /// <summary>
        /// Asks Ignite for the list of primary partitions this node is reponsible for in the provided cache 
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, bool> GetBackupPartitions()
        {
            return Cache.Ignite.GetAffinity(Cache.Name).GetBackupPartitions(Cache.Ignite.GetCluster().GetLocalNode())
                .ToDictionary(k => k, v => true);
        }

        /// <summary>
        /// Determines the Ignite partition index responsible for hold this given key.
        /// This is not so performant as it performant as it involves a full lookup of the cache affinity context from Ignite.
        /// If performance is important, use the AffinityKeyFunction assigned to the cache configuration directly
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int PartitionFor(TK key) => Cache.Ignite.GetAffinity(Cache.Name).GetPartition(key);

        /// <summary>
        /// Determines if this node hold the primary parition for the given key
        /// This is not so performant as it performant as it involves a full lookup of the cache affinity context from Ignite.
        /// If performance is important, use the PrimaryParitionMap ditionary availalble from this class.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasPrimaryPartitionFor(TK key) => PrimaryPartitions[PartitionFor(key)];

        /// <summary>
        /// Determines if this node holds a backup parition for the given key
        /// This is not so performant as it performant as it involves a full lookup of the cache affinity context from Ignite.
        /// If performance is important, use the BackupParitionMap ditionary availalble from this class.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasBackupPartitionFor(TK key) => BackupPartitions[PartitionFor(key)];
    }
}
