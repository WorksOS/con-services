using System;
using System.Collections.Generic;
using System.Text;
using Apache.Ignite.Core.Cache;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    /// <summary>
    /// Provides a container for the real-time and latent TAG file buffer queues made available in TRex
    /// </summary>
    public class TAGFileBufferQueues
    {
        /// <summary>
        /// The real time queue is intended to buffer TAG files that arrive as a result of direct TAG file submissiong
        /// from in field systems
        /// </summary>
        public TAGFileBufferQueue realTimeQueue = new TAGFileBufferQueue();

        /// <summary>
        /// The latent queue is intended to buffer TAG files that arrive as a part of bulk reprocessing or manual 
        /// importation of TAg files into a project
        /// </summary>
        public TAGFileBufferQueue latentQueue = new TAGFileBufferQueue();

        /// <summary>
        /// Returns a batch of TAG files to be processed either from the real time queue if there are any available,
        /// or from the latent queue.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ICacheEntry<TAGFileBufferQueueKey, TAGFileBufferQueueItem>> SelectBatch()
        {
            return realTimeQueue.SelectBatch() ?? latentQueue.SelectBatch();
        }
    }
}
