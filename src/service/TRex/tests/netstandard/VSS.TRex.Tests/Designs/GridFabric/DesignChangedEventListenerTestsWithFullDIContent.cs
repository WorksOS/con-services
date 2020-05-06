using System;
using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Events;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Designs.GridFabric
{
  public class DesignChangedEventListenerTestsWithFullDIContent : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {

    [Fact]
    public void Invoke_WithSiteModels()
    {
      var message = new DesignChangedEvent();
      var listener = new DesignChangedEventListener(TRexGrids.ImmutableGridName());
      listener.Invoke(Guid.Empty, message).Should().BeTrue();
    }

    [Fact]
    public void StartListening()
    {
      var listener = new DesignChangedEventListener(TRexGrids.ImmutableGridName())
      {
        MessageTopicName = "TestMessageTopic"
      };
      listener.StartListening();
    }

    [Fact]
    public void StopListening()
    {
      var listener = new DesignChangedEventListener(TRexGrids.ImmutableGridName());
      listener.StopListening();
    }

  }
}
