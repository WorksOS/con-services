using System;
using Apache.Ignite.Core;
using Microsoft.Extensions.Logging;
using System.IO;
using VSS.Serilog.Extensions;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.Storage.Utilities;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Storage
{
  public abstract class StorageProxy_IgniteBase
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<StorageProxy_IgniteBase>();

    private IMutabilityConverter _mutabilityConverter;
    private IMutabilityConverter MutabilityConverter => _mutabilityConverter ?? (_mutabilityConverter = DIContext.Obtain<IMutabilityConverter>());

    protected readonly IIgnite ignite;

    protected IStorageProxyCache<INonSpatialAffinityKey, byte[]> generalNonSpatialCache;
    protected IStorageProxyCache<ISubGridSpatialAffinityKey, byte[]> spatialCache;

    public IStorageProxyCache<ISubGridSpatialAffinityKey, byte[]> SpatialCache => spatialCache;


    protected IStorageProxyCache<INonSpatialAffinityKey, byte[]> siteModelCache;

    public IStorageProxyCache<INonSpatialAffinityKey, byte[]> SiteModelCache => siteModelCache;


    protected IStorageProxyCache<ISiteModelMachineAffinityKey, byte[]> siteModelMachineCache;

    public IStorageProxyCache<ISiteModelMachineAffinityKey, byte[]> SiteModelMachineCache => siteModelMachineCache;

    /// <summary>
    /// Determines the correct cache to read/write particular types of information from/to
    /// </summary>
    /// <param name="streamType"></param>
    /// <returns></returns>
    public IStorageProxyCache<INonSpatialAffinityKey, byte[]> NonSpatialCache(FileSystemStreamType streamType)
    {
      switch (streamType)
      {
        case FileSystemStreamType.ProductionDataXML:
          return siteModelCache;
        default:
          return generalNonSpatialCache;
      }
    }

    /// <summary>
    /// Determines the correct cache to read/write information related to a machines activities within a project
    /// that are not a part of the general schema representing the ingested production data from machine control systems.
    /// eg: THe site model elevation change maps used to request only that spatial information has changed since the
    /// last time a machine asked for that information from a project.
    /// </summary>
    /// <param name="streamType"></param>
    /// <returns></returns>
    public IStorageProxyCache<ISiteModelMachineAffinityKey, byte[]> ProjectMachineCache(FileSystemStreamType streamType)
    {
      switch (streamType)
      {
        case FileSystemStreamType.SiteModelMachineElevationChangeMap:
          return siteModelMachineCache;
        default:
          return null;
      }
    }

    /// <summary>
    /// Controls which grid (Mutable or Immutable) this storage proxy performs reads and writes against.
    /// </summary>
    public StorageMutability Mutability { get; set; }

    public StorageProxy_IgniteBase(StorageMutability mutability)
    {
      Mutability = mutability;

      ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(mutability);
    }

    /// <summary>
    /// Computes the cache key name for a given data model and a given named stream within that datamodel
    /// </summary>
    /// <param name="DataModelID"></param>
    /// <param name="Name"></param>
    /// <returns></returns>
    protected static INonSpatialAffinityKey ComputeNamedStreamCacheKey(Guid DataModelID, string Name) => new NonSpatialAffinityKey(DataModelID, Name);

    /// <summary>
    /// Computes the cache key name for the given data model and a given spatial data stream within that datamodel
    /// </summary>
    /// <param name="DataModelID"></param>
    /// <param name="Name"></param>
    /// <param name="SubgridX"></param>
    /// <param name="SubgridY"></param>
    /// <returns></returns>
    protected static string ComputeNamedStreamCacheKey(long DataModelID, string Name, uint SubgridX, uint SubgridY)
    {
      return $"{DataModelID}-{Name}-{SubgridX}-{SubgridY}";
    }

    /// <summary>
    /// Supports taking a mutable version of a piece of data and transforming it into the immutable form if not present in the immutable cache
    /// </summary>
    /// <param name="mutableCache"></param>
    /// <param name="immutableCache"></param>
    /// <param name="cacheKey"></param>
    /// <param name="streamType"></param>
    /// <returns></returns>
    protected bool PerformNonSpatialImmutabilityConversion(IStorageProxyCache<INonSpatialAffinityKey, byte[]> mutableCache,
      IStorageProxyCache<INonSpatialAffinityKey, byte[]> immutableCache,
      INonSpatialAffinityKey cacheKey,
      FileSystemStreamType streamType)
    {
      if (mutableCache == null || immutableCache == null)
      {
        return false;
      }

      using (var MS = new MemoryStream(mutableCache.Get(cacheKey)))
      {
        using (var mutableStream = MemoryStreamCompression.Decompress(MS))
        {
          return PerformNonSpatialImmutabilityConversion(mutableStream, immutableCache, cacheKey, streamType, null);
        }
      }
    }

    /// <summary>
    /// Supports taking a mutable version of a piece of data and transforming it into the immutable form if not present in the immutable cache
    /// </summary>
    /// <param name="mutableStream"></param>
    /// <param name="immutableCache"></param>
    /// <param name="cacheKey"></param>
    /// <param name="streamType"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    protected bool PerformNonSpatialImmutabilityConversion(MemoryStream mutableStream,
      IStorageProxyCache<INonSpatialAffinityKey, byte[]> immutableCache,
      INonSpatialAffinityKey cacheKey,
      FileSystemStreamType streamType,
      object source)
    {
      if ((mutableStream == null && source == null) || immutableCache == null)
      {
        return false;
      }

      MemoryStream immutableStream = null;

      try
      {
        // Convert from the mutable to the immutable form and store it into the immutable cache
        if (MutabilityConverter.ConvertToImmutable(streamType, mutableStream, source, out immutableStream) && immutableStream != null)
        {
          using (var compressedStream = MemoryStreamCompression.Compress(immutableStream))
          {
            if (Log.IsTraceEnabled())
              Log.LogInformation(
                $"Putting key:{cacheKey} in {immutableCache.Name}, size:{immutableStream.Length} -> {compressedStream.Length}, ratio:{(compressedStream.Length / (1.0 * immutableStream.Length)) * 100}%");

            // Place the converted immutable item into the immutable cache
            immutableCache.Put(cacheKey, compressedStream.ToArray());

            return true;
          }
        }
        else
        {
          // There was no immutable version of the requested information. Allow this to bubble up the stack...
          Log.LogError(
            $"MutabilityConverter.ConvertToImmutable failed to convert mutable data for streamType={streamType}");

          return false;
        }
      }
      finally
      {
        if (immutableStream != null && immutableStream != mutableStream)
        {
          immutableStream.Dispose();
        }
      }
    }

    /// <summary>
    /// Supports taking a mutable version of a piece of data and transforming it into the immutable form if not present in the immutable cache
    /// </summary>
    /// <param name="mutableCache"></param>
    /// <param name="immutableCache"></param>
    /// <param name="cacheKey"></param>
    /// <param name="streamType"></param>
    /// <returns></returns>
    protected void PerformSpatialImmutabilityConversion(IStorageProxyCache<ISubGridSpatialAffinityKey, byte[]> mutableCache,
      IStorageProxyCache<ISubGridSpatialAffinityKey, byte[]> immutableCache,
      ISubGridSpatialAffinityKey cacheKey,
      FileSystemStreamType streamType)
    {
      if (mutableCache == null || immutableCache == null)
      {
        return;
      }

      using (MemoryStream MS = new MemoryStream(mutableCache.Get(cacheKey)), mutableStream = MemoryStreamCompression.Decompress(MS))
      {
        PerformSpatialImmutabilityConversion(mutableStream, immutableCache, cacheKey, streamType, null);
      }
    }

    /// <summary>
    /// Supports taking a mutable version of a piece of data and transforming it into the immutable form if not present in the immutable cache
    /// </summary>
    /// <param name="mutableStream"></param>
    /// <param name="immutableCache"></param>
    /// <param name="cacheKey"></param>
    /// <param name="streamType"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    protected void PerformSpatialImmutabilityConversion(MemoryStream mutableStream,
      IStorageProxyCache<ISubGridSpatialAffinityKey, byte[]> immutableCache,
      ISubGridSpatialAffinityKey cacheKey,
      FileSystemStreamType streamType,
      object source)
    {
      if (mutableStream == null || immutableCache == null)
      {
        return;
      }

      MemoryStream immutableStream = null;

      try
      {
        // Convert from the mutable to the immutable form and store it into the immutable cache
        if (MutabilityConverter.ConvertToImmutable(streamType, mutableStream, source, out immutableStream) &&
            immutableStream != null)
        {
          using (var compressedStream = MemoryStreamCompression.Compress(immutableStream))
          {
            if (Log.IsTraceEnabled())
              Log.LogInformation(
                $"Putting key:{cacheKey} in {immutableCache.Name}, size:{immutableStream.Length} -> {compressedStream.Length}, ratio:{(compressedStream.Length / (1.0 * immutableStream.Length)) * 100}%");

            // Place the converted immutable item into the immutable cache
            immutableCache.Put(cacheKey, compressedStream.ToArray());
          }
        }
        else
        {
          // There was no immutable version of the requested information. Allow this to bubble up the stack...
          Log.LogError(
            $"MutabilityConverter.ConvertToImmutable failed to convert mutable data for streamType={streamType}");

          immutableStream = null;
        }
      }
      finally
      {
        if (immutableStream != null && immutableStream != mutableStream)
        {
          immutableStream.Dispose();
        }
      }
    }
  }
}
