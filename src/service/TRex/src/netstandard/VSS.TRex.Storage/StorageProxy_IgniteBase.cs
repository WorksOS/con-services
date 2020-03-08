using System;
using Apache.Ignite.Core;
using Microsoft.Extensions.Logging;
using System.IO;
using VSS.Serilog.Extensions;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
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
    private IMutabilityConverter MutabilityConverter => _mutabilityConverter ??= DIContext.Obtain<IMutabilityConverter>();

    protected readonly IIgnite ignite;

    protected IStorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper> generalNonSpatialCache;
    protected IStorageProxyCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper> spatialCache;
    protected IStorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper> spatialDataExistenceMapCache;

    public IStorageProxyCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper> SpatialCache => spatialCache;

    protected IStorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper> siteModelCache;
    protected IStorageProxyCache<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper> siteModelMachineCache;


    /// <summary>
    /// Determines the correct cache to read/write particular types of information from/to
    /// </summary>
    /// <param name="streamType"></param>
    /// <returns></returns>
    public IStorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper> NonSpatialCache(FileSystemStreamType streamType)
    {
      return streamType switch
      {
        FileSystemStreamType.ProductionDataXML => siteModelCache,
        FileSystemStreamType.SubGridExistenceMap => spatialDataExistenceMapCache,
        _ => generalNonSpatialCache
      };
    }

    /// <summary>
    /// Determines the correct cache to read/write information related to a machines activities within a project
    /// that are not a part of the general schema representing the ingested production data from machine control systems.
    /// eg: THe site model elevation change maps used to request only that spatial information has changed since the
    /// last time a machine asked for that information from a project.
    /// </summary>
    /// <param name="streamType"></param>
    /// <returns></returns>
    public IStorageProxyCache<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper> ProjectMachineCache(FileSystemStreamType streamType)
    {
      return streamType switch
      {
        FileSystemStreamType.SiteModelMachineElevationChangeMap => siteModelMachineCache,
        _ => null
      };
    }

    /// <summary>
    /// Controls which grid (Mutable or Immutable) this storage proxy performs reads and writes against.
    /// </summary>
    public StorageMutability Mutability { get; set; }

    protected StorageProxy_IgniteBase(StorageMutability mutability)
    {
      Mutability = mutability;

      ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(mutability);
    }

    /// <summary>
    /// Computes the cache key name for a given data model and a given named stream within that datamodel
    /// </summary>
    /// <param name="dataModelId"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    protected static INonSpatialAffinityKey ComputeNamedStreamCacheKey(Guid dataModelId, string name) => new NonSpatialAffinityKey(dataModelId, name);

    /// <summary>
    /// Computes the cache key name for the given data model and a given spatial data stream within that datamodel
    /// </summary>
    /// <param name="dataModelId"></param>
    /// <param name="name"></param>
    /// <param name="subGridX"></param>
    /// <param name="subGridY"></param>
    /// <returns></returns>
    protected static string ComputeNamedStreamCacheKey(long dataModelId, string name, uint subGridX, uint subGridY)
    {
      return $"{dataModelId}-{name}-{subGridX}-{subGridY}";
    }

    /// <summary>
    /// Supports taking a mutable version of a piece of data and transforming it into the immutable form if not present in the immutable cache
    /// </summary>
    /// <param name="mutableCache"></param>
    /// <param name="immutableCache"></param>
    /// <param name="cacheKey"></param>
    /// <param name="streamType"></param>
    /// <returns></returns>
    protected bool PerformNonSpatialImmutabilityConversion(IStorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper> mutableCache,
      IStorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper> immutableCache,
      INonSpatialAffinityKey cacheKey,
      FileSystemStreamType streamType)
    {
      if (mutableCache == null || immutableCache == null)
      {
        return false;
      }

      using (var ms = new MemoryStream(mutableCache.Get(cacheKey).Bytes))
      {
        using (var mutableStream = MemoryStreamCompression.Decompress(ms))
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
      IStorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper> immutableCache,
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
            immutableCache.Put(cacheKey, new SerialisedByteArrayWrapper(compressedStream.ToArray()));

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
    protected void PerformSpatialImmutabilityConversion(IStorageProxyCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper> mutableCache,
      IStorageProxyCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper> immutableCache,
      ISubGridSpatialAffinityKey cacheKey,
      FileSystemStreamType streamType)
    {
      if (mutableCache == null || immutableCache == null)
      {
        return;
      }

      using MemoryStream ms = new MemoryStream(mutableCache.Get(cacheKey).Bytes), mutableStream = MemoryStreamCompression.Decompress(ms);
      PerformSpatialImmutabilityConversion(mutableStream, immutableCache, cacheKey, streamType, null);
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
      IStorageProxyCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper> immutableCache,
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
            immutableCache.Put(cacheKey, new SerialisedByteArrayWrapper(compressedStream.ToArray()));
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
