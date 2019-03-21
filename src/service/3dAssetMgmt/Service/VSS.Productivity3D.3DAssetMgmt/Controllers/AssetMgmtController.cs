using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.AssetMgmt3D.Models;

namespace VSS.Productivity3D.AssetMgmt3D.Controllers
{

  public class AssetMgmtController : BaseController
  {

    private IAssetRepository assetRepository;

    public AssetMgmtController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IAssetRepository assetRepository)
      : base(loggerFactory, serviceExceptionHandler)
    {
      this.assetRepository = assetRepository;
    }

    /// <summary>
    /// Get a list of customers, projects and files for me
    /// </summary>
    /// <response code="200">A list of customers you can currently access.</response>
    /// <response code="403">Invalid access token provided</response>
    [HttpPost("api/v1/assets/assetuids")]
    [ProducesResponseType(typeof(List<AssetDisplayModel>), 200)]
    public async Task<IActionResult> GetMatchingAssets([FromQuery] List<Guid> assetUids)
    {
      var results = (await assetRepository.GetAssets(assetUids)).Select(asset =>
      {
        return new KeyValuePair<Guid, long>(Guid.Parse(asset.AssetUID), asset.LegacyAssetID);
      });

      return Json(new AssetDisplayModel { assetIdentifiers = results });
    }

    /// <summary>
    /// Get a list of customers, projects and files for me
    /// </summary>
    /// <response code="200">A list of customers you can currently access.</response>
    /// <response code="403">Invalid access token provided</response>
    [HttpPost("api/v1/assets/assetids")]
    [ProducesResponseType(typeof(List<AssetDisplayModel>), 200)]
    public async Task<IActionResult> GetMatchingAssets([FromQuery] List<long> assetIds)
    {
      var results = (await assetRepository.GetAssets(assetIds)).Select(asset =>
      {
        return new KeyValuePair<Guid, long>(Guid.Parse(asset.AssetUID), asset.LegacyAssetID);
      });

      return Json(new AssetDisplayModel { assetIdentifiers = results });
    }

  }
}
