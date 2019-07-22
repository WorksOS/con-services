using System.Collections.Generic;
using System.Threading;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Transactions;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Extensions;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.Storage
{
    /// <summary>
    /// A transacted Storage Proxy that collects mutating changes (write and delete operations) that
    /// may be committed at a single time
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    public class StorageProxyCacheTransacted<TK, TV> : StorageProxyCache<TK, TV>, IStorageProxyCacheTransacted<TK, TV>
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<StorageProxyCacheTransacted<TK, TV>>();

        private static readonly bool ReportPendingTransactedDeleteDuplicatesToLog = false;

        /// <summary>
        /// The hashed set of elements pending deletion stored in the transacted cache.
        /// Note: The default equality comparer is supplied to prevent the hash set creating large numbers of comparer objects in its operations
        /// </summary>
        private readonly HashSet<TK> PendingTransactedDeletes;

        /// <summary>
        /// The dictionary of elements pending writing stored in the transacted cache.
        /// Note: The default equality comparer is supplied to prevent the hash set creating large numbers of comparer objects in its operations
        /// </summary>
        protected readonly Dictionary<TK, TV> PendingTransactedWrites;

        private long BytesWritten;

        public StorageProxyCacheTransacted(ICache<TK, TV> cache, IEqualityComparer<TK> comparer) : base(cache)
        {
          PendingTransactedDeletes = new HashSet<TK>(comparer);
          PendingTransactedWrites = new Dictionary<TK, TV>(comparer);
        }

        /// <summary>
        /// Provides look-aside get semantics into the transaction writes so that a write-then-read operation 
        /// will always return the value last 'written', even if not yet committed
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override TV Get(TK key)
        {
          lock (PendingTransactedWrites)
          {
            if (PendingTransactedWrites.TryGetValue(key, out var value))
              return value;
          }

          return base.Get(key);
        }

        /// <summary>
        /// Removes the given key from the cache. If there has been a previous un-committed remove for the key
        /// then an argument exception is thrown
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override bool Remove(TK key)
        {
          // Remove any uncommitted writes for the deleted item from pending writes
          lock (PendingTransactedWrites)
          {
              PendingTransactedWrites.Remove(key);
          }

          // Note the delete request in pending deletes
          var pendingTransactedAddFailed = false;
          lock (PendingTransactedDeletes)
          {
            pendingTransactedAddFailed = !PendingTransactedDeletes.Add(key);
          }

          if (pendingTransactedAddFailed && ReportPendingTransactedDeleteDuplicatesToLog)
            Log.LogWarning($"Key {key} is already present in the set of transacted deletes for the cache [Remove]");

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
        /// <param name="key"></param>
        /// <param name="value"></param>
        public override void Put(TK key, TV value)
        {
            lock (PendingTransactedWrites)
            {
              // If there is an existing pending write for the give key, then replace the
              // element in the dictionary with the new element
              PendingTransactedWrites.Remove(key);

              // Add the pending write request to the collection
              PendingTransactedWrites.Add(key, value);
            }

            if (value is byte[] bytes) 
              IncrementBytesWritten(bytes.Length);
        }

        /// <summary>
        /// Accepts a list of elements to be put and enlists the local Put() semantics to handle them
        /// </summary>
        /// <param name="values"></param>
        public override void PutAll(IEnumerable<KeyValuePair<TK, TV>> values) => values.ForEach(x => Put(x.Key, x.Value));
      
        /// <summary>
        /// Commits all pending deletes and writes to the underlying cache
        /// </summary>
        public override void Commit() => Commit(out _, out _, out _);

        public override void Commit(ITransaction tx) => Commit(out _, out _, out _);

        public override void Commit(out int numDeleted, out int numUpdated, out long numBytesWritten)
        {
            // The generic transactional cache cannot track the size of the elements being 'put' to the cache
            lock (PendingTransactedDeletes)
            {
              numDeleted = PendingTransactedDeletes.Count;
              base.RemoveAll(PendingTransactedDeletes);
            }

            lock (PendingTransactedWrites)
            {
              numUpdated = PendingTransactedWrites.Count;
              base.PutAll(PendingTransactedWrites);
            }

            numBytesWritten = BytesWritten;

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
            PendingTransactedDeletes.Clear();
          }

          lock (PendingTransactedWrites)
          {
            PendingTransactedWrites.Clear();
          }

          BytesWritten = 0;
        }

        public override void IncrementBytesWritten(long bytesWritten) => Interlocked.Add(ref BytesWritten, bytesWritten);

    }
}
