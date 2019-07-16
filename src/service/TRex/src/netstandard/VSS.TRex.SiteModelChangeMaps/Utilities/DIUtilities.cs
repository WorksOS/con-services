﻿using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.SiteModelChangeMaps.Utilities
{
  public static class DIUtilities
  {
    /// <summary>
    /// Add the factories for the storage proxy caches, both standard and transacted, for spatial and non spatial caches in TRex
    /// </summary>
    private static void AddDIEntries()
    {
      var nonTransactedProxy = new StorageProxyCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>(DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Immutable)?.GetCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>(TRexCaches.SiteModelChangeBufferQueueCacheName()));
      var transactedProxy = new StorageProxyCacheTransacted<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>(DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Immutable)?.GetCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>(TRexCaches.SiteModelChangeBufferQueueCacheName()), new SiteModelChangeBufferQueueKeyEqualityComparer());

      DIBuilder.Continue()

        //***********************************************
        // Injected factories for non-transacted proxies
        // **********************************************

        // Add the singleton reference to the non-transacted site model change map cache
        .Add(x => x.AddSingleton<IStorageProxyCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>(nonTransactedProxy))

        //***********************************************
        // Injected factories for transacted proxies
        // ***********************************************

        // Add the singleton reference to the non-transacted site model change map cache
        .Add(x => x.AddSingleton<IStorageProxyCacheTransacted<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>(transactedProxy));
    }

    /// <summary>
    /// If the calling context is directly using an IServiceCollection then obtain the DIBuilder based on it before adding...
    /// </summary>
    /// <param name="services"></param>
    public static void AddProxyCacheFactoriesToDI(IServiceCollection services)
    {
      DIBuilder.Continue(services).Add(x => AddDIEntries());
    }
  }
}
