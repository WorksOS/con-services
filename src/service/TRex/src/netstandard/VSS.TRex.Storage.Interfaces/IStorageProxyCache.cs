using System.Collections.Generic;
using System.Threading.Tasks;
using Apache.Ignite.Core.Cache;

namespace VSS.TRex.Storage.Interfaces
{
  /// <summary>
  /// Defines the subset of Ignite ICache APIs required to support storage proxy semantics in TRex
  /// </summary>
  /// <typeparam name="TK"></typeparam>
  /// <typeparam name="TV"></typeparam>
  public interface IStorageProxyCache<TK, TV> : IStorageProxyCacheCommit
  {
    TV Get(TK key);
    Task<TV> GetAsync(TK key);

    bool TryGet(TK key, out TV value);
    Task<CacheResult<TV>> TryGetAsync(TK key);

    bool Remove(TK key);
    Task<bool> RemoveAsync(TK key);

    void RemoveAll(IEnumerable<TK> key);
    Task RemoveAllAsync(IEnumerable<TK> keys);

    void Put(TK key, TV value);
    Task PutAsync(TK key, TV value);

    void PutAll(IEnumerable<KeyValuePair<TK, TV>> values);
    Task PutAllAsync(IEnumerable<KeyValuePair<TK, TV>> vals);


    ICacheLock Lock(TK key);
  }
}
