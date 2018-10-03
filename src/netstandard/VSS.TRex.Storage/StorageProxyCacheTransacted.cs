using System;
using System.Collections.Generic;
using Apache.Ignite.Core.Cache;
using Microsoft.Extensions.Logging;

namespace VSS.TRex.Storage
{
    /// <summary>
    /// A transacted Storage Proxy that collects mutating changes (write and delete operations) that
    /// may be committed at a single time
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    public class StorageProxyCacheTransacted<TK, TV> : StorageProxyCache<TK, TV>
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<StorageProxyCacheTransacted<TK, TV>>();

        private HashSet<TK> PendingTransactedDeletes = new HashSet<TK>();
        private Dictionary<TK, TV> PendingTransactedWrites = new Dictionary<TK, TV>();

        public long BytesWritten { get; set; }

        public StorageProxyCacheTransacted(ICache<TK, TV> cache) : base(cache)
        {
        }

        /// <summary>
        /// Provides look-aside get semantics into the transaction writes so that a write-then-read operation 
        /// will always return the value last 'written', even if not yet committed
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override TV Get(TK key) => PendingTransactedWrites.TryGetValue(key, out TV value) ? value : base.Get(key);

        /// <summary>
        /// Removes the given key from the cache. If there has been a previous un-committed remove for the key
        /// then an argument exception is thrown
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override bool Remove(TK key)
        {
          if (!PendingTransactedDeletes.Add(key))
          {
            Log.LogWarning($"Key {key} is already present in the set of transacted deletes for the cache [Remove]");
          }

          return true;
        }

        /// <summary>
        /// Removes the given keys from the cache. If there has been a previous un-committed remove for the key
        /// then an argument exception is thrown
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public override void RemoveAll(IEnumerable<TK> keys)
        {
          foreach (var key in keys)
          {
            if (!PendingTransactedDeletes.Add(key))
            {
              Log.LogWarning($"Key {key} is already present in the set of transacted deletes for the cache [RemoveAll]"); 
            }
          }
        }

        /// <summary>
        /// Provides Put semantics into the cache. If there has been a previous uncommitted put for the same key then
        /// the previous value put is discarded and the new value used to replace it.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public override void Put(TK key, TV value)
        {
            // If there is an existing pending write for the give key, then replace the
            // element in the dictionary with the new element
            if (PendingTransactedWrites.ContainsKey(key))
                PendingTransactedWrites.Remove(key);

            PendingTransactedWrites.Add(key, value);

            if (value is byte[] bytes) 
              IncrementBytesWritten(bytes.Length);
        }

        /// <summary>
        /// Accepts a list of elements to be put and enlists the local Put() semantics to handle them
        /// </summary>
        /// <param name="values"></param>
        public override void PutAll(IEnumerable<KeyValuePair<TK, TV>> values)
        {
          foreach (var x in values)
            Put(x.Key, x.Value);
        }

        /// <summary>
        /// Commits all pending deletes and writes to the underlying cache
        /// </summary>
        public override void Commit() => Commit(out _, out _, out _);

        public override void Commit(out int numDeleted, out int numUpdated, out long numBytesWritten)
        {
            // The generic transactional cache cannot track the size of the elements being 'put' to the cache
            numDeleted = PendingTransactedDeletes.Count;
            foreach (var x in PendingTransactedDeletes)
              base.Remove(x);

            numUpdated = PendingTransactedWrites.Count;

            base.PutAll(PendingTransactedWrites);

            numBytesWritten = BytesWritten;

            Clear();
        }

        /// <summary>
        /// Clears all pending delete and write information collected during the transaction
        /// </summary>
        public override void Clear()
        {
            PendingTransactedDeletes.Clear();
            PendingTransactedWrites.Clear();

            BytesWritten = 0;
        }

        public override void IncrementBytesWritten(long bytesWritten) => BytesWritten += bytesWritten;
  }
}
