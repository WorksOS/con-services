using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Affinity;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cluster;
using Xunit;

namespace VSS.TRex.Tests.netcore.Affinity
{
    [Serializable]
    public class MutableNonSpatialAffinityFunction : IAffinityFunction
    {
        public int Partitions => 1024;

        public void RemoveNode(Guid nodeId) { }

        public IEnumerable<IEnumerable<IClusterNode>> AssignPartitions(AffinityFunctionContext context)
        {
            List<List<IClusterNode>> result = Enumerable.Range(0, Partitions).Select(x => new List<IClusterNode>()).ToList();
            List<IClusterNode> Nodes = context.CurrentTopologySnapshot.ToList();

            if (Nodes.Count > 0)
            {
                for (int partitionIndex = 0; partitionIndex < Partitions; partitionIndex++)
                    result[partitionIndex].Add(Nodes[Partitions % Nodes.Count]);
            }

            return result;
        }

        public int GetPartition(object key) => Math.Abs(((NonSpatialAffinityKey)key).ProjectID.GetHashCode()) % Partitions;
    }

    [Serializable]
    public struct NonSpatialAffinityKey
    {
        public Guid ProjectID { get; set; }

        public string KeyName { get; set; }

        public NonSpatialAffinityKey(Guid projectID, string keyName)
        {
            ProjectID = projectID;
            KeyName = keyName;
        }

        public override string ToString() => $"{ProjectID}-{KeyName}";
    }

    public class AffinityTests
    {
        private static IIgnite ignite;

        private static void ConfigureGrid(IgniteConfiguration cfg)
        {
            cfg.IgniteInstanceName = "MyGrid3";

            //cfg.BinaryConfiguration = new BinaryConfiguration(typeof(Guid));
        }

        private static void EnsureServer()
        {
            IgniteConfiguration cfg = new IgniteConfiguration();
            ConfigureGrid(cfg);
            ignite = Ignition.Start(cfg);
        }

        [Fact(Skip = "Requires live Ignite node")]
        public void Test_NonSpatialAffintyKey_MutableGrid_IgniteException()
        {
            EnsureServer();

            ICache<NonSpatialAffinityKey, byte[]> cache = ignite.GetOrCreateCache<NonSpatialAffinityKey, byte[]>(
                new CacheConfiguration
                {
                    Name = "MyCache",
                    CacheMode = CacheMode.Partitioned,
                    AffinityFunction = new MutableNonSpatialAffinityFunction(),
                });

            NonSpatialAffinityKey key1 = new NonSpatialAffinityKey {ProjectID = Guid.NewGuid(), KeyName = "bob1"};
            NonSpatialAffinityKey key2 = new NonSpatialAffinityKey {ProjectID = Guid.NewGuid(), KeyName = "bob2"};

            cache.Put(key1, new byte[100]);
            cache.Put(key2, new byte[100]);
/*            cache.PutAll(new List<KeyValuePair<NonSpatialAffinityKey, byte[]>>
            {
                new KeyValuePair<NonSpatialAffinityKey, byte[]>(key1, new byte[100]),
                new KeyValuePair<NonSpatialAffinityKey, byte[]>(key2, new byte[100])
            });*/

            Assert.True(true);
        }
    }
}
