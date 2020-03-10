using System.Collections.Generic;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Transactions;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.Storage
{
    /// <summary>
    /// Implements base semantics of ICache support for Storage proxies by shadowing it with a class member
    /// that is a concrete implementation of ICache
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    public class StorageProxyCache<TK, TV> : IStorageProxyCache<TK, TV>
    {
        private readonly ICache<TK, TV> Cache;
      
        public virtual string Name => Cache.Name;

        public virtual void Commit()
        {
            throw new System.NotImplementedException("Base StorageProxyCache does not support transactional behaviour");
        }

        public virtual void Commit(ITransaction tx)
        {
          throw new System.NotImplementedException("Base StorageProxyCache does not support transactional behaviour");
        }

        public virtual void Commit(out int numDeleted, out int numUpdated, out long numBytesWritten)
        {
           throw new System.NotImplementedException("Base StorageProxyCache does not support transactional behaviour"); 
        }
    
        public virtual void Commit(ITransaction tx, out int numDeleted, out int numUpdated, out long numBytesWritten)
        {
          throw new System.NotImplementedException("Base StorageProxyCache does not support transactional behaviour"); 
        }

        public virtual void Clear()
        {
            throw new System.NotImplementedException("Base StorageProxyCache does not support transactional behaviour");
        }

        public virtual void IncrementBytesWritten(long bytesWritten)
        {
          // No implementation for base class;
        }

        public StorageProxyCache(ICache<TK, TV> cache)
        {
            Cache = cache;
        }

        public virtual TV Get(TK key) => Cache.Get(key);

        public virtual bool Remove(TK key) => Cache.Remove(key);

        public virtual void RemoveAll(IEnumerable<TK> keys) => Cache.RemoveAll(keys);

        public virtual void Put(TK key, TV value) => Cache.Put(key, value);

        public virtual void PutAll(IEnumerable<KeyValuePair<TK, TV>> values) => Cache.PutAll(values);

        public virtual ICacheLock Lock(TK key) => Cache.Lock(key);
    }
}
