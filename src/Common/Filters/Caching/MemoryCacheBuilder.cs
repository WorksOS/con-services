using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace VSS.Productivity3D.Common.Filters.Caching
{
  public class MemoryCacheBuilder<T> : IMemoryCacheBuilder<T> where T : IEquatable<T>
  {
    private readonly Dictionary<T, IMemoryCache> cacheObjects;
    private readonly ReaderWriterLockSlim entryLock;
    private readonly IOptions<MemoryCacheOptions> cacheOptions;

    public MemoryCacheBuilder(IOptions<MemoryCacheOptions> memoryCacheOptions)
    {
      cacheObjects = new Dictionary<T, IMemoryCache>();
      entryLock = new ReaderWriterLockSlim();
      cacheOptions = memoryCacheOptions;
    }

    public IMemoryCache GetMemoryCache(T cacheUid)
    {
      entryLock.EnterReadLock();
      try
      {
        if (cacheObjects.TryGetValue(cacheUid, out var memoryObject))
          return memoryObject;
      }
      finally
      {
        entryLock.ExitReadLock();
      }

      entryLock.EnterWriteLock();
      try
      {
        var cache = new MemoryCache(cacheOptions);
        cacheObjects.Add(cacheUid, cache);
        return cache;
      }
      finally
      {
        entryLock.ExitWriteLock();
      }
    }

    public void ClearMemoryCache(T cacheUid)
    {
      entryLock.EnterWriteLock();

      try
      {
        if (!cacheObjects.TryGetValue(cacheUid, out var memoryObject)) return;
        cacheObjects.Remove(cacheUid);
        memoryObject.Dispose();
      }
      finally
      {
        entryLock.ExitWriteLock();
      }
    }
  }
}
