using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Tile.Service.Common.Filters;
using VSS.Tile.Service.Common.Interfaces;
using VSS.Tile.Service.Common.Services;
using VSS.WebApi.Common;

namespace VSS.Tile.Service.WebApi.Controllers
{
  public class ProductionDataTileController : BaseController<ThumbnailController>
  {
    public ProductionDataTileController(IProductivity3dV2ProxyCompactionTile productivity3DProxyCompactionTile, IPreferenceProxy prefProxy, IFileImportProxy fileImportProxy,
      IMapTileGenerator tileGenerator, IMemoryCache cache, IConfigurationStore configStore,
      IBoundingBoxHelper boundingBoxHelper, ITPaaSApplicationAuthentication authn)
      : base(productivity3DProxyCompactionTile, prefProxy, fileImportProxy, tileGenerator, cache, configStore, boundingBoxHelper, authn)
    {
    }

    /// <summary>
    /// This requests returns raw array of bytes with PNG without any diagnostic information. If it fails refer to the request with diagnostic info.
    /// Supplies tiles of rendered overlays for a number of different thematic sets of data held in a project such as elevation, compaction, temperature, cut/fill, volumes etc
    /// </summary>
    /// <param name="service">WMS parameter - value WMS</param>
    /// <param name="version">WMS parameter - value 1.3.0</param>
    /// <param name="request">WMS parameter - value GetMap</param>
    /// <param name="format">WMS parameter - value image/png</param>
    /// <param name="transparent">WMS parameter - value true</param>
    /// <param name="layers">WMS parameter - value Layers</param>
    /// <param name="crs">WMS parameter - value EPSG:4326</param>
    /// <param name="styles">WMS parameter - value null</param>
    /// <param name="width">The width, in pixels, of the image tile to be rendered, usually 256</param>
    /// <param name="height">The height, in pixels, of the image tile to be rendered, usually 256</param>
    /// <param name="bbox">The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Filter UID</param>
    /// <param name="cutFillDesignUid">Design UID for cut-fill</param>
    /// <param name="mode">The thematic mode to be rendered; elevation, compaction, temperature etc</param>
    /// <param name="volumeBaseUid">Base Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeTopUid">Top Design or  filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeCalcType">Summary volumes calculation type</param>
    /// <param name="explicitFilters"></param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request succeeds. 
    /// If the size of a pixel in the rendered tile covers more than 10.88 meters in width or height, then the pixel will be rendered 
    /// in a 'representational style' where black (currently, but there is a work item to allow this to be configurable) is used to 
    /// indicate the presence of data. Representational style rendering performs no filtering what so ever on the data.10.88 meters is 32 
    /// (number of cells across a sub grid) * 0.34 (default width in meters of a single cell).
    /// </returns>
    [ValidateTileParameters]
    [Route("api/v1/productiondatatiles/png")]
    [HttpGet]
    public async Task<FileResult> GetProductionDataTileRaw(
      [FromQuery] string service,
      [FromQuery] string version,
      [FromQuery] string request,
      [FromQuery] string format,
      [FromQuery] string transparent,
      [FromQuery] string layers,
      [FromQuery] string crs,
      [FromQuery] string styles,
      [FromQuery] ushort width,
      [FromQuery] ushort height,
      [FromQuery] string bbox,
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? cutFillDesignUid,
      [FromQuery] DisplayMode mode,
      [FromQuery] Guid? volumeBaseUid,
      [FromQuery] Guid? volumeTopUid,
      [FromQuery] VolumeCalcType? volumeCalcType,
      [FromQuery] bool explicitFilters = false)
    {
      Log.LogDebug($"{nameof(GetProductionDataTileRaw)}: " + Request.QueryString);

      var bitmap = await productivity3DProxyCompactionTile.GetProductionDataTile(projectUid, filterUid,
        cutFillDesignUid, width, height,
        bbox, mode, volumeBaseUid, volumeTopUid, volumeCalcType, CustomHeaders, explicitFilters);

      return new FileStreamResult(new MemoryStream(bitmap), ContentTypeConstants.ImagePng);
    }
  }
}

