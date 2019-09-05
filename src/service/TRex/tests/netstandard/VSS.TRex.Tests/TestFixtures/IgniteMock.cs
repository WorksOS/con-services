using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Messaging;
using Apache.Ignite.Core.Transactions;
using Moq;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Serialisation;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;
using VSS.TRex.SiteModels.GridFabric.Events;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.SubGrids.GridFabric.Listeners;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.Tests.BinarizableSerialization;

namespace VSS.TRex.Tests.TestFixtures
{
  /// <summary>
  /// Defines a collection of Mock objects that collectively mock and re-plumb the Ignite infrastructure layer into a form suitable for unit/integration testing
  /// with frameworks like XUnit and NUnit with the TRex business logic not being aware it is not running on an actual Ignite grid.
  /// </summary>
  public class IgniteMock
  {
    public Mock<ICompute> mockCompute { get; }
    public Mock<IClusterNode> mockClusterNode { get; }
    public Mock<ICollection<IClusterNode>> mockClusterNodes { get; }
    public Mock<IMessaging> mockMessaging { get; }
    public Mock<IClusterGroup> mockClusterGroup { get; }
    public Mock<ICluster> mockCluster { get; }
    public Mock<IIgnite> mockIgnite { get; }
    public Mock<ITransactions> mockTransactions { get; }
    public Mock<ITransaction> mockTransaction { get; }

    /// <summary>
    /// Constructor that creates the collection of mocks that together mock the Ignite infrastructure layer in TRex
    /// </summary>
    public IgniteMock()
    {
      // Wire up Ignite Compute Apply and Broadcast apis on the Compute interface
      mockCompute = new Mock<ICompute>(MockBehavior.Strict);

      // Pretend there is a single node in the cluster group
      mockClusterNode = new Mock<IClusterNode>(MockBehavior.Strict);
      mockClusterNode.Setup(x => x.GetAttribute<string>("TRexNodeId")).Returns("UnitTest-TRexNodeId");

      mockClusterNodes = new Mock<ICollection<IClusterNode>>(MockBehavior.Strict);
      mockClusterNodes.Setup(x => x.Count).Returns(1);

      // Set up the Ignite message fabric mocks to plumb sender and receiver together
      var messagingDictionary = new Dictionary<object, object>(); // topic => listener

      mockMessaging = new Mock<IMessaging>(MockBehavior.Strict);
      mockMessaging
        .Setup(x => x.LocalListen(It.IsAny<IMessageListener<ISerialisedByteArrayWrapper>>(), It.IsAny<object>()))
        .Callback((IMessageListener<ISerialisedByteArrayWrapper> listener, object topic) =>
        {
          messagingDictionary.Add(topic, listener);
        });

      mockMessaging.Setup(x => x.StopLocalListen(It.IsAny<IMessageListener<ISerialisedByteArrayWrapper>>(), It.IsAny<object>()));
     
      mockMessaging
        .Setup(x => x.LocalListen(It.IsAny<IMessageListener<ISiteModelAttributesChangedEvent>>(), It.IsAny<object>()))
        .Callback((IMessageListener<ISiteModelAttributesChangedEvent> listener, object topic) =>
        {
          messagingDictionary.Add(topic, listener);
        });

      mockMessaging.Setup(x => x.StopLocalListen(It.IsAny<IMessageListener<ISiteModelAttributesChangedEvent>>(), It.IsAny<object>()));

      mockMessaging
        .Setup(x => x.Send(It.IsAny<object>(), It.IsAny<object>()))
        .Callback((object message, object topic) =>
        {
          messagingDictionary.TryGetValue(topic, out var listener);
          if (listener is SubGridListener _listener)
            _listener.Invoke(Guid.Empty, message as SerialisedByteArrayWrapper);
          else
            throw new TRexException($"Type of listener ({listener}) not SubGridListener as expected.");
        });

      mockMessaging
        .Setup(x => x.SendOrdered(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<TimeSpan?>()))
        .Callback((object message, object topic, TimeSpan? timeSpan) =>
        {
          messagingDictionary.TryGetValue(topic, out var listener);
          if (listener is SubGridListener _listener1)
            _listener1.Invoke(Guid.Empty, message as SerialisedByteArrayWrapper);
          else if (listener is SiteModelAttributesChangedEventListener _listener2)
            _listener2.Invoke(Guid.Empty, message as SiteModelAttributesChangedEvent);
          else
            throw new TRexException("Type of listener not SubGridListener or SiteModelAttributesChangedEventListener as expected.");
        });

      mockClusterGroup = new Mock<IClusterGroup>(MockBehavior.Strict);
      mockClusterGroup.Setup(x => x.GetNodes()).Returns(mockClusterNodes.Object);
      mockClusterGroup.Setup(x => x.GetCompute()).Returns(mockCompute.Object);
      mockClusterGroup.Setup(x => x.GetMessaging()).Returns(mockMessaging.Object);

      mockCompute.Setup(x => x.ClusterGroup).Returns(mockClusterGroup.Object);

      mockCluster = new Mock<ICluster>(MockBehavior.Strict);
      mockCluster.Setup(x => x.ForAttribute(It.IsAny<string>(), It.IsAny<string>())).Returns(mockClusterGroup.Object);
      mockCluster.Setup(x => x.GetLocalNode()).Returns(mockClusterNode.Object);
      mockCluster.Setup(x => x.GetMessaging()).Returns(mockMessaging.Object);

      var clusterActiveState = true;
      mockCluster.Setup(x => x.IsActive()).Returns(() => clusterActiveState);
      mockCluster.Setup(x => x.SetActive(It.IsAny<bool>())).Callback((bool state) => { /* Never change state from true... clusterActiveState = state; */ });

      mockTransaction = new Mock<ITransaction>(MockBehavior.Strict);
      mockTransactions = new Mock<ITransactions>(MockBehavior.Strict);
      mockTransactions.Setup(x => x.TxStart()).Returns(mockTransaction.Object);

      mockIgnite = new Mock<IIgnite>(MockBehavior.Strict);
      mockIgnite.Setup(x => x.GetCluster()).Returns(mockCluster.Object);
      mockIgnite.Setup(x => x.GetMessaging()).Returns(mockMessaging.Object);
      mockIgnite.Setup(x => x.Name).Returns(TRexGrids.ImmutableGridName);
      mockIgnite.Setup(x => x.GetTransactions()).Returns(mockTransactions.Object);
    }

