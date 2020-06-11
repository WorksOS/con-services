using System.Collections.Generic;
using System.Threading.Tasks;
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
    private readonly ICache<TK, TV> _cache;

    public bool HasCache => _cache != null;

    public virtual string Name => _cache?.Name ?? "";

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

    /// <summary>
    /// Standard Ignite caches do not have pending writes to compute potential bytes written for.
    /// </summary>
    public long PotentialCommitWrittenBytes() => 0;

    public virtual void IncrementBytesWritten(long bytesWritten)
    {
      // No implementation for base class;
    }

    public StorageProxyCache(ICache<TK, TV> cache)
    {
      _cache = cache;
    }

    public virtual TV Get(TK key) => _cache.Get(key);

    public virtual Task<TV> GetAsync(TK key) => _cache.GetAsync(key);
    public bool TryGet(TK key, out TV value) => _cache.TryGet(key, out value);

    public Task<CacheResult<TV>> TryGetAsync(TK key) => _cache.TryGetAsync(key);

    public virtual bool Remove(TK key) => _cache.Remove(key);
    public Task<bool> RemoveAsync(TK key) => _cache.RemoveAsync(key);

    public virtual void RemoveAll(IEnumerable<TK> keys) => _cache.RemoveAll(keys);
    public Task RemoveAllAsync(IEnumerable<TK> keys) => _cache.RemoveAllAsync(keys);

    public virtual Task PutAsync(TK key, TV value) => _cache.PutAsync(key, value);

    public virtual void Put(TK key, TV value) => _cache.Put(key, value);

    public virtual void PutAll(IEnumerable<KeyValuePair<TK, TV>> values) => _cache.PutAll(values);
    public Task PutAllAsync(IEnumerable<KeyValuePair<TK, TV>> values) => _cache.PutAllAsync(values);

    public virtual ICacheLock Lock(TK key) => _cache.Lock(key);
  }
}
