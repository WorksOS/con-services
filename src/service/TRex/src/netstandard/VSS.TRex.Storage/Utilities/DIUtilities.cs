using System;
using Apache.Ignite.Core;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.Storage.Utilities
{
  public static class DIUtilities
  {
    /// <summary>
    /// Add the factories for the storage proxy caches, both standard and transacted, for spatial and non spatial caches in TRex
    /// </summary>
    private static void AddDIEntries()
    {
      DIBuilder.Continue()
        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, IStorageProxyCache<ISubGridSpatialAffinityKey, byte[]>>>
        (factory => (ignite, mutability) => new StorageProxyCache<ISubGridSpatialAffinityKey, byte[]>(ignite?.GetCache<ISubGridSpatialAffinityKey, byte[]>(TRexCaches.SpatialCacheName(mutability)))))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, IStorageProxyCache<INonSpatialAffinityKey, byte[]>>>
        (factory => (ignite, mutability) => new StorageProxyCache<INonSpatialAffinityKey, byte[]>(ignite?.GetCache<INonSpatialAffinityKey, byte[]>(TRexCaches.NonSpatialCacheName(mutability)))))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, IStorageProxyCacheTransacted<ISubGridSpatialAffinityKey, byte[]>>>
        (factory => (ignite, mutability) => new StorageProxyCacheTransacted<ISubGridSpatialAffinityKey, byte[]>(ignite?.GetCache<ISubGridSpatialAffinityKey, byte[]>(TRexCaches.SpatialCacheName(mutability)))))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, IStorageProxyCacheTransacted<INonSpatialAffinityKey, byte[]>>>
        (factory => (ignite, mutability) => new StorageProxyCacheTransacted<INonSpatialAffinityKey, byte[]>(ignite?.GetCache<INonSpatialAffinityKey, byte[]>(TRexCaches.NonSpatialCacheName(mutability)))));
    }

    /// <summary>
    /// If the calling context is directly using an IServiceCollection then obtain the DIBuilder based on it before adding...
    /// </summary>
    /// <param name="services"></param>
    public static void AddProxyCacheFactoriesToDI(IServiceCollection services)
    {
      DIBuilder.Continue(services).Add(x => AddDIEntries());
    }

    public static void AddProxyCacheFactoriesToDI()
    {
      AddDIEntries();
    }
  }
}
