using System;
using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Events;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Tests.BinarizableSerialization;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Designs.GridFabric
{

  public class DesignChangedEventListenerTests_WithoutDIContext : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Creation()
    {
      var listener = new DesignChangedEventListener();
      listener.Should().NotBeNull();
      listener.GridName.Should().BeNullOrEmpty();
      listener.MessageTopicName.Should().Be(DesignChangedEventListener.DESIGN_CHANGED_EVENTS_TOPIC_NAME);
    }

    [Fact]
    public void Creation2()
    {
      var listener = new DesignChangedEventListener("GridName");
      listener.Should().NotBeNull();
      listener.GridName.Should().Be("GridName");
      listener.MessageTopicName.Should().Be(DesignChangedEventListener.DESIGN_CHANGED_EVENTS_TOPIC_NAME);
    }

    [Fact]
    public void Dispose()
    {
      var listener = new DesignChangedEventListener();
      listener.Dispose();
    }

    [Fact]
    public void ReadWriteBinarizable()
    {
      SimpleBinarizableInstanceTester.TestClass<DesignChangedEventListener>("Empty DesignChangedEventListener not same after round trip serialisation");

      var listener = new DesignChangedEventListener();
      TestBinarizable_ReaderWriterHelper.RoundTripSerialise(listener);

      var listener2 = new DesignChangedEventListener("GridName")
      {
        MessageTopicName = "TestMessageTopic"
      };

      TestBinarizable_ReaderWriterHelper.RoundTripSerialise(listener2);
    }

    [Fact]
    public void StartListening_FailWithNoMessagingFabric()
    {
      var listener = new DesignChangedEventListener(TRexGrids.ImmutableGridName())
      {
        MessageTopicName = "TestMessageTopic"
      };
      listener.StartListening();
    }
  }

  public class DesignChangedEventListenerTests_WithFullDIContext : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
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
