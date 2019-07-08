using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Types;

namespace VSS.TRex.SiteModels
{
  public class SiteModelChangeProcessorItemHandler : IDisposable
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SiteModelChangeProcessorItemHandler>();

    /// <summary>
    /// The interval between epochs where the service checks to see if there is anything to do
    /// </summary>
    private const int kQueueServiceCheckIntervalMS = 1000;

    private bool _aborted;

    /// <summary>
    /// The handler is initially inactive until all elements from the initial query scan are complete
    /// </summary>
    private bool _active;

    private readonly IIgnite _ignite;
    private readonly IStorageProxyFactory _storageProxyFactory;
    private readonly IStorageProxy _storageProxy;
    private readonly IStorageProxyCache<ISiteModelMachineAffinityKey, byte[]> _changeMapCache;

    private readonly ICache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem> _itemQueueCache;
    private readonly ConcurrentQueue<ICacheEntry<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>> _queue;

    public SiteModelChangeProcessorItemHandler()
    {
      _ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Immutable);

      if (_ignite == null)
      {
        Log.LogError("Failed to obtain immutable ignite reference");
      }

      _queue = new ConcurrentQueue<ICacheEntry<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>();
      _itemQueueCache = _ignite.GetCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>(TRexCaches.SiteModelChangeBufferQueueCacheName());
      _storageProxyFactory = DIContext.Obtain<IStorageProxyFactory>();
      _storageProxy = _storageProxyFactory.ImmutableGridStorage();
      _changeMapCache = _storageProxy.ProjectMachineCache(FileSystemStreamType.SiteModelMachineElevationChangeMap);

      var _ = Task.Factory.StartNew(ProcessChangeMaoUpdateItems, TaskCreationOptions.LongRunning);
    }

    public void Activate()
    {
      _active = true;
    }

    public void Add(ICacheEntry<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem> item)
    {
      _queue.Enqueue(item);
      // Todo: Wake up the processor here, rather than allowing it to sleep for a fixed period of time
    }

    /// <summary>
    /// A version of ProcessTAGFilesFromGrouper2 that uses task parallelism
    /// </summary>
    private void ProcessChangeMaoUpdateItems()
    {
      try
      {
        Log.LogInformation("#In# ProcessChangeMaoUpdateItems starting executing");

        // Cycle looking for new work to until aborted...
        do
        {
          // Check to see if there is an item to be processed
          if (_active && _queue.TryDequeue(out var item))
          {
            Log.LogInformation($"Extracted item from queue, ProjectUID:{item.Value.ProjectUID}, added at {item.Value.InsertUTC} in thread {Thread.CurrentThread.ManagedThreadId}");

            if (ProcessItem(item.Value))
            {
              // Remove the item from the cache
              if (!_itemQueueCache.Remove(item.Key))
              {
                Log.LogError($"Failed to remove queued change map update item with key: Project = {item.Value.ProjectUID}, insert date = {item.Value.InsertUTC}");
              }
            }
          }
          else
          {
            Thread.Sleep(kQueueServiceCheckIntervalMS);
          }
        } while (!_aborted);

        Log.LogInformation("#Out# ProcessChangeMaoUpdateItems completed executing");
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception thrown in ProcessChangeMaoUpdateItems");
      }
    }

    /// <summary>
    /// Takes a site model spatial change map and incorporates those changes in the changes for each machine in the
    /// site model.
    /// Once items are processed they are removed from the change map queue retirement queue.
    /// </summary>
    private bool ProcessItem(SiteModelChangeBufferQueueItem item)
    {
      try
      {
        if (item == null)
        {
          Log.LogError("Item supplied to queue processor is null. Aborting");
          return false;
        }

        if (item.Content == null)
        {
          Log.LogError("Item supplied to queue processor has no internal content. Aborting");
          return false;
        }

        var siteModels = DIContext.Obtain<ISiteModels>();
        var siteModel = siteModels.GetSiteModel(item.ProjectUID);

        if (siteModel == null)
        {
          Log.LogError($"Site model {item.ProjectUID} does not exist. Aborting");
          return false;
        }

        // Create a new storage proxy to perform the update for this item with


        var sw = Stopwatch.StartNew();

        // Implement change map integration into machine change maps
        // 0. Obtain transaction (will create implicit locks on items)
        // 1. Read record for machine
        // 2. Integrate new map
        // 3. Write record back to store
        // 4. Commit transaction

        using (var tx = _storageProxy.StartTransaction())
        {
          foreach (var machine in siteModel.Machines)
          {
            var key = new SiteModelMachineAffinityKey(item.ProjectUID, machine.ID, FileSystemStreamType.SiteModelMachineElevationChangeMap);

            // Read the serialized change map from the cache
            var changeMapBytes = _changeMapCache.Get(key);

            //using (var mask = new SubGridTreeSubGridExistenceBitMask())
            var currentMask = new SubGridTreeSubGridExistenceBitMask();

            // If there is an existing change map for the machine then read its contents in the newly created bitmask sub grid tree
            if (changeMapBytes != null)
            {
              currentMask.FromBytes(changeMapBytes);
            }

            // Extract the change map from the item  
            //using (var updateMask = new SubGridTreeSubGridExistenceBitMask())
            var updateMask = new SubGridTreeSubGridExistenceBitMask();

            updateMask.FromBytes(item.Content);

            switch (item.Operation)
            {
              case SiteModelChangeMapOperation.AddSpatialChanges: // Add the two of them together...
                currentMask.SetOp_OR(updateMask);
                break;

              case SiteModelChangeMapOperation.RemoveSpatialChanges: // Subtract from the change map...
                currentMask.SetOp_ANDNOT(updateMask);
                break;

              default:
                Log.LogError($"Unknown operation encountered: {(int) item.Operation}");
                break;
            }

            // Commit the updated mask to the store
            _changeMapCache.Put(key, currentMask.ToBytes());
          }
          
          if (_storageProxy.Commit(tx, out var numDeleted, out var numUpdated, out var numBytesWritten))
          {
            Log.LogInformation($"Item processed from queue cache, requiring {numDeleted} deletions, {numUpdated} updates with {numBytesWritten} bytes written in {sw.Elapsed}");
          }
          else
          {
            Log.LogInformation("Commit failed");
            return false;
          }
        }

        return true;
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception thrown while processing queued items:");
        throw;
      }
    }

    public void Dispose()
    {
      _aborted = true;
    }
  }
}
