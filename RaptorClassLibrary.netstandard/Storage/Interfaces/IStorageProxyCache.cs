using System.Collections.Generic;
using Apache.Ignite.Core.Cache;

namespace VSS.TRex.Storage.Interfaces
{
    /// <summary>
    /// Defines the subset of Ignite ICache APIs required to support storage proxy semantics in TRex
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    public interface IStorageProxyCache<TK, TV>
    {
        TV Get(TK key);

        CacheResult<TV> GetAndRemove(TK key);

        void Put(TK key, TV value);

        void PutAll(IEnumerable<KeyValuePair<TK, TV>> values);

        string Name { get; }

        void Commit();
        void Clear();
    }
}
