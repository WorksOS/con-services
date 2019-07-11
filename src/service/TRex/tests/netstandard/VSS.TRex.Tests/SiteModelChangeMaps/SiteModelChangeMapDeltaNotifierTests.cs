using System;
using FluentAssertions;
using VSS.TRex.DI;
using VSS.TRex.SiteModelChangeMaps;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
using VSS.TRex.SiteModelChangeMaps.Interfaces;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModelChangeMaps
{
  public class SiteModelChangeMapDeltaNotifierTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var notifier = new SiteModelChangeMapDeltaNotifier();
      notifier.Should().NotBeNull();
    }

    [Fact]
    public void Notify_EmptyChangeMap()
    {
      var notifier = new SiteModelChangeMapDeltaNotifier();

      // The notifier uses the non-transacted storage proxy:
      var proxy = DIContext.Obtain<IStorageProxyCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>();
      proxy.Should().NotBeNull();
      proxy.Clear();

      var projectUid = Guid.NewGuid();
      var insertUTC = DateTime.UtcNow;

      // Ask the notifier to notify a new item
      notifier.Notify(projectUid, insertUTC, new SubGridTreeSubGridExistenceBitMask(), SiteModelChangeMapOrigin.Ingest, SiteModelChangeMapOperation.AddSpatialChanges);

      // Check the new item was placed into the cache
      var cachedItem = proxy.Get(new SiteModelChangeBufferQueueKey(projectUid, insertUTC));
      cachedItem.Should().NotBeNull();

      var readMap = new SubGridTreeSubGridExistenceBitMask();
      readMap.FromBytes(cachedItem.Content);

      readMap.CountBits().Should().Be(0);
    }
  }
}
