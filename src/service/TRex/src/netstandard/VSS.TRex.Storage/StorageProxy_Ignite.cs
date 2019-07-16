using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Transactions;
using VSS.Serilog.Extensions;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
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
      spatialCache = DIContext.Obtain<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCache<ISubGridSpatialAffinityKey, byte[]>>>()(ignite, Mutability, FileSystemStreamType.SubGridDirectory);
      generalNonSpatialCache = DIContext.Obtain<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCache<INonSpatialAffinityKey, byte[]>>>()(ignite, Mutability, FileSystemStreamType.SubGridDirectory);
      siteModelCache = DIContext.Obtain<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCache<INonSpatialAffinityKey, byte[]>>>()(ignite, Mutability, FileSystemStreamType.ProductionDataXML);
      siteModelMachineCache = DIContext.Obtain<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCache<ISiteModelMachineAffinityKey, byte[]>>>()(ignite, Mutability, FileSystemStreamType.SiteModelMachineElevationChangeMap);
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
        var cacheKey = ComputeNamedStreamCacheKey(dataModelID, streamName);

        using (var compressedStream = MemoryStreamCompression.Compress(mutableStream))
        {
          if (Log.IsTraceEnabled())
            Log.LogInformation($"Putting key:{cacheKey} in {NonSpatialCache(streamType).Name}, size:{mutableStream.Length} -> {compressedStream.Length}, ratio:{(compressedStream.Length / (1.0 * mutableStream.Length)) * 100}%");
          NonSpatialCache(streamType).Put(cacheKey, compressedStream.ToArray());
        }

        try
        {
          // Create the immutable stream from the source data
          if (Mutability == StorageMutability.Mutable && ImmutableProxy != null)
          {
            if (!PerformNonSpatialImmutabilityConversion(mutableStream, ImmutableProxy.NonSpatialCache(streamType), cacheKey, streamType, source))
            {
              Log.LogError("Unable to project an immutable stream");
              return FileSystemErrorStatus.MutableToImmutableConversionError;
            }
          }
        }
        catch (Exception e)
        {
          Log.LogError(e, "Exception performing mutability conversion:");
          return FileSystemErrorStatus.MutableToImmutableConversionError;
        }

        return FileSystemErrorStatus.OK;
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Exception writing stream {streamName} to persistent store");
        return FileSystemErrorStatus.UnknownErrorWritingToFS;
      }
    }

    /// <summary>
    /// Supports writing a spatial data stream to the persistent store via the grid cache.
    /// </summary>
    /// <param name="dataModelID"></param>
    /// <param name="streamName"></param>
    /// <param name="subGridX"></param>
    /// <param name="subGridY"></param>
    /// <param name="segmentEndDateTicks"></param>
    /// <param name="version"></param>
    /// <param name="streamType"></param>
    /// <param name="mutableStream"></param>
    /// <param name="source"></param>
    /// <param name="segmentStartDateTicks"></param>
    /// <returns></returns>
    public FileSystemErrorStatus WriteSpatialStreamToPersistentStore(Guid dataModelID,
      string streamName,
      int subGridX, int subGridY,
      long segmentStartDateTicks, long segmentEndDateTicks,
      long version,
      FileSystemStreamType streamType,
      MemoryStream mutableStream,
      object source)
    {
      try
      {
        ISubGridSpatialAffinityKey cacheKey = new SubGridSpatialAffinityKey(version, dataModelID, subGridX, subGridY, segmentStartDateTicks, segmentEndDateTicks);

        using (var compressedStream = MemoryStreamCompression.Compress(mutableStream))
        {
          if (Log.IsTraceEnabled())
            Log.LogInformation($"Putting key:{cacheKey} in {spatialCache.Name}, size:{mutableStream.Length} -> {compressedStream.Length}, ratio:{(compressedStream.Length / (1.0 * mutableStream.Length)) * 100}%");
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
          Log.LogError(e, "Exception performing mutability conversion:");
          return FileSystemErrorStatus.MutableToImmutableConversionError;
        }

        return FileSystemErrorStatus.OK;
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Exception writing spatial stream {streamName} to persistent store");
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
        var cacheKey = ComputeNamedStreamCacheKey(dataModelID, streamName);

        //Log.LogInformation($"Getting key:{cacheKey}");

        try
        {
          using (var MS = new MemoryStream(NonSpatialCache(streamType).Get(cacheKey)))
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
        Log.LogError(e, "Exception occurred:");

        stream = null;
        return FileSystemErrorStatus.UnknownErrorReadingFromFS;
      }
    }

    /// <summary>
    /// Supports reading a stream of spatial data from the persistent store via the grid cache
    /// </summary>
    /// <param name="dataModelID"></param>
    /// <param name="streamName"></param>
    /// <param name="subGridX"></param>
    /// <param name="subGridY"></param>
    /// <param name="segmentStartDateTicks"></param>
    /// <param name="segmentEndDateTicks"></param>
    /// <param name="version"></param>
    /// <param name="streamType"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    public FileSystemErrorStatus ReadSpatialStreamFromPersistentStore(Guid dataModelID,
      string streamName,
      int subGridX, int subGridY,
      long segmentStartDateTicks,
      long segmentEndDateTicks,
      long version,
      FileSystemStreamType streamType,
      out MemoryStream stream)
    {
      stream = null;

      try
      {
        ISubGridSpatialAffinityKey cacheKey = new SubGridSpatialAffinityKey(version, dataModelID, subGridX, subGridY, segmentStartDateTicks, segmentEndDateTicks);

        //Log.LogInformation($"Getting key:{streamName}");

        try
        {
          using (var MS = new MemoryStream(spatialCache.Get(cacheKey)))
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
        Log.LogError(e, "Exception occurred:");

        stream = null;
        return FileSystemErrorStatus.UnknownErrorReadingFromFS;
      }
    }

    /// <summary>
    /// Supports removing a named stream from the persistent store via the grid cache
    /// </summary>
    /// <param name="dataModelID"></param>
    /// <param name="streamType"></param>
    /// <param name="streamName"></param>
    /// <returns></returns>
    public FileSystemErrorStatus RemoveStreamFromPersistentStore(Guid dataModelID,
      FileSystemStreamType streamType, 
      string streamName)
    {
      try
      {
        var cacheKey = ComputeNamedStreamCacheKey(dataModelID, streamName);

        if (Log.IsTraceEnabled())
          Log.LogInformation($"Removing key:{cacheKey}");

        // Remove item from both immutable and mutable caches
        try
        {
          NonSpatialCache(streamType).Remove(cacheKey);
        }
        catch (KeyNotFoundException E)
        {
          Log.LogError(E, "Exception occurred:");
        }

        ImmutableProxy?.RemoveStreamFromPersistentStore(dataModelID, streamType, streamName);

        return FileSystemErrorStatus.OK;
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Exception removing stream {streamName} from persistent store");
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
        throw new ArgumentException("Non-mutable storage proxy may not accept an immutable storage proxy reference");

      if (immutableProxy == null)
        throw new ArgumentException("Null immutable storage proxy reference supplied to SetImmutableStorageProxy()");

      if (immutableProxy.Mutability != StorageMutability.Immutable)
        throw new ArgumentException("Immutable storage proxy reference is not marked with Immutable mutability");

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
    /// Commits unsaved changes in the storage proxy in conjunction with an Ignite transaction mediating the activity
    /// No implementation for non-transactional storage proxy
    /// </summary>
    public virtual bool Commit(ITransaction tx, out int numDeleted, out int numUpdated, out long numBytesWritten)
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

    public bool Commit(ITransaction tx)
    {
      Commit();
      tx.Commit();

      return true;
    }

    public ITransaction StartTransaction(TransactionConcurrency concurrency, TransactionIsolation isolation)
    { 
      return (ignite ?? DIContext.Obtain<ITRexGridFactory>().Grid(Mutability))
        .GetTransactions()
        .TxStart(concurrency, isolation);
    }
  }
}
