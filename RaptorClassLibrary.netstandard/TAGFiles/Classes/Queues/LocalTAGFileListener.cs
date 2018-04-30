using System;
using Apache.Ignite.Core.Cache.Event;
using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    public class LocalTAGFileListener : ICacheEntryEventListener<TAGFileBufferQueueKey, TAGFileBufferQueueItem>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Evnet called whenever there are new items in the TAG file buffer queue discovered by the continuous query
        /// </summary>
        /// <param name="evts"></param>
        public void OnEvent(IEnumerable<ICacheEntryEvent<TAGFileBufferQueueKey, TAGFileBufferQueueItem>> evts)
        {
            // Add the keys for the given events into the Project/Asset mapping buckets ready for a processing context
            // to acquire them. 

            Log.Info($"About to add TAG file items to the grouper");
            int count = 0;

            foreach (var evt in evts)
            {
                // Only interested in newly added items to the cache. Updates and deletes are ignored.
                if (evt.EventType != CacheEntryEventType.Created)
                    continue;

                count++;
                try
                {
                    TAGFileBufferQueueItemHandler.Instance().Add(evt.Key /*, evt.Value*/);
                    Log.Info($"Added TAG file item [{evt.Key}] to the grouper");
                }
                catch (Exception e)
                {
                    Log.Error(
                        $"Exception {e} occurred addign TAG file item {evt.Key} to the grouper");
                }
            }

            Log.Info($"Added {count} TAG file items to the grouper");
        }
    }
}
