using System;
using Apache.Ignite.Core;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.SiteModelChangeMaps.Utilities
{
  public static class DIUtilities
  {
    /// <summary>
    /// Add the factories for the storage proxy caches, both standard and transacted, for spatial and non spatial caches in TRex
    /// </summary>
    private static void AddDIEntries()
    {
      DIBuilder.Continue()

        //***********************************************
        // Injected factories for non-transacted proxies
        // **********************************************

        .Add(x => x.AddSingleton<Func<IIgnite, IStorageProxyCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>>
          (factory => ignite => new StorageProxyCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>(ignite?.GetCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>(TRexCaches.SiteModelChangeBufferQueueCacheName()))))

        //***********************************************
        // Injected factories for non-transacted proxies
        // ***********************************************

        .Add(x => x.AddSingleton<Func<IIgnite, IStorageProxyCacheTransacted<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>>
          (factory => ignite => new StorageProxyCacheTransacted<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>(ignite?.GetCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>(TRexCaches.SiteModelChangeBufferQueueCacheName()), new SiteModelChangeBufferQueueKeyEqualityComparer())));
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
