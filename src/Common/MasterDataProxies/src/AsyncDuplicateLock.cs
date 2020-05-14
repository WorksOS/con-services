using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VSS.MasterData.Proxies
{
  public sealed class AsyncDuplicateLock
  {
    private sealed class RefCounted<T>
    {
      public RefCounted(T value)
      {
        RefCount = 1;
        Value = value;
      }

      public int RefCount { get; set; }
      public T Value { get; private set; }
    }

    private static readonly Dictionary<string, RefCounted<SemaphoreSlim>> _semaphoreSlims
      = new Dictionary<string, RefCounted<SemaphoreSlim>>();

    private SemaphoreSlim GetOrCreate(string key)
    {
      RefCounted<SemaphoreSlim> item;
      lock (_semaphoreSlims)
      {
        if (_semaphoreSlims.TryGetValue(key, out item))
        {
          ++item.RefCount;
        }
        else
        {
          item = new RefCounted<SemaphoreSlim>(new SemaphoreSlim(1, 1));
          _semaphoreSlims[key] = item;
        }
      }
      return item.Value;
    }

    public IDisposable Lock(string key)
    {
      GetOrCreate(key).Wait();
      return new Releaser { Key = key };
    }

    public async Task<IDisposable> LockAsync(string key)
    {
      await GetOrCreate(key).WaitAsync();
      return new Releaser { Key = key };
    }

    private sealed class Releaser : IDisposable
    {
      public string Key { get; set; }

      public void Dispose()
      {
        RefCounted<SemaphoreSlim> item;
        lock (_semaphoreSlims)
        {
          item = _semaphoreSlims[Key];
          --item.RefCount;
          if (item.RefCount == 0)
            _semaphoreSlims.Remove(Key);
        }
        item.Value.Release();
      }
    }
  }
}
