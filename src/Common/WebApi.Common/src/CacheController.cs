using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using VSS.Common.Abstractions.Cache.Interfaces;

namespace VSS.WebApi.Common
{
  // Hide from Swagger
  [ApiExplorerSettings(IgnoreApi=true)]
  public class CacheController : Controller
  {
    private readonly IDataCache cache;

    public CacheController(IDataCache cache)
    {
      this.cache = cache;
    }

    [HttpGet("cache/tags")]
    public IActionResult GetTags()
    {
      var tags = cache.CacheTags;
      return Json(tags);
    }

    [HttpGet("cache/keys")]
    public IActionResult GetKeys()
    {
      var keys = cache.CacheKeys;
      return Json(keys);
    }

    [HttpGet("cache/keys/{key}")]
    public IActionResult GetItem(string key)
    {
      var obj = cache.Get<object>(key);
      if (obj == null)
        return Json(null);

      var result = new
      {
        Data = obj,
        Tags = cache.GetTagsForKey(key)
      };

      return Json(result);
    }

    [HttpDelete("cache/tags/{tag}")]
    public IActionResult DeleteTag(string tag)
    {
      cache.RemoveByTag(tag);
      return Ok();
    }

    [HttpDelete("cache/keys/{key}")]
    public IActionResult DeleteKey(string key)
    {
      cache.RemoveByKey(key);
      return Ok();
    }

    [HttpGet("cache/dump")]
    public IActionResult Dump()
    {
      var keys = cache.CacheKeys;
      var tags = cache.CacheTags;

      var objects = new Dictionary<string, object>();
      foreach (var key in keys)
      {
        var obj = cache.Get<object>(key);
        var value = new
        {
          Data = obj,
          Tags = cache.GetTagsForKey(key)
        };
        objects.Add(key, value);
      }

      var data = new
      {
        Keys = keys,
        Tags = tags,
        Objects = objects
      };
      return Json(data);
    }
  }
}