using System;
using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Events;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.TRex.Tests.Designs.GridFabric
{

  public class DesignChangedEventSenderTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var sender = new DesignChangedEventSender();
      sender.Should().NotBeNull();
    }

    [Fact]
    public void DesignStateChanged_Mutable()
    {
      var sender = new DesignChangedEventSender();
      sender.DesignStateChanged(DesignNotificationGridMutability.NotifyMutable, Guid.NewGuid(), Guid.NewGuid(), ImportedFileType.DesignSurface);
    }

    [Fact]
    public void DesignStateChanged_Immutable()
    {
      var sender = new DesignChangedEventSender();
      sender.DesignStateChanged(DesignNotificationGridMutability.NotifyImmutable, Guid.NewGuid(), Guid.NewGuid(), ImportedFileType.DesignSurface);
    }


    [Fact]
    public void DesignStateChanged_All()
    {
      var sender = new DesignChangedEventSender();
      sender.DesignStateChanged(DesignNotificationGridMutability.NotifyAll, Guid.NewGuid(), Guid.NewGuid(), ImportedFileType.DesignSurface);
    }
  }

}
