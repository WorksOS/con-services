using System;
using System.Collections.Generic;
using Apache.Ignite.Core.Cache.Event;
using FluentAssertions;
using Moq;
using VSS.TRex.SiteModelChangeMaps;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;
using Xunit;

namespace VSS.TRex.Tests.SiteModelChangeMaps
{
  public class LocalSiteModelChangeListenerTests : SiteModelChangeTestsBase
  {
    [Fact]
    public void Creation()
    {
      var listener = new LocalSiteModelChangeListener(new SiteModelChangeProcessorItemHandler());
      listener.Should().NotBeNull();
      listener.Handler.Should().NotBeNull();
    }

    [Fact]
    public void Creation_NoHandler()
    {
      Action act = () => new LocalSiteModelChangeListener(null);
      act.Should().Throw<ArgumentException>().WithMessage("Listener must be supplied with a handler");
    }

    [Theory]
    [InlineData(CacheEntryEventType.Created, true)]
    [InlineData(CacheEntryEventType.Updated, false)]
    [InlineData(CacheEntryEventType.Removed, false)]
    public void OnEvent_AcceptedEvent(CacheEntryEventType evtType, bool expectedToBeAccepted)
    {
      var mockEvent = new Mock<ICacheEntryEvent<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>();
      mockEvent.Setup(x => x.EventType).Returns(evtType);

      using (var handler = new SiteModelChangeProcessorItemHandler())
      {
        var listener = new LocalSiteModelChangeListener(handler);
        listener.OnEvent(new List<ICacheEntryEvent<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>> {mockEvent.Object});

        // the listener should have placed the event in to the handler
        handler.QueueCount.Should().Be(expectedToBeAccepted ? 1 : 0);
      }
    }
  }
}