    private static ICache<TK, TV> BuildMockForCache<TK, TV>(string cacheName)
    {
      if (cacheDictionary.TryGetValue(cacheName, out var cache))
        return (ICache<TK, TV>)cache;

      var mockCache = new Mock<ICache<TK, TV>>(MockBehavior.Strict);
      var mockCacheDictionary = new Dictionary<TK, TV>();

      mockCache.Setup(x => x.Get(It.IsAny<TK>())).Returns((TK key) =>
      {
        if (mockCacheDictionary.TryGetValue(key, out var value))
          return value;
        throw new KeyNotFoundException($"Key {key} not found in mock cache");
      });

      mockCache.Setup(x => x.Put(It.IsAny<TK>(), It.IsAny<TV>())).Callback((TK key, TV value) =>
      {
        mockCacheDictionary.Add(key, value);
      });

      mockCache.Setup(x => x.PutIfAbsent(It.IsAny<TK>(), It.IsAny<TV>())).Returns((TK key, TV value) =>
      {
        if (!mockCacheDictionary.ContainsKey(key))
        {
          mockCacheDictionary.Add(key, value);
          return true;
        }

        return false;
      });

      cacheDictionary.Add(cacheName, mockCache.Object);
      return mockCache.Object;
    }

    public static void AddMockedCacheToIgniteMock<TK, TV>()
    {
      var mockIgnite = DIContext.Obtain<Mock<IIgnite>>();

      mockIgnite
        .Setup(x => x.GetOrCreateCache<TK, TV>(It.IsAny<CacheConfiguration>()))
        .Returns((CacheConfiguration cfg) => BuildMockForCache<TK, TV>(cfg.Name));
      mockIgnite
        .Setup(x => x.GetCache<TK, TV>(It.IsAny<string>()))
        .Returns((string cacheName) => BuildMockForCache<TK, TV>(cacheName));
    }

    public static void RemoveMockedCacheFromIgniteMock<TK, TV>()
    {
      var mockIgnite = DIContext.Obtain<Mock<IIgnite>>();

      mockIgnite.Setup(x => x.GetOrCreateCache<TK, TV>(It.IsAny<CacheConfiguration>()));
      mockIgnite.Setup(x => x.GetCache<TK, TV>(It.IsAny<string>()));
    }

    private static Dictionary<string, object> cacheDictionary; // object = ICache<TK, TV>

    /// <summary>
    /// Removes and recreates any dynamic content contained in the Ignite mock. References to the mocked Ignite context are accessed via the TRex
    /// Dependency Injection layer.
    /// </summary>
    public static void ResetDynamicMockedIgniteContent()
    {
      // Create the dictionary to contain all the mocked caches
      cacheDictionary = new Dictionary<string, object>(); 

      // Create the mocked cache for the existence maps cache and any other cache using this signature
      AddMockedCacheToIgniteMock<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>();

      // Create the mocked cache for the site model change map queue cache and any other cache using this signature
      AddMockedCacheToIgniteMock<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>();

      // Create the mocked cache for the site model TAG file buffer queue cache and any other cache using this signature
      AddMockedCacheToIgniteMock<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>();

      // Create the mocked cache for the site model segment retirement queue and any other cache using this signature
      AddMockedCacheToIgniteMock<ISegmentRetirementQueueKey, SegmentRetirementQueueItem>();

      // Create the mocked cache for the subgrid spatial data in the site model and any other cache using this signature
      AddMockedCacheToIgniteMock<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>();

      // Create the mocked cache for the site model machine change maps and any other cache using this signature
      AddMockedCacheToIgniteMock<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper>();
    }

