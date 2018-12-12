using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Events;
using VSS.ConfigurationStore;
using VSS.TRex.Common;
using VSS.TRex.DI;

namespace VSS.TRex.GridFabric.Affinity
{
  /// <summary>
  /// Provides capabilities for determining partition maps to nodes for Ignite caches. It is templated on 
  /// the ket (TK) and value (TV) types of the cache being referenced.
  /// </summary>
  public class AffinityPartitionMap<TK, TV> : IEventListener<CacheRebalancingEvent>
  {
    /// <summary>
    /// Backing variable for PrimaryPartitions
    /// </summary>
    private bool[] primaryPartitions;

    /// <summary>
    /// Provides a map of primary partitions that this node is responsible for
    /// </summary>
    public bool[] PrimaryPartitions
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
    /// The reference to the Ignite cache the partition map relates
    /// </summary>
    private ICache<TK, TV> Cache { get; set; }

    private ICacheAffinity Affinity { get; set; }
    private IClusterNode LocalNode { get; set; }

    protected int NumPartitionsPerDataCache = DIContext.Obtain<IConfigurationStore>().GetValueInt("NUMPARTITIONS_PERDATACACHE", (int) Consts.NUMPARTITIONS_PERDATACACHE);

    /// <summary>
    /// Constructor accepting a cache reference to obtain the partition map information for
    /// </summary>
    /// <param name="cache"></param>
    public AffinityPartitionMap(ICache<TK, TV> cache)
    {
      Cache = cache ?? throw new ArgumentException("Supplied cache cannot be null", nameof(cache));

      Affinity = Cache.Ignite.GetAffinity(Cache.Name);
      LocalNode = Cache.Ignite.GetCluster().GetLocalNode();
    }

    /// <summary>
    /// Asks Ignite for the list of primary partitions this node is responsible for in the provided cache 
    /// </summary>
    /// <returns></returns>
    private bool[] GetPrimaryPartitions()
    {
      bool[] result = new bool[NumPartitionsPerDataCache];

      foreach (int partition in Affinity.GetPrimaryPartitions(LocalNode))
        result[partition] = true;

      return result;
    }

    /// <summary>
    /// Asks Ignite for the list of primary partitions this node is responsible for in the provided cache 
    /// </summary>
    /// <returns></returns>
    private Dictionary<int, bool> GetBackupPartitions() => Affinity.GetBackupPartitions(LocalNode).ToDictionary(k => k, v => true);

    /// <summary>
    /// Determines the Ignite partition index responsible for hold this given key.
    /// This is not so performant as it performant as it involves a full lookup of the cache affinity context from Ignite.
    /// If performance is important, use the AffinityKeyFunction assigned to the cache configuration directly
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public int PartitionFor(TK key) => Cache.Ignite.GetAffinity(Cache.Name).GetPartition(key);

    /// <summary>
    /// Determines if this node hold the primary partition for the given key
    /// This is not so performant as it performant as it involves a full lookup of the cache affinity context from Ignite.
    /// If performance is important, use the PrimaryParitionMap dictionary available from this class.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool HasPrimaryPartitionFor(TK key) => PrimaryPartitions[PartitionFor(key)];

    /// <summary>
    /// Determines if this node holds a backup partition for the given key
    /// This is not so performant as it performant as it involves a full lookup of the cache affinity context from Ignite.
    /// If performance is important, use the BackupParitionMap dictionary availalble from this class.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool HasBackupPartitionFor(TK key) => BackupPartitions[PartitionFor(key)];

    public bool Invoke(CacheRebalancingEvent evt)
    {
      if (evt.CacheName.Equals(Cache.Name)) // && evt.DiscoveryEventType == )
      {
        // Assign primary and backup partition maps to null to force them to be recalculated
        primaryPartitions = null;
        backupPartitions = null;
      }

      return true;
    }
  }
}
