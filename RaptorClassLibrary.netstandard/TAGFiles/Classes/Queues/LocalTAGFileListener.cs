using Apache.Ignite.Core.Cache.Event;
using System.Collections.Generic;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    public class LocalTAGFileListener : ICacheEntryEventListener<TAGFileBufferQueueKey, TAGFileBufferQueueItem>
    {
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


        }
    }
}
