using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.TRex.DI;
using VSS.TRex.SiteModelChangeMaps;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
using VSS.TRex.SiteModelChangeMaps.Interfaces;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModelChangeMaps
{
  public class SiteModelChangeMapDeltaNotifierTests : SiteModelChangeTestsBase
  {
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
      var proxy = DIContext.Obtain<Func<IStorageProxyCache<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>>>()();
      proxy.Should().NotBeNull();
      proxy.Clear();

      var projectUid = Guid.NewGuid();
      var insertUtc = DateTime.UtcNow;

      // Ask the notifier to notify a new item
      notifier.Notify(projectUid, insertUtc, new SubGridTreeSubGridExistenceBitMask(), origin, operation);

      // Check the new item was placed into the cache
      var cachedItem = proxy.Get(new SiteModelChangeBufferQueueKey(projectUid, insertUtc));
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
      var proxy = DIContext.Obtain<Func<IStorageProxyCache<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>>>()();
      proxy.Should().NotBeNull();
      proxy.Clear();

      var insertUtc = DateTime.UtcNow;

      // Ask the notifier to notify a new item
      notifier.Notify(siteModel.ID, insertUtc, siteModel.ExistenceMap, SiteModelChangeMapOrigin.Ingest, SiteModelChangeMapOperation.AddSpatialChanges);

      // Check the new item was placed into the cache
      var cachedItem = proxy.Get(new SiteModelChangeBufferQueueKey(siteModel.ID, insertUtc));
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
      var proxy = DIContext.Obtain<Func<IStorageProxyCache<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>>>()();
      proxy.Should().NotBeNull();
 
      // Check the new item was placed into the cache
      var testProxy = proxy as IStorageProxyCacheTransacted_TestHarness<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>;
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
