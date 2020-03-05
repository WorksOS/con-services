using System;
using System.Runtime.Caching;
using System.Threading;

namespace Utilities.Cache
{
    public class InMemoryCache : ICache
    {
        private readonly MemoryCache _memoryCache;
        private string _region;
        private CacheItemPolicy _cacheItemPolicy;
        private ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        public InMemoryCache(string region = null, CacheItemPolicy cacheItemPolicy = null)
        {
            this._memoryCache = MemoryCache.Default;
            if (!string.IsNullOrEmpty(region))
            {
                this._region = region;
            }

            if (cacheItemPolicy == null)
            {
                this._cacheItemPolicy = new CacheItemPolicy
                {
                    AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddHours(24)),
                    Priority = CacheItemPriority.NotRemovable
                };
            }
            else
            {
                this._cacheItemPolicy = cacheItemPolicy;
            }
        }

        public void SetPolicy(CacheItemPolicy policy)
        {
            this._cacheItemPolicy = policy;
        }

        public T Get<T>(string key, Func<string, T> action, CacheItemPolicy policy = null, string region = null)
        {
            if (policy == null)
            {
                policy = this._cacheItemPolicy;
            }
            if (string.IsNullOrEmpty(region))
            {
                region = this._region;
            }
            var cacheKey = (region + ":" + key).ToLower();
            locker.EnterReadLock();
            var data = this._memoryCache.Get(cacheKey);
            locker.ExitReadLock();
            if (data == null)
            {
                if (action != null)
                {
                    data = action(key); //Intentionally left null check for data/value, since the action method can return null for some keys
                    //locker.EnterWriteLock();
                    //this._memoryCache.Add(cacheKey, data, policy, null);
                    //locker.ExitWriteLock();
                }
            }
            return (T)data;
        }

        public bool Upsert<T>(string key, T value, CacheItemPolicy policy = null, string region = null)
        {
            //Intentionally left null check for data/value, since the user can assign null for some keys
            bool result = false;
            if (policy == null)
            {
                policy = this._cacheItemPolicy;
            }
            if (string.IsNullOrEmpty(region))
            {
                region = this._region;
            }
            var cacheKey = (region + ":" + key).ToLower();
            try
            {
                if (this._memoryCache.Contains(cacheKey))
                {
                    locker.EnterWriteLock();
                    this._memoryCache[cacheKey] = value;
                    locker.ExitWriteLock();
                }
                else
                {
                    locker.EnterWriteLock();
                    this._memoryCache.Add(cacheKey, value, policy, null);
                    locker.ExitWriteLock();
                }
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public long GetRegionCount(string regionName = null)
        {
            return this._memoryCache.GetCount(regionName);
        }
    }
}
