using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Event;
using System;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    /// <summary>
    /// Provides a 'remote filter' for a continuous query watching the TAG file buffer queue. The filter itself
    /// performs some orchestration of the elements ready for further processing but does not pass the TAG file
    /// so as to suppress the TAG file being sent to the local context that emitted the conitous query itself
    /// </summary>
    [Serializable]
    public class RemoteTAGFileFilter : 
        ICacheEntryFilter<TAGFileBufferQueueKey, TAGFileBufferQueueItem>,
        ICacheEntryEventFilter<TAGFileBufferQueueKey, TAGFileBufferQueueItem>,
        IDisposable
    {
        [NonSerialized]
        private TAGFileBufferQueueItemHandler tagFileHandler;

        public void Dispose()
        {
            tagFileHandler?.Dispose();
        }

        private void EnsureTagFileInstance()
        {
            tagFileHandler = tagFileHandler ?? (tagFileHandler = TAGFileBufferQueueItemHandler.Instance());
        }

        public bool Invoke(ICacheEntry<TAGFileBufferQueueKey, TAGFileBufferQueueItem> entry)
        {
            EnsureTagFileInstance();

            // Add the keys for the given events into the Project/Asset mapping buckets ready for a processing context
            // to acquire them
            tagFileHandler.Add(entry.Key /*, entry.Value*/);

            // Advise the caller this item is not filtered [as have already dealt with it so no futher 
            // processing of the item is required.
            return false;
        }

        public bool Evaluate(ICacheEntryEvent<TAGFileBufferQueueKey, TAGFileBufferQueueItem> evt)
        {
            EnsureTagFileInstance();

            // Add the keys for the given events into the Project/Asset mapping buckets ready for a processing context
            // to acquire them
            tagFileHandler.Add(evt.Key /*, evt.Value*/);

            // Advise the caller this item is not filtered [as have already dealt with it so no futher 
            // processing of the item is required.
            return false;
        }
    }
}
