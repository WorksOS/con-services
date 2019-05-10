using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.Productivity3D.AssetMgmt3D.Abstractions.Models;
using VSS.Productivity3D.AssetMgmt3D.Models;

namespace VSS.Productivity3D.AssetMgmt3D.Controllers
{

  public class AssetMgmtController : BaseController
  {
    private readonly IAssetRepository assetRepository;

    public AssetMgmtController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler,
      IAssetRepository assetRepository)
      : base(loggerFactory, serviceExceptionHandler)
    {
      this.assetRepository = assetRepository;
    }

    /// <summary>
    /// Get a list of asset Uid/Id matches for Uids supplied
    /// </summary>
    /// <response code="200">A list of matched assets.</response>
    /// <response code="403">Invalid access token provided</response>
    [HttpPost("api/v1/assets/assetuids")]
    [ProducesResponseType(typeof(List<AssetDisplayModel>), 200)]
    public async Task<IActionResult> GetMatchingAssets([FromBody] List<Guid> assetUids)
    {
      var assetUidDisplay = string.Join(", ", assetUids ?? new List<Guid>());
      Log.LogInformation($"Getting Assets for AssetIds: {assetUidDisplay}");

      var assets = await assetRepository.GetAssets(assetUids);
      var displayModel = ConvertDbAssetToDisplayModel(assets);
      return Json(displayModel);
    }

    /// <summary>
    /// Get a list of asset Uid/Id matches for Ids supplied
    /// </summary>
    /// <response code="200">A list of matched assets.</response>
    /// <response code="403">Invalid access token provided</response>
    [HttpPost("api/v1/assets/assetids")]
    [ProducesResponseType(typeof(List<AssetDisplayModel>), 200)]
    public async Task<IActionResult> GetMatchingAssets([FromBody] List<long> assetIds)
    {
      var assetIdDisplay = string.Join(", ", assetIds ?? new List<long>());
      Log.LogInformation($"Getting Assets for AssetIds: {assetIdDisplay}");

      var assets = await assetRepository.GetAssets(assetIds);
      var displayModel = ConvertDbAssetToDisplayModel(assets);
      return Json(displayModel);
    }

    /// <summary>
    /// Get a potentially matching 2D asset  for a 3D asset
    /// </summary>
    /// <response code="200">A list of matched assets.</response>
    /// <response code="403">Invalid access token provided</response>
    [HttpGet("api/v1/assets/match3dasset/{assetUid}")]
    [ProducesResponseType(typeof(AssetDisplayModel), 200)]
    public async Task<IActionResult> GetMatching2DAssets([FromRoute] Guid assetUid)
    {
      Log.LogInformation($"Getting matching Assets for AssetUID3D: {assetUid}");
      var matchingAsset = new MatchingAssets() {AssetUID3D = assetUid.ToString()};
      var result = await assetRepository.GetMatching3D2DAssets(matchingAsset);

      var model = result == null
        ? new MatchingAssetsDisplayModel((int) AssetMgmt3DExecutionStates.ErrorCodes.NoMatchingAssets, "No matching assets found")
        : new MatchingAssetsDisplayModel
        {
          AssetUID3D = result.AssetUID3D,
          CustomerName = result.CustomerName,
          AssetUID2D = result.AssetUID2D,
          MakeCode2D = result.MakeCode2D,
          MakeCode3D = result.MakeCode3D,
          SerialNumber3D = result.SerialNumber3D,
          Model = result.Model,
          Name = result.Name,
          SerialNumber2D = result.SerialNumber2D
        };

      Log.LogInformation($"Returning Asset Display Model for AssetUID3D: {assetUid}. Data: {JsonConvert.SerializeObject(result)}");
      return Json(model);
    }

    /// <summary>
    /// Get a potentially matching 3D asset  for a 2D asset
    /// </summary>
    /// <response code="200">A list of matched assets.</response>
    /// <response code="403">Invalid access token provided</response>
    [HttpGet("api/v1/assets/match2dasset/{assetUid}")]
    [ProducesResponseType(typeof(AssetDisplayModel), 200)]
    public async Task<IActionResult> GetMatching3DAssets([FromRoute] Guid assetUid)
    {
      Log.LogInformation($"Getting matching Assets for AssetUID2D: {assetUid}");
      var matchingAsset = new MatchingAssets() { AssetUID2D = assetUid.ToString() };
      var result = await assetRepository.GetMatching3D2DAssets(matchingAsset);

      var model = result == null
        ? new MatchingAssetsDisplayModel((int)AssetMgmt3DExecutionStates.ErrorCodes.NoMatchingAssets, "No matching assets found")
        : new MatchingAssetsDisplayModel
        {
          AssetUID3D = result.AssetUID3D,
          CustomerName = result.CustomerName,
          AssetUID2D = result.AssetUID2D,
          MakeCode2D = result.MakeCode2D,
          MakeCode3D = result.MakeCode3D,
          SerialNumber3D = result.SerialNumber3D,
          Model = result.Model,
          Name = result.Name,
          SerialNumber2D = result.SerialNumber2D
        };

      Log.LogInformation($"Returning Asset Display Model for AssetUID2D: {assetUid}. Data: {JsonConvert.SerializeObject(result)}");
      return Json(model);
    }

    /// <summary>
    /// Convert a List of Asset Database Models to Display Models, validating the Guid string can be parsed
    /// </summary>
    private AssetDisplayModel ConvertDbAssetToDisplayModel(IEnumerable<Asset> assets)
    {
      var results = assets.Select(a =>
      {
        if (Guid.TryParse(a.AssetUID, out var g))
          return new KeyValuePair<Guid, long>(g, a.LegacyAssetID);

        Log.LogWarning($"Failed to parse {a.AssetUID} to a guid for AssetID: {a.LegacyAssetID}");
        return new KeyValuePair<Guid, long>(Guid.Empty, a.LegacyAssetID);
      }).ToList();

      Log.LogInformation($"Matched assets: {JsonConvert.SerializeObject(results)}");

      return new AssetDisplayModel
      {
        assetIdentifiers = results
      };
    }
  }
}
