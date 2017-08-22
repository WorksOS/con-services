using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using Microsoft.Extensions.Caching.Memory;

namespace VSS.Productivity3D.Common.Filters.Caching
{

  //Based on a reference implementation
  public class ParametrizedMemoryResponseCache : IResponseCache
  {
    private readonly IMemoryCacheBuilder<Guid> _cache;
    private readonly IResponseCachingKeyProvider _keyProvider;

    public ParametrizedMemoryResponseCache(IMemoryCacheBuilder<Guid> cache, IResponseCachingKeyProvider keyProvider)
    {
      if (cache == null)
        throw new ArgumentNullException(nameof(cache));

      _cache = cache;
      _keyProvider = keyProvider;
    }

    public Task<IResponseCacheEntry> GetAsync(string key)
    {
      //Deviation from reference implementation
      var projectUid = _keyProvider.ExtractProjectGuidFromKey(key);
      //Deviation from reference implementation
      var entry = _cache.GetMemoryCache(projectUid).Get(key);
      var memoryCachedResponse = entry as MemoryCachedResponse;

      if (memoryCachedResponse != null)
        return Task.FromResult<IResponseCacheEntry>(new CachedResponse
        {
          Created = memoryCachedResponse.Created,
          StatusCode = memoryCachedResponse.StatusCode,
          Headers = memoryCachedResponse.Headers,
          Body = new SegmentReadStream(memoryCachedResponse.BodySegments, memoryCachedResponse.BodyLength)
        });

      return Task.FromResult(entry as IResponseCacheEntry);
    }


    public async Task SetAsync(string key, IResponseCacheEntry entry, TimeSpan validFor)
    {
      var cachedResponse = entry as CachedResponse;
      //Deviation from reference implementation
      var projectUid = _keyProvider.ExtractProjectGuidFromKey(key);
      if (cachedResponse != null)
      {
        var segmentStream = new SegmentWriteStream(StreamUtilities.BodySegmentSize);

        await cachedResponse.Body.CopyToAsync(segmentStream);
        //Deviation from reference implementation
        _cache.GetMemoryCache(projectUid).Set<MemoryCachedResponse>(
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
            AbsoluteExpirationRelativeToNow = validFor
          });
      }
      else
      {
        //Deviation from reference implementation
        _cache.GetMemoryCache(projectUid).Set(
          key,
          entry,
          new MemoryCacheEntryOptions

          {
            AbsoluteExpirationRelativeToNow = validFor
          });
      }
    }
  }
}