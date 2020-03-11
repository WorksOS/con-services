using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModelChangeMaps;
using VSS.TRex.SiteModelChangeMaps.Interfaces;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModelChangeMaps
{
  public class SiteModelChangeTestsBase : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddChangeMapQueueCacheToDI()
    {
      var transactedProxy = new StorageProxyCacheTransacted_TestHarness<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>
        (DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Immutable)?.GetCache<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>(TRexCaches.SiteModelChangeBufferQueueCacheName()), new SiteModelChangeBufferQueueKeyEqualityComparer());

      var nonTransactedProxy = new StorageProxyCacheTransacted_TestHarness<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>
        (DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Immutable)?.GetCache<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>(TRexCaches.SiteModelChangeBufferQueueCacheName()), new SiteModelChangeBufferQueueKeyEqualityComparer());

      DIBuilder
        .Continue()

        // Add the factories for the storage proxy caches, both standard and transacted, for spatial and non spatial caches in TRex

        ////////////////////////////////////////////////////
        // Injected standard storage proxy cache 
        ////////////////////////////////////////////////////

        // Add the singleton reference to the non-transacted site model change map cache
        .Add(x => x.AddSingleton<Func<IStorageProxyCache<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>>>(() => nonTransactedProxy))

        /////////////////////////////////////////////////////
        // Injected transacted storage proxy cache factories
        /////////////////////////////////////////////////////

        // Add the singleton reference to the transacted site model change map cache
        .Add(x => x.AddSingleton<Func<IStorageProxyCacheTransacted<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>>>(() => transactedProxy))

        .Build()
        .Add(x => x.AddSingleton<ISiteModelChangeMapDeltaNotifier>(new SiteModelChangeMapDeltaNotifier()))

        .Build();
    }

    public SiteModelChangeTestsBase()
    {
      AddChangeMapQueueCacheToDI();
    }
  }
}
