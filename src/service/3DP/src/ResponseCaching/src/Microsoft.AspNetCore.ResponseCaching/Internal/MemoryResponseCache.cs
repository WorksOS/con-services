// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.AspNetCore.ResponseCaching.Internal
{
  public class MemoryResponseCache : IResponseCache, IExtendedResponseCache
  {
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, ConcurrentBag<string>> _cacheKeys;
    private readonly SemaphoreSlim _semaphore;

    public MemoryResponseCache(IMemoryCache cache)
    {
      _cache = cache ?? throw new ArgumentNullException(nameof(cache));
      _cacheKeys = new ConcurrentDictionary<string, ConcurrentBag<string>>();
      _semaphore = new SemaphoreSlim(1, 1);
    }

    public IResponseCacheEntry Get(string key)
    {
      var entry = _cache.Get(key);

      if (entry is MemoryCachedResponse memoryCachedResponse)
      {
        return new CachedResponse
        {
          Created = memoryCachedResponse.Created,
          StatusCode = memoryCachedResponse.StatusCode,
          Headers = memoryCachedResponse.Headers,
          Body = new SegmentReadStream(memoryCachedResponse.BodySegments, memoryCachedResponse.BodyLength)
        };
      }

      return entry as IResponseCacheEntry;
    }

    public Task<IResponseCacheEntry> GetAsync(string key)
    {
      return Task.FromResult(Get(key));
    }

    public void Set(string baseKey, string key, IResponseCacheEntry entry, TimeSpan validFor)
    {
      try
      {
        _semaphore.Wait();
        if (entry is CachedResponse cachedResponse)
        {
          var segmentStream = new SegmentWriteStream(StreamUtilities.BodySegmentSize);
          cachedResponse.Body.CopyTo(segmentStream);
          _cache.Set(
              key,
              new MemoryCachedResponse
              {
                Created = cachedResponse.Created,
                StatusCode = cachedResponse.StatusCode,
                Headers = cachedResponse.Headers,
                BodySegments = segmentStream.GetSegments(),
                BodyLength = segmentStream.Length
              },
              new MemoryCacheEntryOptions
              {
                AbsoluteExpirationRelativeToNow = validFor,
                Size = CacheEntryHelpers.EstimateCachedResponseSize(cachedResponse)
              });
        }
        else
        {
          _cache.Set(
              key,
              entry,
              new MemoryCacheEntryOptions
              {
                AbsoluteExpirationRelativeToNow = validFor,
                Size = CacheEntryHelpers.EstimateCachedVaryByRulesySize(entry as CachedVaryByRules)
              });
        }
      }
      finally
      {
        _semaphore.Release();
      }
      


      var cacheKey = baseKey ?? key;
      if (cacheKey.StartsWith("PRJUID=") && !_cacheKeys.ContainsKey(cacheKey))
      {
        _cacheKeys.TryAdd(cacheKey, new ConcurrentBag<string>());
      }

      if (!string.IsNullOrEmpty(baseKey) && !string.IsNullOrEmpty(key))
      {
        _cacheKeys.TryGetValue(baseKey, out var cacheBag);
        cacheBag.Add(key);
      }
    }

    public Task SetAsync(string baseKey, string key, IResponseCacheEntry entry, TimeSpan validFor)
    {
      Set(baseKey, key, entry, validFor);
      return Task.CompletedTask;
    }

    /// <summary>
    /// Custom VSS method to clear the response cache of all responses for the project.
    /// </summary>
    public Task ClearAsync(string projectUidBaseKey)
    {
      if (_cacheKeys.TryGetValue(projectUidBaseKey, out var cacheBag))
      {
        while (cacheBag.TryTake(out var cacheKey))
        {
            _cache.Remove(cacheKey);
        }

        // After clearing our collection of cached response keys and their response cache objects, remember to remove the root key.
        _cacheKeys.TryRemove(projectUidBaseKey, out _);
      }

      return Task.CompletedTask;
    }
  }
}
