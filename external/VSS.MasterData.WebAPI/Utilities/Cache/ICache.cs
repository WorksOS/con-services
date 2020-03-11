using System;
using System.Runtime.Caching;

namespace Utilities.Cache
{
    public interface ICache
    {
        void SetPolicy(CacheItemPolicy policy);
        T Get<T>(string key, Func<string, T> action, CacheItemPolicy policy = null, string region = null);
        bool Upsert<T>(string key, T value, CacheItemPolicy policy = null, string region = null);
        long GetRegionCount(string regionName = null);
    }
}
