using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Events;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Common;
using VSS.TRex.DI;

namespace VSS.TRex.GridFabric.Affinity
{
  /// <summary>
  /// Provides capabilities for determining partition maps to nodes for Ignite caches. It is based on
  /// the key (TK) and value (TV) types of the cache being referenced.
  /// </summary>
  public class AffinityPartitionMap<TK, TV> : IEventListener<CacheRebalancingEvent>, IEventListener<DiscoveryEvent>
  {
    /// <summary>
    /// Backing variable for PrimaryPartitions
    /// </summary>
    private bool[] _primaryPartitions;

    /// <summary>
    /// Provides a map of primary partitions that this node is responsible for
    /// </summary>
    public bool[] PrimaryPartitions() => _primaryPartitions ??= GetPrimaryPartitions();

    /// <summary>
    /// Backing variable for BackupPartitions
    /// </summary>
    public Dictionary<int, bool> backupPartitions;

    /// <summary>
    /// Provides a map of backup partitions that this node is responsible for
    /// </summary>
    public Dictionary<int, bool> BackupPartitions => backupPartitions ??= GetBackupPartitions();

    /// <summary>
    /// The reference to the Ignite cache the partition map relates
    /// </summary>
    private ICache<TK, TV> Cache { get; }

    private ICacheAffinity Affinity { get; }
    private IClusterNode LocalNode { get; }

    protected readonly int NumPartitionsPerDataCache = DIContext.Obtain<IConfigurationStore>().GetValueInt("NUMPARTITIONS_PERDATACACHE", Consts.NUMPARTITIONS_PERDATACACHE);

    /// <summary>
    /// Constructor accepting a cache reference to obtain the partition map information for
    /// </summary>
    public AffinityPartitionMap(ICache<TK, TV> cache)
    {
      Cache = cache ?? throw new ArgumentException("Supplied cache cannot be null", nameof(cache));

      Affinity = Cache.Ignite.GetAffinity(Cache.Name);
      LocalNode = Cache.Ignite.GetCluster().GetLocalNode();

      Cache.Ignite.GetEvents().LocalListen<CacheRebalancingEvent>(this,
        EventType.CacheRebalanceStopped,
        EventType.CacheRebalanceStarted);

      Cache.Ignite.GetEvents().LocalListen<DiscoveryEvent>(this,
        EventType.NodeFailed,
        EventType.NodeJoined,
        EventType.NodeLeft,
        EventType.CacheNodesLeft);
    }

    /// <summary>
    /// Asks Ignite for the list of primary partitions this node is responsible for in the provided cache 
    /// </summary>
    private bool[] GetPrimaryPartitions()
    {
      var result = new bool[NumPartitionsPerDataCache];

      foreach (var partition in Affinity.GetPrimaryPartitions(LocalNode))
        result[partition] = true;

      return result;
    }

    /// <summary>
    /// Asks Ignite for the list of primary partitions this node is responsible for in the provided cache 
    /// </summary>
    private Dictionary<int, bool> GetBackupPartitions() => Affinity.GetBackupPartitions(LocalNode).ToDictionary(k => k, v => true);

    /// <summary>
    /// Determines the Ignite partition index responsible for hold this given key.
    /// This does not perform so well as it involves a full lookup of the cache affinity context from Ignite.
    /// If performance is important, use the AffinityKeyFunction assigned to the cache configuration directly
    /// </summary>
    public int PartitionFor(TK key) => Cache.Ignite.GetAffinity(Cache.Name).GetPartition(key);

    /// <summary>
    /// Determines if this node hold the primary partition for the given key
    /// This does not perform so well  as it involves a full lookup of the cache affinity context from Ignite.
    /// If performance is important, use the PrimaryPartitionMap dictionary available from this class.
    /// </summary>
    public bool HasPrimaryPartitionFor(TK key) => PrimaryPartitions()[PartitionFor(key)];

    /// <summary>
    /// Determines if this node holds a backup partition for the given key
    /// This does not perform so well  as it involves a full lookup of the cache affinity context from Ignite.
    /// If performance is important, use the BackupPartitionMap dictionary available from this class.
    /// </summary>
    public bool HasBackupPartitionFor(TK key) => BackupPartitions[PartitionFor(key)];

    public bool Invoke(CacheRebalancingEvent evt)
    {
      if (evt.CacheName.Equals(Cache.Name)))
      {
        // Assign primary and backup partition maps to null to force them to be recalculated
        _primaryPartitions = null;
        backupPartitions = null;
      }

      return true;
    }

    public bool Invoke(DiscoveryEvent evt)
    {
      // Something in the topology has changed. Drop the partition maps to stimulate their re-request on next request
      // Assign primary and backup partition maps to null to force them to be recalculated
      _primaryPartitions = null;
      backupPartitions = null;

      return true;
    }
  }
}
