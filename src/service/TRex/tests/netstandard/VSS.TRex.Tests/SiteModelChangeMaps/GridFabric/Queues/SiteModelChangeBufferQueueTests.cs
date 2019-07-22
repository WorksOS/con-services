using System;
using FluentAssertions;
using FluentAssertions.Primitives;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
using VSS.TRex.SiteModelChangeMaps.Interfaces;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModelChangeMaps.GridFabric.Queues
{
  public class SiteModelChangeBufferQueueTests_NoCache : SiteModelChangeTestsBase
  {
    [Fact]
    public void Creation_FailWith_NoCacheAvailable()
    {
      IgniteMock.RemoveMockedCacheFromIgniteMock<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>();

      Action act = () => _ = new SiteModelChangeBufferQueue();
      act.Should().Throw<TRexException>().WithMessage("Ignite cache not available");
    }
  }

  public class SiteModelChangeBufferQueueTests : SiteModelChangeTestsBase
  {
    [Fact]
    public void Creation()
    {
      var queue = new SiteModelChangeBufferQueue();
      queue.Should().NotBeNull();
    }

    [Fact]
    public void Add()
    {
      var queue = new SiteModelChangeBufferQueue();
      var projectUid = Guid.NewGuid();
      var insertUTC = DateTime.UtcNow;

      queue.Add(new SiteModelChangeBufferQueueKey(projectUid, insertUTC), new SiteModelChangeBufferQueueItem
      {
        MachineUid = Guid.NewGuid(),
        ProjectUID = projectUid,
        InsertUTC = insertUTC,
        Content = new byte[1],
        Origin = SiteModelChangeMapOrigin.Ingest,
        Operation = SiteModelChangeMapOperation.AddSpatialChanges
      }).Should().BeTrue();
    }
  }
}
