using System;
using Apache.Ignite.Core.Cache.Configuration;
using FluentAssertions;
using Moq;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
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
      var mockIgnite = IgniteMock.Immutable.mockIgnite;

      mockIgnite.Setup(x => x.GetOrCreateCache<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>(It.IsAny<CacheConfiguration>())).Returns((CacheConfiguration configurationStore) => null);
      mockIgnite.Setup(x => x.GetCache<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>(It.IsAny<string>())).Returns((string s) => null);

      Action act = () => _ = new SiteModelChangeBufferQueue();
      act.Should().Throw<TRexException>().WithMessage("Ignite cache not available");
    }
  }
}
