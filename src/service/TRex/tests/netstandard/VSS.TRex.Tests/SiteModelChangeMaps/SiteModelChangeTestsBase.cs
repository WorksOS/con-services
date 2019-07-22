using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModelChangeMaps;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
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
      var nonTransactedProxy = new StorageProxyCacheTransacted_TestHarness<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>(DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Immutable)?.GetOrCreateCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>(TRexCaches.SiteModelChangeBufferQueueCacheName()), new SiteModelChangeBufferQueueKeyEqualityComparer());
      var transactedProxy = new StorageProxyCacheTransacted_TestHarness<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>(DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Immutable)?.GetOrCreateCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>(TRexCaches.SiteModelChangeBufferQueueCacheName()), new SiteModelChangeBufferQueueKeyEqualityComparer());

      DIBuilder
        .Continue()

        // Add the factories for the storage proxy caches, both standard and transacted, for spatial and non spatial caches in TRex

        ////////////////////////////////////////////////////
        // Injected standard storage proxy cache 
        ////////////////////////////////////////////////////

        // Add the singleton reference to the non-transacted site model change map cache
        .Add(x => x.AddSingleton<IStorageProxyCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>(nonTransactedProxy))

        /////////////////////////////////////////////////////
        // Injected transacted storage proxy cache factories
        /////////////////////////////////////////////////////

        // Add the singleton reference to the transacted site model change map cache
        .Add(x => x.AddSingleton<IStorageProxyCacheTransacted<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>(transactedProxy))

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
