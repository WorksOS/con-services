using System;
using Apache.Ignite.Core;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Affinity;
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

        //***********************************************
        // Injected factories for non-transacted proxies
        // **********************************************

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>>>
        (factory => (ignite, mutability, streamType) => new StorageProxyCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>(ignite?.GetCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.SpatialCacheName(mutability, streamType)))))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>>>
        (factory => (ignite, mutability, streamType) => new StorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(ignite?.GetCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.NonSpatialCacheName(mutability, streamType)))))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCache<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper>>>
        (factory => (ignite, mutability, streamType) =>
        {
          // SiteModel change maps are only maintained on the immutable grid
          if (mutability != StorageMutability.Immutable)
            return null;

          return new StorageProxyCache<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper>(ignite?.GetCache<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.NonSpatialCacheName(mutability, streamType)));
        }))

        //***********************************************
        // Injected factories for transacted proxies
        // **********************************************

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCacheTransacted<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>>>
        (factory => (ignite, mutability, streamType) => new StorageProxyCacheTransacted<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>(ignite?.GetCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.SpatialCacheName(mutability, streamType)), new SubGridSpatialAffinityKeyEqualityComparer())))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCacheTransacted<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>>>
        (factory => (ignite, mutability, streamType) => new StorageProxyCacheTransacted<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(ignite?.GetCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.NonSpatialCacheName(mutability, streamType)), new NonSpatialAffinityKeyEqualityComparer())))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCacheTransacted<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper>>>
        (factory => (ignite, mutability, streamType) =>
        {
          // SiteModel change maps are only maintained on the immutable grid
          if (mutability != StorageMutability.Immutable)
            return null;

          return new StorageProxyCacheTransacted<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper>(ignite?.GetCache<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.NonSpatialCacheName(mutability, streamType)), new SiteModelMachineAffinityKeyEqualityComparer());
        }));
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
