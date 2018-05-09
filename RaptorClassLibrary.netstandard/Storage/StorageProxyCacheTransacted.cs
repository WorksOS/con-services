using System;
using System.Collections.Generic;
using Apache.Ignite.Core.Cache;

namespace VSS.TRex.Storage
{
    /// <summary>
    /// A transacted Storeage Proxy that collects mutating changes (write and delete operations) that
    /// may be commited at a single time
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    public class StorageProxyCacheTransacted<TK, TV> : StorageProxyCache<TK, TV>
    {
        private HashSet<TK> PendingTransactedDeletes = new HashSet<TK>();
        private Dictionary<TK, TV> PendingTransactedWrites = new Dictionary<TK, TV>();

        public StorageProxyCacheTransacted(ICache<TK, TV> cache) : base(cache)
        {
        }

        /// <summary>
        /// Provides look-aside get semantics into the transaction writes so that a write-then-read operation 
        /// will always return the value last 'written', even if not yet committed
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public new TV Get(TK key)
        {
            return PendingTransactedWrites.TryGetValue(key, out TV value) ? value : base.Get(key);
        }

        /// <summary>
        /// Removes the given key from the cache. If there has been a previou un-committed remove for the key
        /// then an argument exception is thrown
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public new CacheResult<TV> GetAndRemove(TK key)
        {
            if (!PendingTransactedDeletes.Add(key))
                throw new ArgumentException($"Key {key} is already present in the set of transacted deletes for the cache");

            return base.GetAndRemove(key);
        }

        /// <summary>
        /// Provides Put semantics into the cache. If there has been a previous uncommited put for the same key then
        /// the previous value put is discarded and the new value used to replace it.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public new void Put(TK key, TV value)
        {
            // If there is an existing pending write for the give key, then replace the
            // element in the directionary with the new element
            if (PendingTransactedWrites.ContainsKey(key))
                PendingTransactedWrites.Remove(key);

            PendingTransactedWrites.Add(key, value);
        }

        /// <summary>
        /// Accepts a list of elements to be put and enlists the local Put() semantics to handle them
        /// </summary>
        /// <param name="values"></param>
        public new void PutAll(IEnumerable<KeyValuePair<TK, TV>> values)
        {
            foreach (var x in values)
                base.Put(x.Key, x.Value);
        }

        /// <summary>
        /// Commits all pending deletes and writes to the underlying cache
        /// </summary>
        public override void Commit()
        {
            foreach (var x in PendingTransactedDeletes)
                base.GetAndRemove(x);
            PendingTransactedDeletes.Clear();

            base.PutAll(PendingTransactedWrites);
            PendingTransactedWrites.Clear();
        }
    }
}
