using System;
using Apache.Ignite.Core.Cache.Event;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;

namespace VSS.TRex.SiteModelChangeMaps.GridFabric.Queues
{
  public class LocalSiteModelChangeListener : ICacheEntryEventListener<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<LocalSiteModelChangeListener>();

    public readonly SiteModelChangeProcessorItemHandler Handler;

    private bool OutputInformationalMessagesToLog = true;

    public LocalSiteModelChangeListener(SiteModelChangeProcessorItemHandler handler)
    {
      Handler = handler ?? throw new ArgumentException("Listener must be supplied with a handler");
    }

    /// <summary>
    /// Event called whenever there are new items in the buffer queue discovered by the continuous query
    /// Events include creation, modification and deletion of cache entries
    /// </summary>
    /// <param name="events"></param>
    public void OnEvent(IEnumerable<ICacheEntryEvent<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>> events)
    {
      // Pass the item creation events to the handler for processing 

      foreach (var evt in events)
      {
        // Only interested in newly added items to the cache. Updates and deletes are ignored.
        if (evt.EventType != CacheEntryEventType.Created)
          continue;

        try
        {
          Handler.Add(evt);

          if (OutputInformationalMessagesToLog)
            Log.LogInformation($"Added item [{evt.Key}]");
        }
        catch (Exception e)
        {
          Log.LogError(e, $"Exception occurred adding item [{evt.Key}]");
        }
      }
    }
  }
}
