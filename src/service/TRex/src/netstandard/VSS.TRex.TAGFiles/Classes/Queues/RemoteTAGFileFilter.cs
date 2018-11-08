using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Event;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    /// <summary>
    /// Provides a 'remote filter' for a continuous query watching the TAG file buffer queue. The filter itself
    /// performs some orchestration of the elements ready for further processing but does not pass the TAG file
    /// so as to suppress the TAG file being sent to the local context that emitted the continuous query itself
    /// </summary>
    public class RemoteTAGFileFilter : 
        ICacheEntryFilter<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>,
        ICacheEntryEventFilter<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>
    {
        public readonly TAGFileBufferQueueItemHandler handler;

        public RemoteTAGFileFilter(TAGFileBufferQueueItemHandler handler)
        {
            this.handler = handler;
        }

        public bool Invoke(ICacheEntry<ITAGFileBufferQueueKey, TAGFileBufferQueueItem> entry)
        {
            // Add the keys for the given events into the Project/Asset mapping buckets ready for a processing context
            // to acquire them
            handler.Add(entry.Key);

            // Advise the caller this item is not filtered [as have already dealt with it so no further 
            // processing of the item is required.
            return false;
        }

        public bool Evaluate(ICacheEntryEvent<ITAGFileBufferQueueKey, TAGFileBufferQueueItem> evt)
        {
            // Add the keys for the given events into the Project/Asset mapping buckets ready for a processing context
            // to acquire them
            handler.Add(evt.Key);

            // Advise the caller this item is not filtered [as have already dealt with it so no further 
            // processing of the item is required.
            return false;
        }
    }
}
