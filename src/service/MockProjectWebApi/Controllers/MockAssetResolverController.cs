using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace MockProjectWebApi.Controllers
{
  public class MockAssetResolverController : Controller
  {
    [HttpGet("api/v1/assets")]
    public IEnumerable<KeyValuePair<Guid, long>> GetMatchingAssets(
      [FromQuery] List<Guid> assetUids
      )
    {
      Console.WriteLine("Get MockAssetResolverList for supplied Guids");
      return new List<KeyValuePair<Guid, long>>();
    }

    [HttpGet("api/v1/assets")]
    public IEnumerable<KeyValuePair<Guid, long>> GetMatchingAssets(
      [FromQuery] List<long> assetIds
    )
    {
      Console.WriteLine("Get MockAssetResolverList for supplied longs");
      return new List<KeyValuePair<Guid, long>>();
    }
  }
}
