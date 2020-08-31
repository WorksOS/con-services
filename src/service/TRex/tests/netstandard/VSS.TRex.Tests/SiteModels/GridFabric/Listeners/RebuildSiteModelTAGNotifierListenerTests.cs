using System;
using FluentAssertions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.SiteModels.GridFabric.Listeners;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.Tests.BinarizableSerialization;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModels.GridFabric.Listeners
{
  public class RebuildSiteModelTAGNotifierListenerTests_WithoutDIContext : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Creation()
    {
      var listener = new RebuildSiteModelTAGNotifierListener();

      listener.Should().NotBeNull();
      listener.MessageTopicName.Should().Be(RebuildSiteModelTAGNotifierListener.SITE_MODEL_REBUILDER_TAG_FILE_PROCESSED_EVENT_TOPIC_NAME);
    }

    [Fact]
    public void Creation2()
    {
      var listener = new RebuildSiteModelTAGNotifierListener();

      listener.Should().NotBeNull();
      listener.MessageTopicName.Should().Be(RebuildSiteModelTAGNotifierListener.SITE_MODEL_REBUILDER_TAG_FILE_PROCESSED_EVENT_TOPIC_NAME);
    }

    [Fact]
    public void Dispose()
    {
      var listener = new RebuildSiteModelTAGNotifierListener();
      listener.Dispose();
    }

    [Fact]
    public void ReadWriteBinarizable()
    {
      SimpleBinarizableInstanceTester.TestClass<RebuildSiteModelTAGNotifierListener>("Empty RebuildSiteModelTAGNotifierListener not same after round trip serialisation");

      var listener = new RebuildSiteModelTAGNotifierListener();
      TestBinarizable_ReaderWriterHelper.RoundTripSerialise(listener);

      var listener2 = new RebuildSiteModelTAGNotifierListener {MessageTopicName = "TestMessageTopic"};

      TestBinarizable_ReaderWriterHelper.RoundTripSerialise(listener2);
    }


    [Fact]
    public void Invoke_WithNoResponseItems()
    {
      var message = new RebuildSiteModelTAGNotifierEvent();
      var listener = new RebuildSiteModelTAGNotifierListener();
      listener.Invoke(Guid.Empty, message).Should().BeTrue();
    }

    [Fact]
    public void Invoke_WithNoRebuilderManager()
    {
      var message = new RebuildSiteModelTAGNotifierEvent
      {
        ProjectUid = Guid.NewGuid(),
        ResponseItems = new[]
        {
          new ProcessTAGFileResponseItem
          {
            AssetUid = Guid.NewGuid(),
            Exception = "",
            FileName = "ATagFile.tag",
            ReadResult = TAGReadResult.NoError,
            SubmissionFlags = TAGFileSubmissionFlags.AddToArchive | TAGFileSubmissionFlags.NotifyRebuilderOnProceesing,
            Success = true,
            OriginSource = TAGFileOriginSource.LegacyTAGFileSource // Only legacy TAG files supported for rebuilding
          }
        }
      };

      var listener = new RebuildSiteModelTAGNotifierListener();
      listener.Invoke(Guid.Empty, message).Should().BeTrue();
    }

    [Fact]
    public void StartListening_FailWithNoMessagingFabric()
    {
      var listener = new RebuildSiteModelTAGNotifierListener {MessageTopicName = "TestMessageTopic"};
      listener.StartListening();
    }
  }

  public class RebuildSiteModelTAGNotifierListenerTests_WithFullDIContext : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Invoke_WithSiteModels()
    {
      var message = new RebuildSiteModelTAGNotifierEvent
      {
        ProjectUid = Guid.NewGuid(),
        ResponseItems = new[]
        {
          new ProcessTAGFileResponseItem
          {
            AssetUid = Guid.NewGuid(),
            Exception = "",
            FileName = "ATagFile.tag",
            ReadResult = TAGReadResult.NoError,
            SubmissionFlags = TAGFileSubmissionFlags.AddToArchive | TAGFileSubmissionFlags.NotifyRebuilderOnProceesing,
            Success = true,
            OriginSource = TAGFileOriginSource.LegacyTAGFileSource // Only legacy TAG files supported for rebuilding
          }
        }
      };

      var listener = new RebuildSiteModelTAGNotifierListener();
      listener.Invoke(Guid.Empty, message).Should().BeTrue();
    }

    [Fact]
    public void StartListening()
    {
      var listener = new RebuildSiteModelTAGNotifierListener {MessageTopicName = "TestMessageTopic"};
      listener.StartListening();
    }

    [Fact]
    public void StopListening()
    {
      var listener = new RebuildSiteModelTAGNotifierListener();
      listener.StopListening();
    }
  }
}
