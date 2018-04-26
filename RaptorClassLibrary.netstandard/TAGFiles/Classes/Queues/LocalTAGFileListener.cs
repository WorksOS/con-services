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
        /// The TAG file grouper to submit new TAG cache items discovered by the continuous query
        /// </summary>
        public TAGFileBufferQueueGrouper Grouper { get; set; }

        /// <summary>
        /// Evnet called whenever there are new items in the TAG file buffer queue discovered by the continuous query
        /// </summary>
        /// <param name="evts"></param>
        public void OnEvent(IEnumerable<ICacheEntryEvent<TAGFileBufferQueueKey, TAGFileBufferQueueItem>> evts)
        {
            // Add the keys for the given events into the Project/Asset mapping buckets ready for a processing context
            // to acquire them. 

            foreach (var evt in evts)
            {
                try
                {
                    Grouper?.Add(evt.Key /*, evt.Value*/);
                }
                catch (Exception e)
                {
                    Log.Error(
                        $"Exception {e} occurred addign TAG file item {evt.Key} to the grouper from the continuous query");
                }
            }
        }
    }
}
