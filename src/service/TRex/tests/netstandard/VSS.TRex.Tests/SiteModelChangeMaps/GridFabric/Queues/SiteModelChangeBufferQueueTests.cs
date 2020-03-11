using System;
using FluentAssertions;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
using VSS.TRex.SiteModelChangeMaps.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.SiteModelChangeMaps.GridFabric.Queues
{
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
      var insertUtc = DateTime.UtcNow;

      queue.Add(new SiteModelChangeBufferQueueKey(projectUid, insertUtc), new SiteModelChangeBufferQueueItem
      {
        MachineUid = Guid.NewGuid(),
        ProjectUID = projectUid,
        InsertUTC = insertUtc,
        Content = new byte[1],
        Origin = SiteModelChangeMapOrigin.Ingest,
        Operation = SiteModelChangeMapOperation.AddSpatialChanges
      }).Should().BeTrue();
    }
  }
}
