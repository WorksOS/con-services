using System;
using Apache.Ignite.Core.Cache.Event;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.SiteModels.Interfaces.GridFabric.Queues;

namespace VSS.TRex.SiteModels.GridFabric.Queues
{
    public class LocalSiteModelChangeListener : ICacheEntryEventListener<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<LocalSiteModelChangeListener>();

        public readonly SiteModelChangeProcessorItemHandler Handler;

        private bool OutputInformationalMessagesToLog = false;

        public LocalSiteModelChangeListener(SiteModelChangeProcessorItemHandler handler)
        {
            Handler = handler;
        }

        /// <summary>
        /// Event called whenever there are new items in the buffer queue discovered by the continuous query
        /// Events include creation, modification and deletion of cache entries
        /// </summary>
        /// <param name="events"></param>
        public void OnEvent(IEnumerable<ICacheEntryEvent<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>> events)
        {
            // Add the keys for the given events into the Project/Asset mapping buckets ready for a processing context
            // to acquire them. 

            int countOfCreatedEvents = 0;

            foreach (var evt in events)
            {
                // Only interested in newly added items to the cache. Updates and deletes are ignored.
                if (evt.EventType != CacheEntryEventType.Created)
                    continue;

                countOfCreatedEvents++;
                try
                {
                    Handler.Process(evt.Value);

                    if (OutputInformationalMessagesToLog)
                      Log.LogInformation($"#Progress# Added item [{evt.Key}] to the grouper");
                }
                catch (Exception e)
                {
                    Log.LogError(e, $"Exception occurred adding item {evt.Key} to the grouper");
                }
            }

            if (countOfCreatedEvents > 0)
            {
              if (OutputInformationalMessagesToLog)
                Log.LogInformation($"#Progress# Added {countOfCreatedEvents} TAG file items to the grouper");
            }
        }
    }
}
