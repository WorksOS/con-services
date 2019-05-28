using System;
using Apache.Ignite.Core;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.Types;

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

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCache<ISubGridSpatialAffinityKey, byte[]>>>
        (factory => (ignite, mutability, streamType) => new StorageProxyCache<ISubGridSpatialAffinityKey, byte[]>(ignite?.GetCache<ISubGridSpatialAffinityKey, byte[]>(TRexCaches.SpatialCacheName(mutability, streamType)))))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCache<INonSpatialAffinityKey, byte[]>>>
        (factory => (ignite, mutability, streamType) => new StorageProxyCache<INonSpatialAffinityKey, byte[]>(ignite?.GetCache<INonSpatialAffinityKey, byte[]>(TRexCaches.NonSpatialCacheName(mutability, streamType)))))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCacheTransacted<ISubGridSpatialAffinityKey, byte[]>>>
        (factory => (ignite, mutability, streamType) => new StorageProxyCacheTransacted<ISubGridSpatialAffinityKey, byte[]>(ignite?.GetCache<ISubGridSpatialAffinityKey, byte[]>(TRexCaches.SpatialCacheName(mutability, streamType)))))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCacheTransacted<INonSpatialAffinityKey, byte[]>>>
        (factory => (ignite, mutability, streamType) => new StorageProxyCacheTransacted<INonSpatialAffinityKey, byte[]>(ignite?.GetCache<INonSpatialAffinityKey, byte[]>(TRexCaches.NonSpatialCacheName(mutability, streamType)))));
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
