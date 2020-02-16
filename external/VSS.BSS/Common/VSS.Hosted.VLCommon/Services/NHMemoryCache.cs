using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;

namespace VSS.Hosted.VLCommon
{
  public class NHMemoryCache<K,T> : MemoryCache
  {
    private CacheItemPolicy cacheItemPolicy = null;

    public TimeSpan SlidingExpiration
    {
      set { cacheItemPolicy.SlidingExpiration = value; }
    }

    public long Count
    {
      get { return base.GetCount(); }
    }

    public NHMemoryCache(string cacheName) : base(cacheName)
    {
      cacheItemPolicy = new CacheItemPolicy();      
    }
  
    public bool Add(K key, T value)
    {      
      return base.Add(key.ToString(), value, cacheItemPolicy, null);
    }
   
    public void Set(K key, T value)
    {
      base.Set(key.ToString(), value, cacheItemPolicy, null);
    }

    public void Set(K key, T value, DateTimeOffset absoluteExpiration)
    {
      base.Set(key.ToString(), value, absoluteExpiration, null);
    }

    public void SetValues(Dictionary<K, T> values)
    {
      foreach (var item in values)
      {
        Set(item.Key, item.Value);
      }
    }

    public void SetValues(Dictionary<K, T> values, DateTimeOffset absoluteExpiration)
    {
      foreach (var item in values)
      {
        Set(item.Key, item.Value, absoluteExpiration);
      }
    }

    public T Get(K key)
    {
      return (T)base.Get(key.ToString(), null);
    }

    public bool Contains(K key)
    {
      return base.Contains(key.ToString(), null);
    }

    public void Clear()
    {
      IEnumerator<KeyValuePair<string, object>> enumerator = base.GetEnumerator();
      while (enumerator.MoveNext())
      {
        base.Remove(enumerator.Current.Key);
      }
    }

    public void SetValuesNoCacheExpiry(Dictionary<K, T> values)
    {
      foreach (var item in values)
      {
        Set(item.Key, item.Value, ObjectCache.InfiniteAbsoluteExpiration);
      }
    }
    
  }
}
