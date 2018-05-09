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

        public StorageProxyCache(ICache<TK, TV> cache)
        {
            Cache = cache;
        }

        public TV Get(TK key)
        {
            return Cache.Get(key);
        }

        public CacheResult<TV> GetAndRemove(TK key)
        {
            return Cache.GetAndRemove(key);
        }

        public void Put(TK key, TV value)
        {
            Cache.Put(key, value);
        }

        public void PutAll(IEnumerable<KeyValuePair<TK, TV>> values)
        {
            Cache.PutAll(values);
        }
    }
}
