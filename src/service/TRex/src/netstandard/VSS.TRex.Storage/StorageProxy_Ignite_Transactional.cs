using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.DI;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;

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

    private static readonly bool UseAsyncTasksForStorageProxyIgniteTransactionalCommits = DIContext.Obtain<IConfigurationStore>()
      .GetValueBool("TREX_USE_SYNC_TASKS_FOR_STORAGE_PROXY_IGNITE_TRANSACTIONAL_COMMITS", true);
    /// <summary>
    /// Constructor that obtains references to the mutable and immutable, spatial and non-spatial caches present in the grid
    /// </summary>
    /// <param name="mutability"></param>
    public StorageProxy_Ignite_Transactional(StorageMutability mutability) : base(mutability)
    {
    }

    private bool CommitAsync(out int numDeleted, out int numUpdated, out long numBytesWritten)
    {
      numDeleted = 0;
      numUpdated = 0;
      numBytesWritten = 0;

      (int, int, long) LocalCommit(IStorageProxyCacheCommit committer)
      {
        try
        {
          spatialCache.Commit(out var numDeletedLocal, out var numUpdatedLocal, out var numBytesWrittenLocal);
          return (numDeletedLocal, numUpdatedLocal, numBytesWrittenLocal);
        }
        catch (Exception e)
        {
          Log.LogError(e, $"Exception thrown committing changes to Ignite for {committer.Name}");
          throw;
        }
      }

      var commitTasks = new List<Task<(int numDeletedLocal, int numUpdatedLocal, long numBytesWrittenLocal)>>
        {
          Task.Factory.Run(() => LocalCommit(spatialCache)),
          Task.Factory.Run(() => LocalCommit(generalNonSpatialCache)),
          Task.Factory.Run(() => LocalCommit(siteModelCache)),
          Task.Factory.Run(() => LocalCommit(spatialDataExistenceMapCache)),
          Task.Factory.Run(() => LocalCommit(siteModelMachineCache))
        };

      var commitResults = commitTasks.WhenAll();
      commitResults.Wait();

      if (commitResults.IsFaulted || commitTasks.Any(x => x.IsFaulted))
        return false;

      foreach (var (numDeletedLocal, numUpdatedLocal, numBytesWrittenLocal) in commitResults.Result)
      {
        numDeleted += numDeletedLocal;
        numUpdated += numUpdatedLocal;
        numBytesWritten += numBytesWrittenLocal;
      }

      return true;
    }

    private bool CommitSync(out int numDeleted, out int numUpdated, out long numBytesWritten)
    {
      var numDeletedLocal = 0;
      var numUpdatedLocal = 0;
      long numBytesWrittenLocal = 0;

      void LocalCommit(IStorageProxyCacheCommit committer)
      {
        try
        {
          spatialCache.Commit(out var numDeletedInternal, out var numUpdatedInternal, out var numBytesWrittenInternal);
          numDeletedLocal += numDeletedInternal;
          numUpdatedLocal += numUpdatedInternal;
          numBytesWrittenLocal += numBytesWrittenInternal;
        }
        catch (Exception e)
        {
          Log.LogError(e, $"Exception thrown committing changes to Ignite for {committer.Name}");
          throw;
        }
      }

      LocalCommit(spatialCache);
      LocalCommit(generalNonSpatialCache);
      LocalCommit(siteModelCache);
      LocalCommit(spatialDataExistenceMapCache);
      LocalCommit(siteModelMachineCache);

      numDeleted = numDeletedLocal;
      numUpdated = numUpdatedLocal;
      numBytesWritten = numBytesWrittenLocal;
      return true;
    }


    /// <summary>
    /// Commits all unsaved changes in the spatial and non-spatial stores. Each store is committed asynchronously.
    /// </summary>
    /// <returns></returns>
    public override bool Commit(out int numDeleted, out int numUpdated, out long numBytesWritten)
    {
      var commitOk = UseAsyncTasksForStorageProxyIgniteTransactionalCommits 
        ? CommitAsync(out numDeleted, out numUpdated, out numBytesWritten) 
        : CommitSync(out numDeleted, out numUpdated, out numBytesWritten);

      return commitOk && (ImmutableProxy?.Commit() ?? true);
    }

    public override bool Commit() => Commit(out _, out _, out _);

    /// <summary>
    /// Clears all changes in the spatial and non spatial stores
    /// </summary>
    public override void Clear()
    {
      void LocalClear(IStorageProxyCacheCommit committer)
      {
        try
        {
          committer.Clear();
        }
        catch
        {
          Log.LogError($"Exception thrown clearing changes for cache {committer.Name}");
          throw;
        }
      }

      LocalClear(SpatialCache);
      LocalClear(generalNonSpatialCache);
      LocalClear(siteModelCache);
      LocalClear(spatialDataExistenceMapCache);
      LocalClear(siteModelMachineCache);
      
      ImmutableProxy?.Clear();
    }
  }
}
