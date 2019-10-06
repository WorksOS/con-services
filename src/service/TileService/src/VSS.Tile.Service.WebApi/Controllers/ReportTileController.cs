using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Tile.Service.Common.Authentication;
using VSS.Tile.Service.Common.Interfaces;
using VSS.Tile.Service.Common.Services;
using VSS.WebApi.Common;

namespace VSS.Tile.Service.WebApi.Controllers
{
  public class ReportTileController : BaseController<ReportTileController>
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public ReportTileController(IProductivity3dV2ProxyCompactionTile productivity3DProxyCompactionTile, IPreferenceProxy prefProxy, IFileImportProxy fileImportProxy,
      IMapTileGenerator tileGenerator, IGeofenceProxy geofenceProxy, IMemoryCache cache, IConfigurationStore configStore,
      IBoundingBoxHelper boundingBoxHelper, IDataOceanClient dataOceanClient, ITPaaSApplicationAuthentication authn)
      : base(productivity3DProxyCompactionTile, prefProxy, fileImportProxy, tileGenerator, geofenceProxy, cache, configStore, boundingBoxHelper, authn)
    {
    }

    /// <summary>
    /// This requests returns raw array of bytes with PNG without any diagnostic information. If it fails refer to the request with disgnostic info.
    /// Gets a tile representing the requested types of data overlayed. 
    /// Types can be any or all of the following: a base map, production data, project boundary, DXF linework, alignments and geofences.
    /// </summary>
    /// <param name="overlays">The types of data to be overlayed</param>
    /// <param name="width">The width, in pixels, of the image tile to be rendered</param>
    /// <param name="height">The height, in pixels, of the image tile to be rendered</param>
    /// <param name="bbox">The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng (optional).
    /// If not provided, the service will calculate a "best fit" based on the provided query parameters.</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="mapType">The base map type</param>
    /// <param name="filterUid">Filter UID</param>
    /// <param name="cutFillDesignUid">Design UID for cut-fill</param>
    /// <param name="mode">The thematic mode to be rendered; elevation, compaction, temperature etc</param>
    /// <param name="volumeBaseUid">Base Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeTopUid">Top Design or  filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeCalcType">Summary volumes calculation type</param>
    /// <param name="language"></param>   
    /// <param name="explicitFilters">If true then filters should not undergo mutation i.e. no lookbacks</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request succeeds.</returns>
    [ProjectUidVerifier]
    [Route("api/v1/reporttiles/png")]
    [HttpGet]
    public async Task<FileResult> GetReportTileRaw(
      [FromQuery] TileOverlayType[] overlays,
      [FromQuery] int width,
      [FromQuery] int height,
      [FromQuery] string bbox,
      [FromQuery] Guid projectUid,
      [FromQuery] MapType? mapType,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? cutFillDesignUid,
      [FromQuery] DisplayMode? mode,
      [FromQuery] Guid? volumeBaseUid,
      [FromQuery] Guid? volumeTopUid,
      [FromQuery] VolumeCalcType? volumeCalcType,
      [FromQuery] string language = null,
      [FromQuery] bool explicitFilters = false)
    {
      Log.LogDebug("GetReportTileRaw: " + Request.QueryString);

      //We need this check up front since accummulating the data (e.g. getting bbox) 
      //required for generating the tile uses this and gives exceptions
      if (overlays == null || overlays.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "At least one type of map tile overlay must be specified"));
      }

      var tileResult = await GetGeneratedTile(projectUid, filterUid, cutFillDesignUid, volumeBaseUid, volumeTopUid,
        volumeCalcType, overlays, width, height, bbox, mapType, mode, language, string.IsNullOrEmpty(bbox), explicitFilters);

      Response.Headers.Add("X-Warning", "false");
      return tileResult;
    }
  }
}
