using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.Productivity3D.AssetMgmt3D.Abstractions.Models;
using VSS.Productivity3D.AssetMgmt3D.Extensions;
using VSS.Productivity3D.AssetMgmt3D.Models;

namespace VSS.Productivity3D.AssetMgmt3D.Controllers
{
  public class AssetMgmtController : BaseController
  {
    private readonly IAssetRepository _assetRepository;

    /// <summary>
    /// Public constructor.
    /// </summary>
    public AssetMgmtController(IAssetRepository assetRepository)
    {
      _assetRepository = assetRepository;
    }

    /// <summary>
    /// Get a list of asset Uid/Id matches for Uids supplied
    /// </summary>
    [HttpPost("api/v1/assets/assetuids")]
    [ProducesResponseType(typeof(List<AssetDisplayModel>), 200)]
    public async Task<IActionResult> GetMatchingAssets(
      [FromBody] List<Guid> assetUids)
    {
      var assetUidDisplay = string.Join(", ", assetUids ?? new List<Guid>());
      Log.LogInformation($"Getting Assets for AssetIds: {assetUidDisplay}");

      var assets = await _assetRepository.GetAssets(assetUids);

      return Json(assets.ConvertDbAssetToDisplayModel());
    }

    /// <summary>
    /// Get a list of asset Uid/Id matches for Ids supplied
    /// </summary>
    [HttpPost("api/v1/assets/assetids")]
    [ProducesResponseType(typeof(List<AssetDisplayModel>), 200)]
    public async Task<IActionResult> GetMatchingAssets(
      [FromBody] List<long> assetIds)
    {
      var assetIdDisplay = string.Join(", ", assetIds ?? new List<long>());
      Log.LogInformation($"Getting Assets for AssetIds: {assetIdDisplay}");

      var assets = await _assetRepository.GetAssets(assetIds);

      return Json(assets.ConvertDbAssetToDisplayModel());
    }

    /// <summary>
    /// Get a potentially matching 2D asset  for a 3D asset
    /// </summary>
    [HttpGet("api/v1/assets/match3dasset/{assetUid}")]
    [ProducesResponseType(typeof(AssetDisplayModel), 200)]
    public async Task<IActionResult> GetMatching2DAssets([FromRoute] Guid assetUid)
    {
      Log.LogInformation($"Getting matching Assets for AssetUID3D: {assetUid}");
      var matchingAsset = new MatchingAssets { AssetUID3D = assetUid.ToString() };
      var result = await _assetRepository.GetMatching3D2DAssets(matchingAsset);

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

      Log.LogInformation($"Returning Asset Display Model for AssetUID3D: {assetUid}. Data: {JsonConvert.SerializeObject(result)}");
      return Json(model);
    }

    /// <summary>
    /// Get a potentially matching 3D asset  for a 2D asset
    /// </summary>
    [HttpGet("api/v1/assets/match2dasset/{assetUid}")]
    [ProducesResponseType(typeof(AssetDisplayModel), 200)]
    public async Task<IActionResult> GetMatching3DAssets([FromRoute] Guid assetUid)
    {
      Log.LogInformation($"Getting matching Assets for AssetUID2D: {assetUid}");
      var matchingAsset = new MatchingAssets { AssetUID2D = assetUid.ToString() };
      var result = await _assetRepository.GetMatching3D2DAssets(matchingAsset);

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
    /// Get location data for a given set of Assets.
    /// </summary>
    /// <remarks>
    /// For use with Dot on the Map polling requests from the UI.
    /// </remarks>
    [HttpPost("api/v1/assets/location")]
    [ProducesResponseType(typeof(AssetDisplayModel), 200)]
    public async Task<IActionResult> GetAssetLocationData([FromBody] List<Guid> assetUids)
    {
      var assetUidDisplay = string.Join(", ", assetUids ?? new List<Guid>());
      Log.LogInformation($"Getting Asset location data for: {assetUidDisplay}");

      var assets = await _assetRepository.GetAssets(assetUids);

      var resultSet = new List<AssetLocationData>(assets.Count());

      foreach (var asset in assets)
      {
        resultSet.Add(new AssetLocationData
        {
          AssetUid = Guid.Parse(asset.AssetUID),
          AssetIdentifier = asset.EquipmentVIN,
          AssetSerialNumber = asset.SerialNumber,
          AssetType = asset.AssetType,
          LocationLastUpdatedUtc = asset.LastActionedUtc,
          MachineName = asset.Name,
          Latitude = 0,
          Longitude = 0,
        });
      }

      Log.LogInformation($"Returning location data for {resultSet.Count} Assets.");
      return Json(resultSet);
    }
  }
}
