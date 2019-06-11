using System;
using Apache.Ignite.Core;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Storage
{
  /// <summary>
  /// Implementation of the IStorageProxy interface that provides read through for items covered by the storage proxy
  /// but which buffers all writes (enlists them in a transaction) until commanded to flush the writes to Ignite in a
  /// single transacted PutAll().
  /// Note: All read and write operations are sending and receiving MemoryStream objects.
  /// </summary>
  public class StorageProxy_Ignite_Transactional : StorageProxy_Ignite
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<StorageProxy_Ignite_Transactional>();

    /// <summary>
    /// Constructor that obtains references to the mutable and immutable, spatial and non-spatial caches present in the grid
    /// </summary>
    /// <param name="mutability"></param>
    public StorageProxy_Ignite_Transactional(StorageMutability mutability) : base(mutability)
    {
      EstablishCaches();
    }

    /// <summary>
    /// Creates transactional storage proxies to be used by the consuming client
    /// </summary>
    private void EstablishCaches()
    {
      spatialCache = DIContext.Obtain<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCacheTransacted<ISubGridSpatialAffinityKey, byte[]>>>()(ignite, Mutability, FileSystemStreamType.SubGridDirectory);
      generalNonSpatialCache = DIContext.Obtain<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCacheTransacted<INonSpatialAffinityKey, byte[]>>>()(ignite, Mutability, FileSystemStreamType.SubGridDirectory);
      siteModelCache = DIContext.Obtain<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCacheTransacted<INonSpatialAffinityKey, byte[]>>>()(ignite, Mutability, FileSystemStreamType.ProductionDataXML);
    }

    /// <summary>
    /// Commits all unsaved changes in the spatial and non-spatial stores. Each store is committed asynchronously.
    /// </summary>
    /// <returns></returns>
    public override bool Commit(out int numDeleted, out int numUpdated, out long numBytesWritten)
    {
      numDeleted = 0;
      numUpdated = 0;
      numBytesWritten = 0;

      try
      {
        spatialCache.Commit(out int _numDeleted, out int _numUpdated, out long _numBytesWritten);
        numDeleted += _numDeleted;
        numUpdated += _numUpdated;
        numBytesWritten += _numBytesWritten;
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception thrown committing changes to Ignite for spatial cache");
        throw;
      }

      try
      {
        generalNonSpatialCache.Commit(out int _numDeleted, out int _numUpdated, out long _numBytesWritten);
        numDeleted += _numDeleted;
        numUpdated += _numUpdated;
        numBytesWritten += _numBytesWritten;
      }
      catch 
      {
        Log.LogError("Exception thrown committing changes to Ignite for general non spatial cache");
        throw;
      }

      try
      {
        siteModelCache.Commit(out int _numDeleted, out int _numUpdated, out long _numBytesWritten);
        numDeleted += _numDeleted;
        numUpdated += _numUpdated;
        numBytesWritten += _numBytesWritten;
      }
      catch
      {
        Log.LogError("Exception thrown committing changes to Ignite for site model cache");
        throw;
      }

      /*
  var commitTasks = new List<Task<(int _numDeleted, int _numUpdated, long _numBytesWritten)>>
  {
    Task.Factory.Run(() =>
    {
      try
      {
        spatialCache.Commit(out int _numDeleted, out int _numUpdated, out long _numBytesWritten);
        return (_numDeleted, _numUpdated, _numBytesWritten);
      }
      catch 
      {
        Log.LogError("Exception thrown committing changes to Ignite for spatial cache");
        throw;
      }
    }),
    Task.Factory.Run(() =>
    {
      try
      {
        generalNonSpatialCache.Commit(out int _numDeleted, out int _numUpdated, out long _numBytesWritten);
        return (_numDeleted, _numUpdated, _numBytesWritten);
      }
      catch
      {
        Log.LogError("Exception thrown committing changes to Ignite for general non spatial cache");
        throw;
      }
    }),
    Task.Factory.Run(() =>
    {
      try
      {
        siteModelCache.Commit(out int _numDeleted, out int _numUpdated, out long _numBytesWritten);
        return (_numDeleted, _numUpdated, _numBytesWritten);
      }
      catch
      {
        Log.LogError("Exception thrown committing changes to Ignite for site model cache");
        throw;
      }
    })
  };

  var commitResults = commitTasks.WhenAll();
  commitResults.Wait();

  if (commitResults.IsFaulted || commitTasks.Any(x => x.IsFaulted))
    return false;

  foreach (var (_numDeleted, _numUpdated, _numBytesWritten) in commitResults.Result)
  {
    numDeleted += _numDeleted;
    numUpdated += _numUpdated;
    numBytesWritten += _numBytesWritten;
  }
  */

      return ImmutableProxy?.Commit() ?? true;
    }

    public override bool Commit() => Commit(out _, out _, out _);

    /// <summary>
    /// Clears all changes in the spatial and non spatial stores
    /// </summary>
    public override void Clear()
    {
      try
      {
        spatialCache.Clear();
      }
      catch
      {
        Log.LogError("Exception thrown clearing changes for spatial cache");
        throw;
      }

      try
      {
        generalNonSpatialCache.Clear();
      }
      catch
      {
        Log.LogError("Exception thrown clearing changes for general non spatial cache");
        throw;
      }

      try
      {
        siteModelCache.Clear();
      }
      catch
      {
        Log.LogError("Exception thrown clearing changes for site model cache");
        throw;
      }

      ImmutableProxy?.Clear();
    }
  }
}
