using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.Storage.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.Storage
{
  /// <summary>
  /// Implementation of the IStorageProxy interface that allows to read/write operations against Ignite based IO support.
  /// Note: All read and write operations are sending and receiving MemoryStream objects.
  /// </summary>
  public class StorageProxy_Ignite : StorageProxy_IgniteBase, IStorageProxy
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<StorageProxy_Ignite>();

    /// <summary>
    /// The reference to a storage proxy representing the immutable data store derived from a mutable data store
    /// </summary>w
    public IStorageProxy ImmutableProxy { get; private set; }

    /// <summary>
    /// Constructor that obtains references to the mutable and immutable, spatial and non-spatial caches present in the grid
    /// </summary>
    /// <param name="mutability"></param>
    public StorageProxy_Ignite(StorageMutability mutability) : base(mutability)
    {
      EstablishCaches();
    }

    private void EstablishCaches()
    {
      spatialCache = new StorageProxyCache<ISubGridSpatialAffinityKey, byte[]>(
        ignite?.GetCache<ISubGridSpatialAffinityKey, byte[]>(TRexCaches.SpatialCacheName(Mutability)));
      nonSpatialCache =
        new StorageProxyCache<INonSpatialAffinityKey, byte[]>(
          ignite?.GetCache<INonSpatialAffinityKey, byte[]>(TRexCaches.NonSpatialCacheName(Mutability)));
    }

    /// <summary>
    /// Supports writing a named data stream to the persistent store via the grid cache.
    /// </summary>
    /// <param name="dataModelID"></param>
    /// <param name="streamName"></param>
    /// <param name="streamType"></param>
    /// <param name="mutableStream"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public FileSystemErrorStatus WriteStreamToPersistentStore(Guid dataModelID,
      string streamName,
      FileSystemStreamType streamType,
      MemoryStream mutableStream,
      object source)
    {
      try
      {
        INonSpatialAffinityKey cacheKey = ComputeNamedStreamCacheKey(dataModelID, streamName);

        using (MemoryStream compressedStream = MemoryStreamCompression.Compress(mutableStream))
        {
          // Log.LogInformation($"Putting key:{cacheKey} in {nonSpatialCache.Name}, size:{mutableStream.Length} -> {compressedStream.Length}");
          nonSpatialCache.Put(cacheKey, compressedStream.ToArray());
        }

        try
        {
          // Create the immutable stream from the source data
          if (Mutability == StorageMutability.Mutable && ImmutableProxy != null)
          {
            if (PerformNonSpatialImmutabilityConversion(mutableStream, ImmutableProxy.NonSpatialCache, cacheKey, streamType, source) == null)
            {
              Log.LogError("Unable to project an immutable stream");
              return FileSystemErrorStatus.MutableToImmutableConversionError;
            }
          }
        }
        catch (Exception e)
        {
          Log.LogError("Exception performing mutability conversion:", e);
          return FileSystemErrorStatus.MutableToImmutableConversionError;
        }

        return FileSystemErrorStatus.OK;
      }
      catch
      {
        return FileSystemErrorStatus.UnknownErrorWritingToFS;
      }
    }
    
    /// <summary>
    /// Supports writing a spatial data stream to the persistent store via the grid cache.
    /// </summary>
    /// <param name="dataModelID"></param>
    /// <param name="streamName"></param>
    /// <param name="subgridX"></param>
    /// <param name="subgridY"></param>
    /// <param name="segmentIdentifier"></param>
    /// <param name="streamType"></param>
    /// <param name="mutableStream"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public FileSystemErrorStatus WriteSpatialStreamToPersistentStore(Guid dataModelID,
      string streamName,
      uint subgridX, uint subgridY,
      string segmentIdentifier,
      FileSystemStreamType streamType,
      MemoryStream mutableStream,
      object source)
    {
      try
      {
        ISubGridSpatialAffinityKey cacheKey = new SubGridSpatialAffinityKey(dataModelID, subgridX, subgridY, segmentIdentifier);

        using (MemoryStream compressedStream = MemoryStreamCompression.Compress(mutableStream))
        {
          // Log.LogInformation($"Putting key:{cacheKey} in {spatialCache.Name}, size:{mutableStream.Length} -> {compressedStream.Length}");
          spatialCache.Put(cacheKey, compressedStream.ToArray());
        }

        // Convert the stream to the immutable form and write it to the immutable storage proxy
        try
        {
          if (Mutability == StorageMutability.Mutable && ImmutableProxy != null)
          {
            PerformSpatialImmutabilityConversion(mutableStream, ImmutableProxy.SpatialCache, cacheKey, streamType, source);
          }
        }
        catch (Exception e)
        {
          Log.LogError("Exception performing mutability conversion:", e);
          return FileSystemErrorStatus.MutableToImmutableConversionError;
        }

        return FileSystemErrorStatus.OK;
      }
      catch
      {
        return FileSystemErrorStatus.UnknownErrorWritingToFS;
      }
    }

    /// <summary>
    /// Supports reading a named stream from the persistent store via the grid cache
    /// </summary>
    /// <param name="dataModelID"></param>
    /// <param name="streamName"></param>
    /// <param name="streamType"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    public FileSystemErrorStatus ReadStreamFromPersistentStore(Guid dataModelID, string streamName, FileSystemStreamType streamType, out MemoryStream stream)
    {
      stream = null;

      try
      {
        INonSpatialAffinityKey cacheKey = ComputeNamedStreamCacheKey(dataModelID, streamName);

        //Log.LogInformation($"Getting key:{cacheKey}");

        try
        {
          using (MemoryStream MS = new MemoryStream(nonSpatialCache.Get(cacheKey)))
          {
            stream = MemoryStreamCompression.Decompress(MS);
            stream.Position = 0;
          }
        }
        catch (KeyNotFoundException)
        {
          return FileSystemErrorStatus.GranuleDoesNotExist;
        }

        return FileSystemErrorStatus.OK;
      }
      catch (Exception e)
      {
        Log.LogInformation("Exception occurred:", e);

        stream = null;
        return FileSystemErrorStatus.UnknownErrorReadingFromFS;
      }
    }

    /// <summary>
    /// Supports reading a stream of spatial data from the persistent store via the grid cache
    /// </summary>
    /// <param name="dataModelID"></param>
    /// <param name="streamName"></param>
    /// <param name="subgridX"></param>
    /// <param name="subgridY"></param>
    /// <param name="segmentIdentifier"></param>
    /// <param name="streamType"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    public FileSystemErrorStatus ReadSpatialStreamFromPersistentStore(Guid dataModelID,
      string streamName,
      uint subgridX, uint subgridY,
      string segmentIdentifier,
      FileSystemStreamType streamType,
      out MemoryStream stream)
    {
      stream = null;

      try
      {
        ISubGridSpatialAffinityKey cacheKey = new SubGridSpatialAffinityKey(dataModelID, subgridX, subgridY, segmentIdentifier);

        //Log.LogInformation($"Getting key:{streamName}");

        try
        {
          using (MemoryStream MS = new MemoryStream(spatialCache.Get(cacheKey)))
          {
            stream = MemoryStreamCompression.Decompress(MS);
            stream.Position = 0;
          }
        }
        catch (KeyNotFoundException)
        {
          return FileSystemErrorStatus.GranuleDoesNotExist;
        }

        return FileSystemErrorStatus.OK;
      }
      catch (Exception e)
      {
        Log.LogInformation("Exception occurred:", e);

        stream = null;
        return FileSystemErrorStatus.UnknownErrorReadingFromFS;
      }
    }

    /// <summary>
    /// Supports removing a named stream from the persistent store via the grid cache
    /// </summary>
    /// <param name="dataModelID"></param>
    /// <param name="streamName"></param>
    /// <returns></returns>
    public FileSystemErrorStatus RemoveStreamFromPersistentStore(Guid dataModelID, string streamName)
    {
      try
      {
        INonSpatialAffinityKey cacheKey = ComputeNamedStreamCacheKey(dataModelID, streamName);

        Log.LogInformation($"Removing key:{cacheKey}");

        // Remove item from both immutable and mutable caches
        try
        {
          nonSpatialCache.Remove(cacheKey);
        }
        catch (Exception E)
        {
          Log.LogError("Exception occurred:", E);
        }

        ImmutableProxy?.RemoveStreamFromPersistentStore(dataModelID, streamName);

        return FileSystemErrorStatus.OK;
      }
      catch
      {
        return FileSystemErrorStatus.UnknownErrorWritingToFS;
      }
    }

    /// <summary>
    /// Sets a reference to a storage proxy that proxies the immutable data store for this mutable data store
    /// </summary>
    /// <param name="immutableProxy"></param>
    public void SetImmutableStorageProxy(IStorageProxy immutableProxy)
    {
      if (Mutability != StorageMutability.Mutable)
      {
        throw new ArgumentException("Non-mutable storage proxy may not accept an immutable storage proxy reference");
      }

      if (immutableProxy == null)
      {
        throw new ArgumentException("Null immutable storage proxy reference supplied to SetImmutableStorageProxy()");
      }

      if (immutableProxy.Mutability != StorageMutability.Immutable)
      {
        throw new ArgumentException("Immutable storage proxy reference is not marked with Immutable mutability");
      }

      ImmutableProxy = immutableProxy;
    }

    /// <summary>
    /// Commits unsaved changes in the storage proxy.
    /// No implementation for non-transactional storage proxy
    /// </summary>
    public virtual bool Commit()
    {
      return true;
    }

    /// <summary>
    /// Commits unsaved changes in the storage proxy.
    /// No implementation for non-transactional storage proxy
    /// </summary>
    public virtual bool Commit(out int numDeleted, out int numUpdated, out long numBytesWritten)
    {
      numDeleted = -1;
      numUpdated = -1;
      numBytesWritten = -1;

      return true;
    }

    /// <summary>
    /// Clears changes in the storage proxy.
    /// No implementation for non-transactional storage proxy
    /// </summary>
    public virtual void Clear()
    {
    }
  }
}
