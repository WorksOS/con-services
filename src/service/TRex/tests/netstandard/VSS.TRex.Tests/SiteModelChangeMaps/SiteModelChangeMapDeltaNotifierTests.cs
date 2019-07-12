using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModelChangeMaps;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
using VSS.TRex.SiteModelChangeMaps.Interfaces;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModelChangeMaps
{
  public class SiteModelChangeMapDeltaNotifierTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddChangeMapQueueCacheToDI()
    {
      var nonTransactedProxy = new StorageProxyCacheTransacted_TestHarness<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>(DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Immutable)?.GetOrCreateCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>(TRexCaches.SiteModelChangeBufferQueueCacheName()), new SiteModelChangeBufferQueueKeyEqualityComparer());
      var transactedProxy = new StorageProxyCacheTransacted_TestHarness<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>(DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Immutable)?.GetOrCreateCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>(TRexCaches.SiteModelChangeBufferQueueCacheName()), new SiteModelChangeBufferQueueKeyEqualityComparer());

      DIBuilder
        .Continue()

        // Add the factories for the storage proxy caches, both standard and transacted, for spatial and non spatial caches in TRex

        ////////////////////////////////////////////////////
        // Injected standard storage proxy cache 
        ////////////////////////////////////////////////////

        // Add the singleton reference to the non-transacted site model change map cache
        .Add(x => x.AddSingleton<IStorageProxyCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>(nonTransactedProxy))

        /////////////////////////////////////////////////////
        // Injected transacted storage proxy cache factories
        /////////////////////////////////////////////////////

        // Add the singleton reference to the transacted site model change map cache
        .Add(x => x.AddSingleton<IStorageProxyCacheTransacted<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>(transactedProxy))

        .Build()
        .Add(x => x.AddSingleton<ISiteModelChangeMapDeltaNotifier>(new SiteModelChangeMapDeltaNotifier()))

        .Build();
    }

    public SiteModelChangeMapDeltaNotifierTests()
    {
      AddChangeMapQueueCacheToDI();
    }

    [Fact]
    public void Creation()
    {
      var notifier = new SiteModelChangeMapDeltaNotifier();
      notifier.Should().NotBeNull();
    }

    [Theory]
    [InlineData(SiteModelChangeMapOrigin.Ingest, SiteModelChangeMapOperation.AddSpatialChanges)]
    [InlineData(SiteModelChangeMapOrigin.Ingest, SiteModelChangeMapOperation.RemoveSpatialChanges)]
    [InlineData(SiteModelChangeMapOrigin.Query, SiteModelChangeMapOperation.AddSpatialChanges)]
    [InlineData(SiteModelChangeMapOrigin.Query, SiteModelChangeMapOperation.RemoveSpatialChanges)]
    public void Notify_EmptyChangeMap(SiteModelChangeMapOrigin origin, SiteModelChangeMapOperation operation)
    {
      var notifier = new SiteModelChangeMapDeltaNotifier();

      // The notifier uses the non-transacted storage proxy:
      var proxy = DIContext.Obtain<IStorageProxyCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>();
      proxy.Should().NotBeNull();
      proxy.Clear();

      var projectUid = Guid.NewGuid();
      var insertUTC = DateTime.UtcNow;

      // Ask the notifier to notify a new item
      notifier.Notify(projectUid, insertUTC, new SubGridTreeSubGridExistenceBitMask(), origin, operation);

      // Check the new item was placed into the cache
      var cachedItem = proxy.Get(new SiteModelChangeBufferQueueKey(projectUid, insertUTC));
      cachedItem.Should().NotBeNull();
      cachedItem.Operation.Should().Be(operation);
      cachedItem.Origin.Should().Be(origin);

      var readMap = new SubGridTreeSubGridExistenceBitMask();
      readMap.FromBytes(cachedItem.Content);

      readMap.CountBits().Should().Be(0);
    }

    [Fact]
    public void Notify_TAGFileDerivedChangeMap_DirectNotify()
    {
      // Build a site model from a TAG file and verify there is a change map written to the queue that matches the existence map
      // for the newly created model
      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var notifier = new SiteModelChangeMapDeltaNotifier();

      // The notifier uses the non-transacted storage proxy:
      var proxy = DIContext.Obtain<IStorageProxyCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>();
      proxy.Should().NotBeNull();
      proxy.Clear();

      var insertUTC = DateTime.UtcNow;

      // Ask the notifier to notify a new item
      notifier.Notify(siteModel.ID, insertUTC, siteModel.ExistenceMap, SiteModelChangeMapOrigin.Ingest, SiteModelChangeMapOperation.AddSpatialChanges);

      // Check the new item was placed into the cache
      var cachedItem = proxy.Get(new SiteModelChangeBufferQueueKey(siteModel.ID, insertUTC));
      cachedItem.Should().NotBeNull();
      cachedItem.ProjectUID.Should().Be(siteModel.ID);
      cachedItem.Operation.Should().Be(SiteModelChangeMapOperation.AddSpatialChanges);
      cachedItem.Origin.Should().Be(SiteModelChangeMapOrigin.Ingest);

      var readMap = new SubGridTreeSubGridExistenceBitMask();
      readMap.FromBytes(cachedItem.Content);

      readMap.CountBits().Should().Be(12);
      readMap.CountBits().Should().Be(siteModel.ExistenceMap.CountBits());
      readMap.ScanAllSetBitsAsSubGridAddresses(x => siteModel.ExistenceMap[x.X >> SubGridTreeConsts.SubGridIndexBitsPerLevel, x.Y >> SubGridTreeConsts.SubGridIndexBitsPerLevel].Should().BeTrue());
    }

    [Fact]
    public void Notify_TAGFileDerivedChangeMap_SiteModelsMediatedNotification()
    {
      // Build a site model from a TAG file and verify there is a change map written to the queue that matches the existence map
      // for the newly created model
      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      // The notifier uses the non-transacted storage proxy:
      var proxy = DIContext.Obtain<IStorageProxyCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>();
      proxy.Should().NotBeNull();
 
      // Check the new item was placed into the cache
      var testProxy = proxy as IStorageProxyCacheTransacted_TestHarness<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>;
      testProxy.GetPendingTransactedWrites().Count.Should().Be(1);
      var cachedItem = testProxy.GetPendingTransactedWrites().Values.First(); 

      cachedItem.Should().NotBeNull();
      cachedItem.ProjectUID.Should().Be(siteModel.ID);
      cachedItem.Operation.Should().Be(SiteModelChangeMapOperation.AddSpatialChanges);
      cachedItem.Origin.Should().Be(SiteModelChangeMapOrigin.Ingest);

      var readMap = new SubGridTreeSubGridExistenceBitMask();
      readMap.FromBytes(cachedItem.Content);

      readMap.CountBits().Should().Be(12);
      readMap.CountBits().Should().Be(siteModel.ExistenceMap.CountBits());
      readMap.ScanAllSetBitsAsSubGridAddresses(x => siteModel.ExistenceMap[x.X >> SubGridTreeConsts.SubGridIndexBitsPerLevel, x.Y >> SubGridTreeConsts.SubGridIndexBitsPerLevel].Should().BeTrue());
    }
  }
}
