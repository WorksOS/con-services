using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Apache.Ignite.Core.Cache;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
using VSS.TRex.SiteModelChangeMaps.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.SiteModelChangeMaps
{
  public class SiteModelChangeProcessorItemHandler : IDisposable
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SiteModelChangeProcessorItemHandler>();

    private bool _aborted;
    public bool Aborted => _aborted;

    /// <summary>
    /// The handler is initially inactive until all elements from the initial query scan are complete
    /// </summary>
    private bool _active;
    public bool Active => _active;

    private readonly IStorageProxy _storageProxy;
    private readonly SiteModelChangeMapProxy _changeMapProxy;

    private readonly IStorageProxyCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem> _itemQueueCache;

    private readonly ConcurrentQueue<ICacheEntry<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>> _queue;

    private readonly EventWaitHandle _waitHandle;

    public SiteModelChangeProcessorItemHandler()
    {
      var ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Immutable);

      if (ignite == null)
      {
        throw new TRexException("Failed to obtain immutable Ignite reference");
      }

      _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

      _queue = new ConcurrentQueue<ICacheEntry<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>();
      _itemQueueCache = DIContext.Obtain<IStorageProxyCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>();

      _changeMapProxy = new SiteModelChangeMapProxy();
      _storageProxy = DIContext.Obtain<IStorageProxyFactory>().ImmutableGridStorage();

      var _ = Task.Factory.StartNew(ProcessChangeMapUpdateItems, TaskCreationOptions.LongRunning);
    }

    public int QueueCount => _queue.Count;

    public void Activate()
    {
      _active = true;
      _waitHandle.Set();
    }

    public void Abort()
    {
      _aborted = true;
    }

    public void Add(ICacheEntry<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem> item)
    {
      _queue.Enqueue(item);
      _waitHandle.Set();
    }

    /// <summary>
    /// A version of ProcessTAGFilesFromGrouper2 that uses task parallelism
    /// </summary>
    private void ProcessChangeMapUpdateItems()
    {
      try
      {
        Log.LogInformation($"#In# {nameof(ProcessChangeMapUpdateItems)} starting executing");

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
            _waitHandle.WaitOne();
          }
        } while (!_aborted);

        Log.LogInformation($"#Out# {nameof(ProcessChangeMapUpdateItems)} completed executing");
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Exception thrown in {nameof(ProcessChangeMapUpdateItems)}");
      }
    }

    /// <summary>
    /// Takes a site model spatial change map and incorporates those changes in the changes for each machine in the
    /// site model.
    /// Once items are processed they are removed from the change map queue retirement queue.
    /// </summary>
    private bool ProcessItem(ISiteModelChangeBufferQueueItem item)
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

        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(item.ProjectUID);

        if (siteModel == null)
        {
          Log.LogError($"Site model {item.ProjectUID} does not exist. Aborting");
          return false;
        }

        // Implement change map integration into machine change maps
        // 0. Obtain transaction (will create implicit locks on items)
        // 1. Read record for machine
        // 2. Integrate new map
        // 3. Write record back to store
        // 4. Commit transaction

        using (var tx = _storageProxy.StartTransaction())
        {
          switch (item.Operation)
          {
            case SiteModelChangeMapOperation.AddSpatialChanges: // Add the two of them together...
            {
              // Add the spatial change to every machine in the site model
              foreach (var machine in siteModel.Machines)
              {
                var currentMask = _changeMapProxy.Get(item.ProjectUID, machine.ID);
                if (currentMask == null)
                {
                  currentMask = new SubGridTreeSubGridExistenceBitMask();
                  currentMask.SetOp_OR(siteModel.ExistenceMap);
                }

                // Extract the change map from the item  
                //using (var updateMask = new SubGridTreeSubGridExistenceBitMask())
                var updateMask = new SubGridTreeSubGridExistenceBitMask();

                updateMask.FromBytes(item.Content);
                currentMask.SetOp_OR(updateMask);
                _changeMapProxy.Put(item.ProjectUID, machine.ID, currentMask);
              }
              break;
            }

            case SiteModelChangeMapOperation.RemoveSpatialChanges: // Subtract from the change map...
            {
              // Remove the spatial change only from the machine the made the query
              var currentMask = _changeMapProxy.Get(item.ProjectUID, item.MachineUid);

              if (currentMask != null)
              {
                // Extract the change map from the item  
                //using (var updateMask = new SubGridTreeSubGridExistenceBitMask())
                var updateMask = new SubGridTreeSubGridExistenceBitMask();

                currentMask.SetOp_ANDNOT(updateMask);
                _changeMapProxy.Put(item.ProjectUID, item.MachineUid, currentMask);
              }
              break;
            }

            default:
              Log.LogError($"Unknown operation encountered: {(int)item.Operation}");
              return false;
          }

          _storageProxy.Commit(tx);
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
