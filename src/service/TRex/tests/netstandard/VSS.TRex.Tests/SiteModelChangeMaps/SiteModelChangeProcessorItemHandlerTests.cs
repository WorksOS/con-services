using System;
using System.IO;
using System.Linq;
using System.Threading;
using Apache.Ignite.Core.Cache.Event;
using FluentAssertions;
using Moq;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.SiteModelChangeMaps;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
using VSS.TRex.SiteModelChangeMaps.Interfaces;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModelChangeMaps
{
  public class SiteModelChangeProcessorItemHandlerTests_NoDI
  {
    [Fact]
    public void Creation_NoIgnite()
    {
      Action act = () =>
      {
        var _ = new SiteModelChangeProcessorItemHandler();
      };
      act.Should().Throw<TRexException>().WithMessage("Failed to obtain immutable Ignite reference");
    }
  }

  public class SiteModelChangeProcessorItemHandlerTests : SiteModelChangeTestsBase
  {
    [Fact]
    public void Creation()
    {
      using (var handler = new SiteModelChangeProcessorItemHandler())
      {
        handler.Should().NotBeNull();
        handler.Active.Should().BeFalse();
        handler.Aborted.Should().BeFalse();
      }
    }

    [Fact]
    public void Activation()
    {
      using (var handler = new SiteModelChangeProcessorItemHandler())
      {
        handler.Activate();
        handler.Active.Should().BeTrue();
        handler.Aborted.Should().BeFalse();
      }
    }

    [Fact]
    public void Abort()
    {
      using (var handler = new SiteModelChangeProcessorItemHandler())
      {
        handler.Abort();
        handler.Aborted.Should().BeTrue();
      }
    }

    private void PerformProcessEvent(SiteModelChangeBufferQueueKey key, SiteModelChangeBufferQueueItem value)
    {
      var mockEvent = new Mock<ICacheEntryEvent<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>();
      mockEvent.Setup(x => x.EventType).Returns(CacheEntryEventType.Created);
      mockEvent.Setup(x => x.Key).Returns(key);
      mockEvent.Setup(x => x.Value).Returns(value);

      using (var handler = new SiteModelChangeProcessorItemHandler())
      {
        handler.Add(mockEvent.Object);
        handler.QueueCount.Should().Be(1);

        // The handler has an item. Activate it to cause it to exercise ProcessChangeMapUpdateItems()

        handler.Activate();

        int count = 0;
        while (count++ == 0 || handler.QueueCount > 0)
        {
          var wait = new EventWaitHandle(false, EventResetMode.ManualReset);
          wait.WaitOne(1000);
        }

        // Check the item was removed from the queue
        handler.QueueCount.Should().Be(0);
      }
    }

    private void TestSiteModelAndChangeMap_Ingest(ISiteModel siteModel, ISubGridTreeBitMask changeMap, int finalBitCount)
    {
      var insertTC = DateTime.UtcNow;
      var key = new SiteModelChangeBufferQueueKey(siteModel.ID, insertTC);
      var value = new SiteModelChangeBufferQueueItem
      {
        ProjectUID = siteModel.ID,
        MachineUid = siteModel.Machines.First().ID,
        InsertUTC = insertTC,
        Operation = SiteModelChangeMapOperation.AddSpatialChanges,
        Origin = SiteModelChangeMapOrigin.Ingest,
        Content = changeMap.ToBytes()
      };

      PerformProcessEvent(key, value);

      // Check there is now a change map item for the site model with the given content
      var changeMapProxy = new SiteModelChangeMapProxy();
      var resultChangeMap = changeMapProxy.Get(key.ProjectUID, value.MachineUid);

      resultChangeMap.Should().NotBeNull();
      resultChangeMap.CountBits().Should().Be(finalBitCount);
    }

    [Fact]
    public void ProcessChangeMapUpdateItems_EmptyMap()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var changeMap = new SubGridTreeSubGridExistenceBitMask();

      TestSiteModelAndChangeMap_Ingest(siteModel, changeMap, 0);
    }

    [Fact]
    public void ProcessChangeMapUpdateItems_NonEmptyMap_SingleSubGridAtOrigin()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var changeMap = new SubGridTreeSubGridExistenceBitMask
      {
        [SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset] = true
      };

      TestSiteModelAndChangeMap_Ingest(siteModel, changeMap, 1);
    }

    [Fact]
    public void ProcessChangeMapUpdateItems_NonEmptyMap_SingleTAGFile()
    {
      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var changeMap = new SubGridTreeSubGridExistenceBitMask();

      TestSiteModelAndChangeMap_Ingest(siteModel, changeMap, 12);
    }

    private void TestSiteModelAndChangeMap_Query(ISiteModel siteModel, Guid machineUid, ISubGridTreeBitMask changeMap, int finalBitCount)
    {
      var insertTC = DateTime.UtcNow;
      var key = new SiteModelChangeBufferQueueKey(siteModel.ID, insertTC);
      var value = new SiteModelChangeBufferQueueItem
      {
        ProjectUID = siteModel.ID,
        MachineUid = machineUid, //siteModel.Machines.First().ID,
        InsertUTC = insertTC,
        Operation = SiteModelChangeMapOperation.RemoveSpatialChanges,
        Origin = SiteModelChangeMapOrigin.Query,
        Content = changeMap.ToBytes()
      };

      PerformProcessEvent(key, value);

      // Check there is now a change map item for the site model with the given content
      var changeMapProxy = new SiteModelChangeMapProxy();
      var resultChangeMap = changeMapProxy.Get(key.ProjectUID, value.MachineUid);

      resultChangeMap.Should().NotBeNull();
      resultChangeMap.CountBits().Should().Be(finalBitCount);
    }

    [Fact]
    public void ProcessChangeMapUpdateItems_EmptyMap_IngestAndQuery()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var changeMap = new SubGridTreeSubGridExistenceBitMask();

      TestSiteModelAndChangeMap_Ingest(siteModel, changeMap, 0);
      TestSiteModelAndChangeMap_Query(siteModel, siteModel.Machines.First().ID, changeMap, 0);
    }

    [Fact]
    public void ProcessChangeMapUpdateItems_NonEmptyMap_SingleSubGridAtOrigin_IngestAndQuery()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var changeMap = new SubGridTreeSubGridExistenceBitMask
      {
        [SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset] = true
      };

      TestSiteModelAndChangeMap_Ingest(siteModel, changeMap, 1);
      TestSiteModelAndChangeMap_Query(siteModel, siteModel.Machines.First().ID, changeMap, 0);
    }

    [Fact]
    public void ProcessChangeMapUpdateItems_NonEmptyMap_SingleTAGFile_IngestAndQuery()
    {
      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      TestSiteModelAndChangeMap_Ingest(siteModel, siteModel.ExistenceMap, 12);
      TestSiteModelAndChangeMap_Query(siteModel, siteModel.Machines.First().ID, siteModel.ExistenceMap, 0);
    }
  }
}

