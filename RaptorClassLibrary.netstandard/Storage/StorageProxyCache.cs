using System.Collections.Generic;
using Apache.Ignite.Core.Cache;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.Storage
{
    /// <summary>
    /// Implements base semantics of ICache support for Storage proxies by shadowing it with a class member
    /// that is a concrete implementation of ICache
    /// implem
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    public class StorageProxyCache<TK, TV> : IStorageProxyCache<TK, TV>
    {
        private ICache<TK, TV> Cache;

        public string Name
        {
            get => Cache.Name;
        }

        public virtual void Commit()
        {
            throw new System.NotImplementedException("Base StorageProxyCache does not support transactional behaviour");
        }

        public virtual void Clear()
        {
            throw new System.NotImplementedException("Base StorageProxyCache does not support transactional behaviour");
        }

        public StorageProxyCache(ICache<TK, TV> cache)
        {
            Cache = cache;
        }

        public virtual TV Get(TK key)
        {
            return Cache.Get(key);
        }

        public virtual bool Remove(TK key)
        {
            return Cache.Remove(key);
        }

        public virtual void Put(TK key, TV value)
        {
            Cache.Put(key, value);
        }

        public virtual void PutAll(IEnumerable<KeyValuePair<TK, TV>> values)
        {
            Cache.PutAll(values);
        }
    }
}
