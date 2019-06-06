using System;
using FluentAssertions;
using VSS.TRex.SiteModels.GridFabric.Events;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModels.GridFabric
{
  public class SiteModelAttributesChangedEventSenderTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var sender = new SiteModelAttributesChangedEventSender();
      sender.Should().NotBeNull();
    }

    [Fact]
    public void ModelAttributesChanged_Mutable()
    {
      var sender = new SiteModelAttributesChangedEventSender();

      sender.ModelAttributesChanged(SiteModelNotificationEventGridMutability.NotifyMutable, Guid.NewGuid());
    }

    [Fact]
    public void ModelAttributesChanged_Immutable()
    {
      var sender = new SiteModelAttributesChangedEventSender();

      sender.ModelAttributesChanged(SiteModelNotificationEventGridMutability.NotifyImmutable, Guid.NewGuid());
    }


    [Fact]
    public void ModelAttributesChanged_All()
    {
      var sender = new SiteModelAttributesChangedEventSender();

      sender.ModelAttributesChanged(SiteModelNotificationEventGridMutability.NotifyAll, Guid.NewGuid());
    }
  }
}
