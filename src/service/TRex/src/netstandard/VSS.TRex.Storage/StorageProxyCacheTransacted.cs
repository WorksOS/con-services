using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Transactions;
using Microsoft.Extensions.Logging;
using VSS.Serilog.Extensions;
using VSS.TRex.Common.Extensions;
using VSS.TRex.GridFabric;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.Storage
{
    /// <summary>
    /// A transacted Storage Proxy that collects mutating changes (write and delete operations) that
    /// may be committed at a single time
    /// </summary>
    public class StorageProxyCacheTransacted<TK, TV> : StorageProxyCache<TK, TV>, IStorageProxyCacheTransacted<TK, TV>
    {
        private static readonly ILogger _log = Logging.Logger.CreateLogger<StorageProxyCacheTransacted<TK, TV>>();

        private static readonly bool _reportPendingTransactedDeleteDuplicatesToLog = false;

        /// <summary>
        /// The hashed set of elements pending deletion stored in the transacted cache.
        /// Note: The default equality comparer is supplied to prevent the hash set creating large numbers of comparer objects in its operations
        /// </summary>
        protected readonly HashSet<TK> PendingTransactedDeletes;

        /// <summary>
        /// The dictionary of elements pending writing stored in the transacted cache.
        /// Note: The default equality comparer is supplied to prevent the hash set creating large numbers of comparer objects in its operations
        /// </summary>
        protected readonly Dictionary<TK, TV> PendingTransactedWrites;

        /// <summary>
        /// The total number of writes made to pending transacted writes, including updates of already existing elements
        /// </summary>
        protected long NumWritesToPendingTransactedWrites;

        /// <summary>
        /// The total number of reads made from pending transacted writes, that did not result in Get()'s to the underlying cache
        /// </summary>
        protected long NumReadsFromPendingTransactedWrites;

        private long _bytesWritten;

        public StorageProxyCacheTransacted(ICache<TK, TV> cache, IEqualityComparer<TK> comparer) : base(cache)
        {
          PendingTransactedDeletes = new HashSet<TK>(comparer);
          PendingTransactedWrites = new Dictionary<TK, TV>(comparer);
        }

        /// <summary>
        /// Provides look-aside get semantics into the transaction writes so that a write-then-read operation 
        /// will always return the value last 'written', even if not yet committed
        /// </summary>
        public override TV Get(TK key)
        {
          lock (PendingTransactedWrites)
          {
            if (PendingTransactedWrites.TryGetValue(key, out var value))
            {
              NumReadsFromPendingTransactedWrites++;
              return value;
            }
          }

          return base.Get(key);
        }

        /// <summary>
        /// Provides look-aside get semantics into the transaction writes so that a write-then-read operation 
        /// will always return the value last 'written', even if not yet committed
        /// </summary>
        public override Task<TV> GetAsync(TK key)
        {
          lock (PendingTransactedWrites)
          {
            if (PendingTransactedWrites.TryGetValue(key, out var value))
            {
              NumReadsFromPendingTransactedWrites++;
              return Task.FromResult(value);
            }
          }

          return base.GetAsync(key);
        }

        /// <summary>
        /// Removes the given key from the cache. If there has been a previous un-committed remove for the key
        /// then an argument exception is thrown
        /// </summary>
        public override bool Remove(TK key)
        {
          if (_log.IsTraceEnabled())
          {
            _log.LogTrace($"Removing item from pending transacted writes, cache = {Name}, key = {key}");
          }

          // Remove any uncommitted writes for the deleted item from pending writes
          lock (PendingTransactedWrites)
          {
            PendingTransactedWrites.Remove(key);
          }

          // Note the delete request in pending deletes
          lock (PendingTransactedDeletes)
          {
            var pendingTransactedAddFailed = !PendingTransactedDeletes.Add(key);

            if (pendingTransactedAddFailed && _reportPendingTransactedDeleteDuplicatesToLog)
              _log.LogWarning($"Key {key} is already present in the set of transacted deletes for cache {Name} [Remove]");
          }

          return true;
        }

        /// <summary>
        /// Removes the given keys from the cache. If there has been a previous un-committed remove for the key
        /// then an argument exception is thrown
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public override void RemoveAll(IEnumerable<TK> keys) => keys.ForEach(x => Remove(x));

        /// <summary>
        /// Provides Put semantics into the cache. If there has been a previous uncommitted put for the same key then
        /// the previous value put is discarded and the new value used to replace it.
        /// </summary>
        public override void Put(TK key, TV value)
        {
            if (_log.IsTraceEnabled())
            {
              _log.LogTrace($"Adding item (Put) to pending transacted writes, cache = {Name}, key = {key}");
            }

            lock (PendingTransactedWrites)
            {
              // If there is an existing pending write for the given key, then replace the
              // element in the dictionary with the new element
              PendingTransactedWrites.Remove(key);

              // Add the pending write request to the collection
              PendingTransactedWrites.Add(key, value);

              NumWritesToPendingTransactedWrites++;
            }

            if (value is ISerialisedByteArrayWrapper wrapper)
            {
              IncrementBytesWritten(wrapper.Count);
            }
        }

        /// <summary>
        /// Provides asynchronous Put semantics into the cache. If there has been a previous uncommitted put for the same key then
        /// the previous value put is discarded and the new value used to replace it.
        /// </summary>
        public override Task PutAsync(TK key, TV value) => Task.Run(() => Put(key, value));

        /// <summary>
        /// Accepts a list of elements to be put and enlists the local Put() semantics to handle them
        /// </summary>
        public override void PutAll(IEnumerable<KeyValuePair<TK, TV>> values) => values.ForEach(x => Put(x.Key, x.Value));
        
        /// <summary>
        /// Accepts a list of elements to be put and enlists the local PutAll() semantics to handle them
        /// </summary>
        public override Task PutAllAsync(IEnumerable<KeyValuePair<TK, TV>> values) => Task.Run(() => PutAll(values));

        /// <summary>
        /// Commits all pending deletes and writes to the underlying cache
        /// </summary>
        public override void Commit() => Commit(out _, out _, out _);

        public override void Commit(ITransaction tx) => Commit(out _, out _, out _);

        public override void Commit(out int numDeleted, out int numUpdated, out long numBytesWritten)
        {
            long localBytesWritten = 0;

            // The generic transactional cache cannot track the size of the elements being 'put' to the cache
            lock (PendingTransactedDeletes)
            {
                numDeleted = PendingTransactedDeletes.Count;

                // TODO: Can (should) this be pooled into a collection of tasks run concurrently?
                PendingTransactedDeletes.ForEach(async x =>
                {
                  try
                  {
                    await base.RemoveAsync(x);
                  }
                  catch (Exception e)
                  {
                    _log.LogError(e, $"Exception in RemoveAsync removing element with key {x} in cache {Name}");
                    throw;
                  }
                });

                // PendingTransactedDeletes needs to be ordered to safely call RemoveAll
                //base.RemoveAll(PendingTransactedDeletes);
            }

            lock (PendingTransactedWrites)
            {
              numUpdated = PendingTransactedWrites.Count;

              // Write all elements one at a time, calculating the cumulative bytes written in the Commit()
              PendingTransactedWrites.ForEach(async x =>
              {
                // TODO: Can (should) this be pooled into a collection of tasks run concurrently?

                try
                {
                  await base.PutAsync(x.Key, x.Value);

                  if (x.Value is ISerialisedByteArrayWrapper wrapper)
                    localBytesWritten += wrapper.Count;
                }
                catch (Exception e)
                {
                  _log.LogError(e, $"Exception in PutAsync putting element with key {x.Key} and value {x.Value} in cache {Name}");
                  throw;
                }
              });

              // This option leverages the ICache<>.PutAll() behaviour. This may cause problems with 
              // large writes and potential out of memory conditions in data regions reported as bugs to Ignite
              // PendingTransactedWrites needs to be ordered to safely call PutAll
              //base.PutAll(PendingTransactedWrites);
            }

            numBytesWritten = localBytesWritten;

            Clear();
        }

        public override void Commit(ITransaction tx, out int numDeleted, out int numUpdated, out long numBytesWritten)
        {
          Commit(out numDeleted, out numUpdated, out numBytesWritten);

          tx?.Commit();

          Clear();
        }

        /// <summary>
        /// Clears all pending delete and write information collected during the transaction
        /// </summary>
        public override void Clear()
        {
          lock (PendingTransactedDeletes)
          {
            if (_log.IsTraceEnabled())
            {
              _log.LogTrace($"Clearing {PendingTransactedDeletes.Count} deletes from transacted cache {Name}");
            }

            PendingTransactedDeletes.Clear();
          }

          lock (PendingTransactedWrites)
          {
            if (_log.IsTraceEnabled())
            {
              _log.LogTrace($"Clearing {PendingTransactedDeletes.Count} writes from transacted cache {Name}");
            }

            PendingTransactedWrites.Clear();
          }

          _bytesWritten = 0;
        }

        public override void IncrementBytesWritten(long bytesWritten) => Interlocked.Add(ref _bytesWritten, bytesWritten);

        public override long PotentialCommitWrittenBytes()
        {
          var numBytes = 0;

          lock (PendingTransactedWrites)
          {
            PendingTransactedWrites.Values.ForEach(x =>
            {
              if (x is ISerialisedByteArrayWrapper wrapper)
                numBytes += wrapper.Count;
            });
          }

          return numBytes;
        }
    }
}
