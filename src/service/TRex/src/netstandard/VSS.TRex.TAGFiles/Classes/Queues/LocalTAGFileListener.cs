using System;
using Apache.Ignite.Core.Cache.Event;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    public class LocalTAGFileListener : ICacheEntryEventListener<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>
    {
        private static readonly ILogger _log = Logging.Logger.CreateLogger<LocalTAGFileListener>();

        private readonly TAGFileBufferQueueItemHandler _handler;

        private bool _outputInformationalMessagesToLog = false;

        public LocalTAGFileListener(TAGFileBufferQueueItemHandler handler)
        {
            this._handler = handler;
        }

        /// <summary>
        /// Event called whenever there are new items in the TAG file buffer queue discovered by the continuous query
        /// Events include creation, modification and deletion of cache entries
        /// </summary>
        /// <param name="events"></param>
        public void OnEvent(IEnumerable<ICacheEntryEvent<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>> events)
        {
            // Add the keys for the given events into the Project/Asset mapping buckets ready for a processing context
            // to acquire them. 

            // Log.LogInformation("About to add TAG file items to the grouper");
            var countOfCreatedEvents = 0;

            foreach (var evt in events)
            {
                // Only interested in newly added items to the cache. Updates and deletes are ignored.
                if (evt.EventType != CacheEntryEventType.Created)
                    continue;

                countOfCreatedEvents++;
                try
                {
                    _handler.Add(evt.Key);
                    if (_outputInformationalMessagesToLog)
                      _log.LogInformation($"#Progress# Added TAG file item [{evt.Key}] to the grouper");
                }
                catch (Exception e)
                {
                    _log.LogError(e, $"Exception occurred adding TAG file item {evt.Key} to the grouper");
                }
            }

            if (countOfCreatedEvents > 0)
            {
              if (_outputInformationalMessagesToLog)
                _log.LogInformation($"#Progress# Added {countOfCreatedEvents} TAG file items to the grouper");
            }
        }
    }
}
