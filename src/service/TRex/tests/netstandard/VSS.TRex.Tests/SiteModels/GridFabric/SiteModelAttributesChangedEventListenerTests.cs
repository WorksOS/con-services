using System;
using FluentAssertions;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModels.GridFabric.Events;
using VSS.TRex.Tests.BinarizableSerialization;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModels.GridFabric
{
  public class SiteModelAttributesChangedEventListenerTests_WithoutDIContext : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Creation()
    {
      var listener = new SiteModelAttributesChangedEventListener();

      listener.Should().NotBeNull();
      listener.GridName.Should().BeNullOrEmpty();
      listener.MessageTopicName.Should().Be(SiteModelAttributesChangedEventListener.SITE_MODEL_ATTRIBUTES_CHANGED_EVENTS_TOPIC_NAME);
    }

    [Fact]
    public void Creation2()
    {
      var listener = new SiteModelAttributesChangedEventListener("GridName");

      listener.Should().NotBeNull();
      listener.GridName.Should().Be("GridName");
      listener.MessageTopicName.Should().Be(SiteModelAttributesChangedEventListener.SITE_MODEL_ATTRIBUTES_CHANGED_EVENTS_TOPIC_NAME);
    }

    [Fact]
    public void Dispose()
    {
      var listener = new SiteModelAttributesChangedEventListener();
      listener.Dispose();
    }

    [Fact]
    public void ReadWriteBinarizable()
    {
      SimpleBinarizableInstanceTester.TestClass<SiteModelAttributesChangedEventListener>("Empty SiteModelAttributesChangedEventListener not same after round trip serialisation");

      var listener = new SiteModelAttributesChangedEventListener();
      TestBinarizable_ReaderWriterHelper.RoundTripSerialise(listener);

      var listener2 = new SiteModelAttributesChangedEventListener("GridName")
      {
        MessageTopicName = "TestMessageTopic"
      };

      TestBinarizable_ReaderWriterHelper.RoundTripSerialise(listener2);
    }

    [Fact]
    public void Invoke_WithNoSiteModels()
    {
      var message = new SiteModelAttributesChangedEvent();
      var listener = new SiteModelAttributesChangedEventListener();
      listener.Invoke(Guid.Empty, message).Should().BeFalse();
    }

    [Fact]
    public void StartListening_FailWithNoMessagingFabric()
    {
      var listener = new SiteModelAttributesChangedEventListener(TRexGrids.ImmutableGridName())
      {
        MessageTopicName = "TestMessageTopic"
      };
      listener.StartListening();
    }
  }

  public class SiteModelAttributesChangedEventListenerTests_WithFullDIContext : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Invoke_WithSiteModels()
    {
      var message = new SiteModelAttributesChangedEvent();
      var listener = new SiteModelAttributesChangedEventListener(TRexGrids.ImmutableGridName());
      listener.Invoke(Guid.Empty, message).Should().BeTrue();
    }

    [Fact]
    public void StartListening()
    {
      var listener = new SiteModelAttributesChangedEventListener(TRexGrids.ImmutableGridName())
      {
        MessageTopicName = "TestMessageTopic"
      };
      listener.StartListening();
    }

    [Fact]
    public void StopListening()
    {
      var listener = new SiteModelAttributesChangedEventListener(TRexGrids.ImmutableGridName());
      listener.StopListening();
    }

  }
}
