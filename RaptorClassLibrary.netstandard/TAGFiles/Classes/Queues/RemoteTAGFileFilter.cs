using Apache.Ignite.Core.Cache.Event;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    public class RemoteTAGFileFilter : ICacheEntryEventFilter<TAGFileBufferQueueKey, TAGFileBufferQueueItem>
    {
        public bool Evaluate(ICacheEntryEvent<TAGFileBufferQueueKey, TAGFileBufferQueueItem> evt)
        {
            // Add the keys for the given events into the Project/Asset mapping buckets ready for a processing context
            // to acquire them. That context needs to be available through some kind of static namespace, such as the
            // TAG file processor context.
            // ....

            // Advise the caller this item is not filtered [as have already dealth with it so no futher 
            // processing of the item is required.
            return false;

            // Currently accept all TAG files on the primary node
        }
    }
}
