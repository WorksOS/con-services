using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting tiles for reporting requests
  /// </summary>
  [Obsolete]
  public class CompactionReportTileController : BaseController<CompactionReportTileController>
  {
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionReportTileController(IConfigurationStore configStore, IFileListProxy fileListProxy, 
      ICompactionSettingsManager settingsManager, ILoggerFactory logger)
      : base(configStore, fileListProxy, settingsManager)
    {
      this.logger = logger;
    }

    /// <summary>
    /// Gets a tile representing the requested types of data overlayed. 
    /// Types can be any or all of the following: a base map, production data, project boundary, DXF linework, alignments and geofences.
    /// </summary>
    [Obsolete("Use tile service instead. This is only used by old scheduled reports.")]
    [ProjectVerifier]
    [Route("api/v2/reporttiles")]
    [HttpGet]
    [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
    public async Task<TileResult> GetReportTile(
      [FromQuery] TileOverlayType[] overlays,
      [FromQuery] int width,
      [FromQuery] int height,
      [FromQuery] MapType? mapType,
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? cutFillDesignUid,
      [FromQuery] DisplayMode? mode,
      [FromQuery] Guid? volumeBaseUid,
      [FromQuery] Guid? volumeTopUid,
      [FromQuery] VolumeCalcType? volumeCalcType,
      [FromQuery] string language = null)
    {
      Log.LogDebug("GetReportTile: " + Request.QueryString);

      var request = new GracefulWebRequest(logger, ConfigStore);
      return await request.ExecuteRequest<TileResult>(GetTileUrl(), null, CustomHeaders, HttpMethod.Get);
    }

    /// <summary>
    /// This requests returns raw array of bytes with PNG without any diagnostic information. If it fails refer to the request with disgnostic info.
    /// Gets a tile representing the requested types of data overlayed. 
    /// Types can be any or all of the following: a base map, production data, project boundary, DXF linework, alignments and geofences.
    /// </summary>
    [ProjectVerifier]
    [Route("api/v2/reporttiles/png")]
    [HttpGet]
    [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
    [Obsolete("Use tile service instead. This is only used by old scheduled reports.")]
    public async Task<FileResult> GetReportTileRaw(
      [FromQuery] TileOverlayType[] overlays,
      [FromQuery] int width,
      [FromQuery] int height,
      [FromQuery] MapType? mapType,
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? cutFillDesignUid,
      [FromQuery] DisplayMode? mode,
      [FromQuery] Guid? volumeBaseUid,
      [FromQuery] Guid? volumeTopUid,
      [FromQuery] VolumeCalcType? volumeCalcType,
      [FromQuery] string language = null)
    {
      Log.LogDebug("GetReportTileRaw: " + Request.QueryString);
      var tileResult = await GetReportTile(overlays, width, height, mapType, projectUid, filterUid, cutFillDesignUid,
        mode, volumeBaseUid, volumeTopUid, volumeCalcType, language);
      Response.Headers.Add("X-Warning", "false");
      return new FileStreamResult(new MemoryStream(tileResult.TileData), "image/png");
    }

    private string GetTileUrl()
    {
      const string tileBaseUrlKey = "TILE_BASE_URL";

      var tileUrl = ConfigStore.GetValueString(tileBaseUrlKey);
      if (string.IsNullOrEmpty(tileUrl))
      {
        throw new Exception($"Missing environment variable {tileBaseUrlKey}");
      }

      return $"{tileUrl}{Request.QueryString}";
    }

    public enum MapType
    {
      MAP,
      SATELLITE,
      HYBRID,
      TERRAIN
    }

  }
}
