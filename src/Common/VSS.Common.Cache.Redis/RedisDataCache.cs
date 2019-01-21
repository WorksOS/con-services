using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using StackExchange.Redis;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Cache.Models;

namespace VSS.Common.Cache.Redis
{
  public class RedisDataCache : IDataCache
  {
    private string KEY_TAG_IDENTIFIER = "tag_";
    private string KEY_IDENTIFIER = "key_";

    private ConnectionMultiplexer redisConnection;

    public RedisDataCache()
    {
      redisConnection = ConnectionMultiplexer.Connect("localhost");
    }

    public TItem Get<TItem>(string key) where TItem : class
    {
      var db = redisConnection.GetDatabase();
      var k = GetKeyName(key);
      var data = db.StringGet(k);
      return FromBinary<TItem>(data);
    }

    public async Task<TItem> GetOrCreate<TItem>(string key, Func<ICacheEntry, Task<CacheItem<TItem>>> factory) where TItem : class
    {
      var db = redisConnection.GetDatabase();
      var k = GetKeyName(key);
      var data = db.StringGet(k);
      var result = FromBinary<TItem>(data);
      if (result != null)
        return result;

      var cacheEntry = new RedisCacheEntry(k);
      var item = await factory(cacheEntry);

      if (item?.Value == null)
        return null;

      // TODO Map the options
      cacheEntry.Value = item.Value;

      db.StringSet(k, ToBinary(item.Value));

      AddTagsForKey(k, item.Tags);

      return item.Value;
    }

    public TItem Set<TItem>(string key, TItem value, IEnumerable<string> tags, MemoryCacheEntryOptions options = null) where TItem : class
    {
      var db = redisConnection.GetDatabase();
      var k = GetKeyName(key);
      // TODO Map the options
      db.StringSet(k, ToBinary(value));

      AddTagsForKey(k, tags?.ToList() ?? new List<string>());

      return value;
    }

    public void RemoveByTag(string tag)
    {
      var t = GetTagKeyName(tag);
      var db = redisConnection.GetDatabase();
      var members = db.SetMembers(t);

      var keys = members
        .Select(redisValue => (RedisKey)GetKeyName(redisValue))
        .ToArray();

      db.KeyDelete(keys);
      db.KeyDelete(t);
    }

    public void RemoveByKey(string key)
    {
      var db = redisConnection.GetDatabase();
      var k = GetKeyName(key);
      db.KeyDelete(k);
    }

    private void AddTagsForKey(string key, IReadOnlyCollection<string> tags)
    {
      if ((tags?.Count ?? 0) == 0)
        return;

      var db = redisConnection.GetDatabase();
      var k = GetKeyName(key);
      foreach (var tag in tags)
      {
        var t = GetTagKeyName(tag);
        db.SetAdd(t, k); // If it already exists in redis, it won't be added again
      }
    }

    private byte[] ToBinary<T>(T t) where T : class
    {
      if (t == null)
        return null;

      return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(t));

    }

    private T FromBinary<T>(byte[] data) where T : class
    {
      if (data == null || data.Length == 0)
        return null;

      var obj = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));

      return obj;
    }

    private string GetTagKeyName(string tag)
    {
      if (tag.StartsWith(KEY_TAG_IDENTIFIER))
        return tag.ToLower();

      if (tag.StartsWith(KEY_IDENTIFIER))
        tag = tag.Replace(KEY_IDENTIFIER, string.Empty);

      return $"{KEY_TAG_IDENTIFIER}{tag.ToLower()}";
    }

    private string GetKeyName(string key)
    {
      if (key.StartsWith(KEY_IDENTIFIER))
        return key.ToLower();

      if (key.StartsWith(KEY_TAG_IDENTIFIER))
        key = key.Replace(KEY_TAG_IDENTIFIER, string.Empty);

      return $"{KEY_IDENTIFIER}{key.ToLower()}";
    }

  }
}
