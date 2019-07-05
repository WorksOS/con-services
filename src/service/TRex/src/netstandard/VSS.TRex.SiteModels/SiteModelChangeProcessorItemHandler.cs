using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModels.GridFabric.Queues;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Types;

namespace VSS.TRex.SiteModels
{
  public class SiteModelChangeProcessorItemHandler
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SiteModelChangeProcessorItemHandler>();

    /// <summary>
    /// Takes a site model spatial change map and incorporates those changes in the changes for each machine in the
    /// site model.
    /// Once items are processed they are removed from the change map queue retirement queue.
    /// </summary>
    public bool Process(SiteModelChangeBufferQueueItem item)
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

        var storageProxy = DIContext.Obtain<ISiteModels>().PrimaryStorageProxy(StorageMutability.Immutable);
        var changeMapCache = storageProxy.ProjectMachineCache(FileSystemStreamType.SiteModelMachineElevationChangeMap);
        var ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Mutable);

        if (ignite == null)
        {
          Log.LogError("Failed to obtain ignite reference");
          return false;
        }

        if (storageProxy.ImmutableProxy == null)
        {
          Log.LogError("Immutable proxy not available in provided storage proxy. Aborting");
          return false;
        }

        var sw = Stopwatch.StartNew();

        storageProxy.Clear();

        // TODO: Implement change map integration into machine change maps
        // 0. Obtain transaction lock on change map
        // 1. Read record for machine
        // 2. Integrate new map
        // 3. Write record back to store
        // 4. Release transaction lock

        foreach (var machine in siteModel.Machines)
        {
          using (var tx = ignite.GetTransactions().TxStart())
          {
            var key = new SiteModelMachineAffinityKey(item.ProjectUID, machine.ID, FileSystemStreamType.SiteModelMachineElevationChangeMap);

            // Read the serialised change map from the cache
            var changeMapBytes = changeMapCache.Get(key);

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

            // Add the two of them together...
            currentMask.SetOp_OR(updateMask);

            // Commit the updated mask to the store
            changeMapCache.Put(key, currentMask.ToBytes());

            tx.Commit();
          }
        }

        // Commit all the change for this retiree group
        if (storageProxy.Commit(out int numDeleted, out int numUpdated, out long numBytesWritten))
        {
          Log.LogInformation($"Item processed from queue cache, requiring {numDeleted} deletions, {numUpdated} updates with {numBytesWritten} bytes written in {sw.Elapsed}");
        }
        else
        {
          Log.LogInformation("Commit failed");
          return false;
        }

        return true;
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception thrown while processing queued items:");
        throw;
      }
    }
  }
}
