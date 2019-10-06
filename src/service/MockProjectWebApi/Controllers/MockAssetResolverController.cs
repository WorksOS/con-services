using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.AssetMgmt3D.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockAssetResolverController : BaseController
  {
    public MockAssetResolverController(ILoggerFactory loggerFactory) : base(loggerFactory)
    { }

    [HttpPost("api/v1/assets/assetuids")]
    [ProducesResponseType(typeof(List<AssetDisplayModel>), 200)]
    public IActionResult GetMatchingAssets([FromBody] List<Guid> assetUids)
    {
      Logger.LogInformation($@"Get MockAssetResolverList for supplied Guids: {assetUids}");

      return Ok(new AssetDisplayModel {assetIdentifiers = new List<KeyValuePair<Guid, long>>()});
    }

    [HttpPost("api/v1/assets/assetids")]
    [ProducesResponseType(typeof(List<AssetDisplayModel>), 200)]
    public IActionResult GetMatchingAssets([FromBody] List<long> assetIds)
    {
      Logger.LogInformation($@"Get MockAssetResolverList for supplied longs: {assetIds}");

      return Ok(new AssetDisplayModel { assetIdentifiers = new List<KeyValuePair<Guid, long>>() });
    }

    private List<KeyValuePair<Guid, long>> GoldenDataMachineList(List<long> assetIds)
    {
      var result = new List<KeyValuePair<Guid, long>>();
      foreach (var assetId in assetIds)
        result.Add(GoldenDimensionsProjectMachineIds.Find(x => x.Value == assetId));

      return result;
    }

    // this set is returned for Mock acceptance tests for raptorClient.GetMachineIDs
    //     GOLDEN_DATA_DIMENSIONS_PROJECT_ID_1 = 1007777
    private readonly List<KeyValuePair<Guid, long>> GoldenDimensionsProjectMachineIds =
      new List<KeyValuePair<Guid, long>>
      {
        new KeyValuePair<Guid, long>(Guid.Parse("000870FF-F4C8-4D56-99F0-0DD5D5142001"), 1),
        new KeyValuePair<Guid, long>(Guid.Parse("000870FF-F4C8-4D56-99F0-0DD5D5142002"), 2),
        new KeyValuePair<Guid, long>(Guid.Parse("000870FF-F4C8-4D56-99F0-0DD5D5142003"), 3),
        new KeyValuePair<Guid, long>(Guid.Parse("000870FF-F4C8-4D56-99F0-0DD5D5142004"), 4),
        new KeyValuePair<Guid, long>(Guid.Parse("000870FF-F4C8-4D56-99F0-0DD5D5142005"), 5),
        new KeyValuePair<Guid, long>(Guid.Parse("000870FF-F4C8-4D56-99F0-0DD5D5142006"), 6),
        new KeyValuePair<Guid, long>(Guid.Parse("000870FF-F4C8-4D56-99F0-0DD5D5142007"), 7),
        new KeyValuePair<Guid, long>(Guid.Parse("000870FF-F4C8-4D56-99F0-0DD5D5142008"), 8),
        new KeyValuePair<Guid, long>(Guid.Parse("000870FF-F4C8-4D56-99F0-0DD5D5142009"), 9),
        new KeyValuePair<Guid, long>(Guid.Parse("000870FF-F4C8-4D56-99F0-0DD5D5142012"), 1244020666025812),
        new KeyValuePair<Guid, long>(Guid.Parse("000870FF-F4C8-4D56-99F0-0DD5D5142013"), 1219470261494388),
        new KeyValuePair<Guid, long>(Guid.Parse("000870FF-F4C8-4D56-99F0-0DD5D5142014"), 796620067684243),
        new KeyValuePair<Guid, long>(Guid.Parse("000870FF-F4C8-4D56-99F0-0DD5D5142015"), 2894079741131753),
        new KeyValuePair<Guid, long>(Guid.Parse("000870FF-F4C8-4D56-99F0-0DD5D5142016"), 4250986182719752),
        new KeyValuePair<Guid, long>(Guid.Parse("000870FF-F4C8-4D56-99F0-0DD5D5142017"), 3385342403242252),
        new KeyValuePair<Guid, long>(Guid.Parse("000870FF-F4C8-4D56-99F0-0DD5D5142018"), 4101662153056458),
        new KeyValuePair<Guid, long>(Guid.Parse("000870FF-F4C8-4D56-99F0-0DD5D5142019"), 751877972662699),
        new KeyValuePair<Guid, long>(Guid.Parse("000870FF-F4C8-4D56-99F0-0DD5D5142020"), 3517551388324974)
      };
  }
}