    private static void TestIBinarizableSerializationForItem(object item)
    {
      if (item is IBinarizable)
      {
        // exercise serialize/deserialize of func and argument before invoking function
        var serializer = new BinarizableSerializer();

        var writer = new TestBinaryWriter();
        serializer.WriteBinary(item, writer);

        var newInstance = Activator.CreateInstance(item.GetType());

        serializer.ReadBinary(newInstance, new TestBinaryReader(writer._stream.BaseStream as MemoryStream));
      }
    }

    public static void AddApplicationGridRouting<TCompute, TArgument, TResponse>() where TCompute : IComputeFunc<TArgument, TResponse>
    {
      var mockCompute = DIContext.Obtain<Mock<ICompute>>();

      mockCompute.Setup(x => x.Apply(It.IsAny<TCompute>(), It.IsAny<TArgument>())).Returns((TCompute func, TArgument argument) =>
      {
        // exercise serialize/deserialize of func and argument before invoking function
        TestIBinarizableSerializationForItem(func);
        TestIBinarizableSerializationForItem(argument);
        var response = func.Invoke(argument);

        // exercise serialie/deserialise of response returning it
        TestIBinarizableSerializationForItem(response);
        return response;
      });

      mockCompute.Setup(x => x.ApplyAsync(It.IsAny<TCompute>(), It.IsAny<TArgument>())).Returns((TCompute func, TArgument argument) =>
      {
        // exercise serialize/deserialize of func and argument before invoking function
        TestIBinarizableSerializationForItem(func);
        TestIBinarizableSerializationForItem(argument);

        var task = new Task<TResponse>(() =>
        {
          var response = func.Invoke(argument);
          TestIBinarizableSerializationForItem(response);

          return response;
        });
        task.Start();

        return task;
      });

      // Mock out the use of the cancellation token by calling the ordinary non-cancellable mock
      mockCompute.Setup(x => x.ApplyAsync(It.IsAny<TCompute>(), It.IsAny<TArgument>(), It.IsAny<CancellationToken>())).Returns((TCompute func, TArgument argument, CancellationToken token) => mockCompute.Object.ApplyAsync(func, argument));
    }

    public static void AddClusterComputeGridRouting<TCompute, TArgument, TResponse>() where TCompute : IComputeFunc<TArgument, TResponse>
    {
      var mockCompute = DIContext.Obtain<Mock<ICompute>>();
      mockCompute.Setup(x => x.Broadcast(It.IsAny<TCompute>(), It.IsAny<TArgument>())).Returns((TCompute func, TArgument argument) =>
      {
        // exercise serialize/deserialize of func and argument before invoking function
        TestIBinarizableSerializationForItem(func);
        TestIBinarizableSerializationForItem(argument);

        var response = new List<TResponse> { func.Invoke(argument) };

        if (response.Count == 1 && response[0] != null)
          TestIBinarizableSerializationForItem(response[0]);

        return response;
      });

      mockCompute.Setup(x => x.BroadcastAsync(It.IsAny<TCompute>(), It.IsAny<TArgument>())).Returns((TCompute func, TArgument argument) =>
      {
        // exercise serialize/deserialize of func and argument before invoking function
        TestIBinarizableSerializationForItem(func);
        TestIBinarizableSerializationForItem(argument);

        var task = new Task<ICollection<TResponse>>(() =>
        {
          var response = func.Invoke(argument);
          TestIBinarizableSerializationForItem(response);

          return new List<TResponse> { response };
        });

        task.Start();
        return task;
      });
    }

    public static void AddClusterComputeSpatialAffinityGridRouting<TCompute, TArgument, TResponse>() where TCompute : IComputeFunc<TResponse>, IComputeFuncArgument<TArgument>
    {
      var mockCompute = DIContext.Obtain<Mock<ICompute>>();

      mockCompute.Setup(x => x.AffinityCall(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TCompute>())).Returns((string cacheName, object key, TCompute func) =>
      {
        // exercise serialize/deserialize of func and argument before invoking function
        TestIBinarizableSerializationForItem(func);
        TestIBinarizableSerializationForItem(key);

        var response = func.Invoke();

        TestIBinarizableSerializationForItem(response);

        return response;
      });

      mockCompute.Setup(x => x.AffinityCallAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TCompute>())).Returns((string cacheName, object key, TCompute func) =>
      {
        // exercise serialize/deserialize of func and argument before invoking function
        TestIBinarizableSerializationForItem(func);
        TestIBinarizableSerializationForItem(key);

        var response = func.Invoke();

        TestIBinarizableSerializationForItem(response);

        return Task.FromResult(response);
      });
    }
  }
}
