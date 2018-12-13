using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core.Cache;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
  public class SegmentRetirementQueueItemHandler
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SegmentRetirementQueueItemHandler>();

    /// <summary>
    /// Takes a set of segment retirees and removes them from grid storage in both the mutable grid (the 'local' grid) and
    /// the immutable grid (that it is a client of).
    /// Once items are successfully removed from storage (or are no longer contained in storage) they are removed from the retirement queue.
    /// </summary>
    /// <param name="storageProxy"></param>
    /// <param name="cache"></param>
    /// <param name="retirees"></param>
    public bool Process(IStorageProxy storageProxy, ICache<ISegmentRetirementQueueKey, SegmentRetirementQueueItem> cache, IEnumerable<SegmentRetirementQueueItem> retirees)
    {
      try
      {
        int count = 0;
        storageProxy.Clear();

        // Process all entries in the retirees list, removing each in turn from the cache.
        if (cache == null)
        {
          Log.LogError($"Cache supplied to segment retirement queue processor is null. {retirees.Count()} retirement groups are pending removal. Aborting");
          return false;
        }

        if (retirees == null)
        {
          Log.LogError("Retirees list supplied to segment retirement queue processor is null. Aborting");
          return false;
        }

        if (storageProxy.ImmutableProxy == null)
        {
          Log.LogError("Immutable proxy not available in provided storage proxy. Aborting");
          return false;
        }

        foreach (var group in retirees)
        {
          if (group == null)
          {
            Log.LogError("Retirees list supplied to segment retirement queue processor contains null items. Aborting");
            return false;
          }

          if (group.SegmentKeys == null || group.SegmentKeys.Length == 0)
          {
            Log.LogError("Retiree groups segment keys list is null or empty. Aborting");
            return false;
          }

          count += group.SegmentKeys.Length;

          Log.LogInformation($"Retiring a group containing {group.SegmentKeys.Length} keys");
          foreach (var key in group.SegmentKeys)
          {
            Log.LogInformation($"About to retire {key}");

            if (!storageProxy.SpatialCache.Remove(key))
            {
              Log.LogError($"Mutable segment retirement cache removal for {key} returned false, aborting");
              return false;
            }

            if (storageProxy.ImmutableProxy == null)
            {
              Log.LogError("Immutable proxy not available in provided storage proxy. Aborting");
              return false;
            }

            if (!storageProxy.ImmutableProxy.SpatialCache.Remove(key))
            {
              Log.LogError($"Immutable segment retirement cache removal for {key} returned false, aborting");
              return false;
            }
          }
        }

        Log.LogInformation($"Prepared {count} retires for removal");
        DateTime startTime = DateTime.Now;

        // Commit all the deletes for this retiree group
        if (storageProxy.Commit(out int numDeleted, out int numUpdated, out long numBytesWritten))
        {
          Log.LogInformation($"{count} retirees removed from queue cache, requiring {numDeleted} deletions, {numUpdated} updates with {numBytesWritten} bytes written in {DateTime.Now - startTime}");
        }
        else
        {
          Log.LogInformation("Segment retirement commit failed");
          return false;
        }

        return true;
      }
      catch (Exception e)
      {
        Log.LogError("Exception thrown while retiring segments:", e);
        throw;
      }
    }
  }
}
