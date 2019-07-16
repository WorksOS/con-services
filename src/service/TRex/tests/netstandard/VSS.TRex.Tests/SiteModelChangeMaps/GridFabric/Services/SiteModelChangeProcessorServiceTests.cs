using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cache.Query.Continuous;
using Apache.Ignite.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.TRex.DI;
using VSS.TRex.SiteModelChangeMaps;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Services;
using VSS.TRex.SiteModelChangeMaps.Interfaces;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModelChangeMaps.GridFabric.Services
{
  public class SiteModelChangeProcessorServiceTests : SiteModelChangeTestsBase
  {
    private IServiceContext TestServiceContext()
    {
      var mockSpanContext = new Mock<IServiceContext>();
      mockSpanContext.Setup(x => x.Name).Returns("UnitTest");
      return mockSpanContext.Object;
    }

    [Fact]
    public void Creation()
    {
      var service = new SiteModelChangeProcessorService();
      service.Should().NotBeNull();
      service.Aborted.Should().BeFalse();
      service.InSteadyState.Should().BeFalse();
    }

    [Fact]
    public void Init()
    {
      var service = new SiteModelChangeProcessorService();
      service.Init(TestServiceContext());
      service.Aborted.Should().BeFalse();
      service.InSteadyState.Should().BeFalse();
    }

    [Fact]
    public void Cancel()
    {
      var service = new SiteModelChangeProcessorService();
      service.Cancel(TestServiceContext());
      service.Aborted.Should().BeTrue();
    }


    [Fact]
    public void Execute()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var projectGuid = siteModel.ID;
      var insertUTC = DateTime.UtcNow;
      var changeMap = new SubGridTreeSubGridExistenceBitMask
      {
        [SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset] = true
      };

      var queryResult = new List<ICacheEntry<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>();
      var mockCacheEntry = new Mock<ICacheEntry<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>();
      var key = new SiteModelChangeBufferQueueKey(projectGuid, insertUTC);
      mockCacheEntry.Setup(x => x.Key).Returns(key);
      mockCacheEntry.Setup(x => x.Value).Returns(new SiteModelChangeBufferQueueItem
      {
        ProjectUID = projectGuid,
        InsertUTC = insertUTC, 
        Content = changeMap.ToBytes(),
        Origin = SiteModelChangeMapOrigin.Ingest,
        Operation = SiteModelChangeMapOperation.AddSpatialChanges
      });

      queryResult.Add(mockCacheEntry.Object);

      var mockQueryCursor = new Mock<IQueryCursor<ICacheEntry<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>>();
      mockQueryCursor.Setup(x => x.GetAll()).Returns(queryResult);
      mockQueryCursor.Setup(x => x.GetEnumerator()).Returns(queryResult.GetEnumerator());

      var mockQueryHandle = new Mock<IContinuousQueryHandle<ICacheEntry<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>>();
      mockQueryHandle.Setup(x => x.GetInitialQueryCursor()).Returns(mockQueryCursor.Object);

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<Func<LocalSiteModelChangeListener, IContinuousQueryHandle<ICacheEntry<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>>>(listener => mockQueryHandle.Object))
        .Complete();

      var service = new SiteModelChangeProcessorService();

      var thread = new Thread(() => service.Execute(TestServiceContext()));
      thread.Start();

      var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
      while (!service.InSteadyState)
      {
        waitHandle.WaitOne(100);
      }

      // Allow some time for the handler to complete processing the item provided to the service
      waitHandle.WaitOne(200);

      service.InSteadyState.Should().BeTrue();

      service.Cancel(TestServiceContext());
      service.Aborted.Should().BeTrue();

      var proxy = new SiteModelChangeMapProxy();
      var resultChangeMap = proxy.Get(projectGuid, siteModel.Machines.First().ID);

      resultChangeMap.CountBits().Should().Be(1);
      resultChangeMap[SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset].Should().BeTrue();
    }
  }
}
