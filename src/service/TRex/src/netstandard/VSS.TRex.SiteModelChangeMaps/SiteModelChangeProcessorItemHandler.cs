using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Apache.Ignite.Core.Cache;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
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

    public bool Aborted { get; private set; }

    /// <summary>
    /// The handler is initially inactive until all elements from the initial query scan are complete
    /// </summary>
    public bool Active { get; private set; }

    private readonly SiteModelChangeMapProxy _changeMapProxy;

    private readonly IStorageProxyCache<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem> _itemQueueCache;

    private readonly ConcurrentQueue<ICacheEntry<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>> _queue;

    private readonly EventWaitHandle _waitHandle;

    public SiteModelChangeProcessorItemHandler()
    {
      var ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Immutable);

      if (ignite == null)
      {
        throw new TRexException("Failed to obtain immutable Ignite reference");
      }

      _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
      _queue = new ConcurrentQueue<ICacheEntry<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>>();
      _itemQueueCache = DIContext.Obtain<Func<IStorageProxyCache<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>>>()();
      _changeMapProxy = new SiteModelChangeMapProxy();

      Log.LogInformation("Starting site model change processor item handler task");
      var _ = Task.Factory.StartNew(ProcessChangeMapUpdateItems, TaskCreationOptions.LongRunning);
    }

    public int QueueCount => _queue.Count;

    public void Activate()
    {
      Active = true;
      _waitHandle.Set();
    }

    public void Abort()
    {
      Aborted = true;
    }

    public void Add(ICacheEntry<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem> item)
    {
      _queue.Enqueue(item);
      _waitHandle.Set();
    }

    /// <summary>
    /// A version of ProcessTAGFilesFromGrouper2 that uses task parallelism
    /// </summary>
    private void ProcessChangeMapUpdateItems()
    {
      Log.LogInformation($"#In# {nameof(ProcessChangeMapUpdateItems)} starting executing");

      // Cycle looking for new work to until aborted...
      do
      {
        try
        {
          // Check to see if there is an item to be processed
          if (Active && _queue.TryDequeue(out var item))
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
        }
        catch (Exception e)
        {
          Log.LogError(e, $"Exception thrown in {nameof(ProcessChangeMapUpdateItems)}");
        }
      } while (!Aborted);

      Log.LogInformation($"#Out# {nameof(ProcessChangeMapUpdateItems)} completed executing");
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
          Log.LogError($"Site model {item.ProjectUID} does not exist [deleted?]. Aborting");
          return false;
        }

        // Implement change map integration into machine change maps
        // 0. Obtain transaction (will create implicit locks on items)
        // 1. Read record for machine
        // 2. Integrate new map
        // 3. Write record back to store
        // 4. Commit transaction

        Log.LogInformation($"Processing an item: {item.Operation}, project:{item.ProjectUID}, machine:{item.MachineUid}");
        var sw = Stopwatch.StartNew();

        switch (item.Operation)
        {
          case SiteModelChangeMapOperation.AddSpatialChanges: // Add the two of them together...
          {
            // Add the spatial change to every machine in the site model
            foreach (var machine in siteModel.Machines)
            {
              using (var l = _changeMapProxy.Lock(item.ProjectUID, machine.ID))
              {
                l.Enter();
                try
                {
                  var currentMask = _changeMapProxy.Get(item.ProjectUID, machine.ID);
                  if (currentMask == null)
                  {
                    currentMask = new SubGridTreeSubGridExistenceBitMask();
                    currentMask.SetOp_OR(siteModel.ExistenceMap);
                  }

                  // Extract the change map from the item  
                  var updateMask = new SubGridTreeSubGridExistenceBitMask();

                  updateMask.FromBytes(item.Content);
                  currentMask.SetOp_OR(updateMask);
                  _changeMapProxy.Put(item.ProjectUID, machine.ID, currentMask);
                }
                finally
                {
                  l.Exit();
                }
              }
            }

            break;
          }

          case SiteModelChangeMapOperation.RemoveSpatialChanges: // Subtract from the change map...
          {
            using (var l = _changeMapProxy.Lock(item.ProjectUID, item.MachineUid))
            {
              l.Enter();
              try
              {
                // Remove the spatial change only from the machine the made the query
                var currentMask = _changeMapProxy.Get(item.ProjectUID, item.MachineUid);

                if (currentMask != null)
                {
                  // Extract the change map from the item  
                  var updateMask = new SubGridTreeSubGridExistenceBitMask();

                  currentMask.SetOp_ANDNOT(updateMask);
                  _changeMapProxy.Put(item.ProjectUID, item.MachineUid, currentMask);
                }
              }
              finally
              {
                l.Exit();
              }
            }

            break;
          }

          default:
            Log.LogError($"Unknown operation encountered: {(int) item.Operation}");
            return false;
        }

        Log.LogInformation($"Completed processing an item in {sw.Elapsed}: {item.Operation}, project:{item.ProjectUID}, machine:{item.MachineUid}");

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
      Aborted = true;
    }
  }
}
