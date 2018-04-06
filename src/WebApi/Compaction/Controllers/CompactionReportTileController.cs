using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using VSS.Productivity3D.WebApiModels.MapHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting tiles for reporting requests
  /// </summary>
  public class CompactionReportTileController : BaseController
  {
    private readonly TileOverlayType[] PROJECT_THUMBNAIL_OVERLAYS =
    {
      TileOverlayType.BaseMap,
      TileOverlayType.ProjectBoundary,
      TileOverlayType.ProductionData
    };

    private const int PROJECT_THUMBNAIL_WIDTH = 220;
    private const int PROJECT_THUMBNAIL_HEIGHT = 182;

    /// <summary>
    /// For retrieving user preferences
    /// </summary>
    private readonly IPreferenceProxy prefProxy;

    /// <summary>
    /// Proxy for getting geofences from master data.
    /// </summary>
    private readonly IGeofenceProxy geofenceProxy;

    /// <summary>
    /// The request factory
    /// </summary>
    private readonly IProductionDataRequestFactory requestFactory;

    /// <summary>
    /// The tile generator
    /// </summary>
    private readonly IMapTileGenerator tileGenerator;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionReportTileController(ILoggerFactory loggerFactory, IConfigurationStore configStore, IGeofenceProxy geofenceProxy,
      IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy, ICompactionSettingsManager settingsManager,
      IServiceExceptionHandler exceptionHandler, IFilterServiceProxy filterServiceProxy, IMapTileGenerator tileGenerator,
      IPreferenceProxy prefProxy, IProductionDataRequestFactory requestFactory)
      : base(loggerFactory, loggerFactory.CreateLogger<CompactionReportTileController>(), exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager)
    {
      this.tileGenerator = tileGenerator;
      this.prefProxy = prefProxy;
      this.geofenceProxy = geofenceProxy;
      this.requestFactory = requestFactory;
    }

    /// <summary>
    /// Gets a tile representing the requested types of data overlayed. 
    /// Types can be any or all of the following: a base map, production data, project boundary, DXF linework, alignments and geofences.
    /// </summary>
    /// <param name="overlays">The types of data to be overlayed</param>
    /// <param name="width">The width, in pixels, of the image tile to be rendered</param>
    /// <param name="height">The height, in pixels, of the image tile to be rendered</param>
    /// <param name="mapType">The base map type</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Filter UID</param>
    /// <param name="cutFillDesignUid">Design UID for cut-fill</param>
    /// <param name="mode">The thematic mode to be rendered; elevation, compaction, temperature etc</param>
    /// <param name="volumeBaseUid">Base Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeTopUid">Top Design or  filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeCalcType">Summary volumes calculation type</param>
    /// <param name="language">Optional language parameter</param> 
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
    /// <executor>CompactionTileExecutor</executor> 
    [ProjectUidVerifier]
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

      var tileResult = await GetGeneratedTile(projectUid, filterUid, cutFillDesignUid, volumeBaseUid, volumeTopUid,
        volumeCalcType, overlays, width, height, mapType, mode, language: language);

      return tileResult;
    }

    /// <summary>
    /// This requests returns raw array of bytes with PNG without any diagnostic information. If it fails refer to the request with disgnostic info.
    /// Gets a tile representing the requested types of data overlayed. 
    /// Types can be any or all of the following: a base map, production data, project boundary, DXF linework, alignments and geofences.
    /// </summary>
    /// <param name="overlays">The types of data to be overlayed</param>
    /// <param name="width">The width, in pixels, of the image tile to be rendered</param>
    /// <param name="height">The height, in pixels, of the image tile to be rendered</param>
    /// <param name="mapType">The base map type</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Filter UID</param>
    /// <param name="cutFillDesignUid">Design UID for cut-fill</param>
    /// <param name="mode">The thematic mode to be rendered; elevation, compaction, temperature etc</param>
    /// <param name="volumeBaseUid">Base Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeTopUid">Top Design or  filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeCalcType">Summary volumes calculation type</param>
    /// <param name="language"></param>    
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request succeeds.</returns>
    /// <executor>CompactionTileExecutor</executor> 
    [ProjectUidVerifier]
    [Route("api/v2/reporttiles/png")]
    [HttpGet]
    [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
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

      var tileResult = await GetGeneratedTile(projectUid, filterUid, cutFillDesignUid, volumeBaseUid, volumeTopUid,
        volumeCalcType, overlays, width, height, mapType, mode, language: language);

      Response.Headers.Add("X-Warning", "false");
      return new FileStreamResult(new MemoryStream(tileResult.TileData), "image/png");
    }

    /// <summary>
    /// Gets a project thumbnail
    /// </summary>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
    [ProjectUidVerifier]
    [Route("api/v2/projectthumbnail")]
    [HttpGet]
    [ResponseCache(Duration = 86400, VaryByQueryKeys = new[] { "*" })]
    public async Task<TileResult> GetProjectThumbnail(
      [FromQuery] Guid projectUid)
    {
      Log.LogDebug("GetProjectThumbnail: " + Request.QueryString);

      var tileResult = await GetGeneratedTile(projectUid, null, null, null, null,
        null, PROJECT_THUMBNAIL_OVERLAYS, PROJECT_THUMBNAIL_WIDTH, PROJECT_THUMBNAIL_HEIGHT, MapType.MAP,
        DisplayMode.Height, true);

      // TODO (Aaron) refactor this repeated code
      //Short-circuit cache time for Archived projects
      if ((User as RaptorPrincipal).GetProject(projectUid).isArchived)
        Response.Headers["Cache-Control"] = "public,max-age=31536000";
      Response.Headers.Add("X-Warning", "false");

      return tileResult;
    }

    /// <summary>
    /// Gets a project thumbnail image.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v2/projectthumbnail/png")]
    [HttpGet]
    [ResponseCache(Duration = 86400, VaryByQueryKeys = new[] { "*" })]
    public async Task<FileResult> GetProjectThumbnailRaw(
      [FromQuery] Guid projectUid)
    {
      Log.LogDebug("GetProjectThumbnailRaw: " + Request.QueryString);

      var tileResult = await GetGeneratedTile(projectUid, null, null, null, null,
        null, PROJECT_THUMBNAIL_OVERLAYS, PROJECT_THUMBNAIL_WIDTH, PROJECT_THUMBNAIL_HEIGHT, MapType.MAP, DisplayMode.Height, true);

      // TODO (Aaron) refactor this repeated code
      //Short-circuit cache time for Archived projects
      if ((User as RaptorPrincipal).GetProject(projectUid).isArchived)
        Response.Headers["Cache-Control"] = "public,max-age=31536000";
      Response.Headers.Add("X-Warning", "false");

      return new FileStreamResult(new MemoryStream(tileResult.TileData), "image/png");
    }

    /// <summary>
    /// Get the generated tile for the request
    /// </summary>
    private async Task<TileResult> GetGeneratedTile(Guid projectUid, Guid? filterUid, Guid? cutFillDesignUid, Guid? volumeBaseUid, Guid? volumeTopUid, VolumeCalcType? volumeCalcType,
      TileOverlayType[] overlays, int width, int height, MapType? mapType, DisplayMode? mode, bool thumbnail = false, string language = null)
    {
      var overlayTypes = overlays.ToList();
      if (overlays.Contains(TileOverlayType.AllOverlays))
      {
        overlayTypes = new List<TileOverlayType>((TileOverlayType[])Enum.GetValues(typeof(TileOverlayType)));
        overlayTypes.Remove(TileOverlayType.AllOverlays);
      }
      var project = (User as RaptorPrincipal).GetProject(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var projectSettingsColors = await GetProjectSettingsColors(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      DesignDescriptor cutFillDesign = cutFillDesignUid.HasValue ? await GetAndValidateDesignDescriptor(projectUid, cutFillDesignUid.Value) : null;
      var sumVolParameters = await GetSummaryVolumesParameters(projectUid, volumeCalcType, volumeBaseUid, volumeTopUid);
      var designDescriptor = (!volumeCalcType.HasValue || volumeCalcType.Value == VolumeCalcType.None)
        ? cutFillDesign
        : sumVolParameters.Item3;
      var dxfFiles = overlayTypes.Contains(TileOverlayType.DxfLinework)
        ? await GetFilesOfType(projectUid, ImportedFileType.Linework)
        : new List<FileData>();
      var alignmentDescriptors = overlayTypes.Contains(TileOverlayType.Alignments)
        ? await GetAlignmentDescriptors(projectUid)
        : new List<DesignDescriptor>();
      language = string.IsNullOrEmpty(language) ? (await GetShortCachedUserPreferences()).Language : language;
      ////var geofences = overlayTypes.Contains(TileOverlayType.Geofences)
      ////  ? await geofenceProxy.GetGeofences((User as RaptorPrincipal).CustomerUid, CustomHeaders)
      ////  : new List<GeofenceData>();
      var geofences = new List<GeofenceData>();

      var request = requestFactory.Create<TileGenerationRequestHelper>(r => r
          .ProjectId(project.projectId)
          .Headers(CustomHeaders)
          .ProjectSettings(projectSettings)
          .ProjectSettingsColors(projectSettingsColors)
          .Filter(filter)
          .DesignDescriptor(designDescriptor))
        .SetBaseFilter(sumVolParameters.Item1)
        .SetTopFilter(sumVolParameters.Item2)
        .SetVolumeCalcType(volumeCalcType)
        .SetGeofences(geofences)
        .SetAlignmentDescriptors(alignmentDescriptors)
        .SetDxfFiles(dxfFiles)
        .SetProject(project)
        .CreateTileGenerationRequest(overlayTypes.ToArray(), width, height, mapType, mode, language);

      request.Validate();

      return await WithServiceExceptionTryExecuteAsync(async () =>
        await tileGenerator.GetMapData(request));
    }

    /// <summary>
    /// Gets the imported files of the specified type in a project
    /// </summary>
    /// <param name="projectUid">The project UID</param>
    /// <param name="fileType">The type of files to retrieve</param>
    /// <returns>List of active imported files of specified type</returns>
    private async Task<List<FileData>> GetFilesOfType(Guid projectUid, ImportedFileType fileType)
    {
      var fileList = await FileListProxy.GetFiles(projectUid.ToString(), GetUserId(), CustomHeaders);
      if (fileList == null || fileList.Count == 0)
      {
        return new List<FileData>();
      }

      return fileList.Where(f => f.ImportedFileType == fileType && f.IsActivated).ToList();
    }

    /// <summary>
    /// Gets alignments descriptors of all active alignment files for the project
    /// </summary>
    /// <param name="projectUid">The project UID</param>
    /// <returns>A list of alignment design descriptors</returns>
    private async Task<List<DesignDescriptor>> GetAlignmentDescriptors(Guid projectUid)
    {
      var alignmentFiles = await GetFilesOfType(projectUid, ImportedFileType.Alignment);
      List<DesignDescriptor> alignmentDescriptors = new List<DesignDescriptor>();
      if (alignmentFiles != null)
      {
        foreach (var alignmentFile in alignmentFiles)
        {
          alignmentDescriptors.Add(await GetAndValidateDesignDescriptor(projectUid, new Guid(alignmentFile.ImportedFileUid)));
        }
      }
      return alignmentDescriptors;
    }

    /// <summary>
    /// Get user preferences
    /// </summary>
    private async Task<UserPreferenceData> GetShortCachedUserPreferences()
    {
      var userPreferences = await prefProxy.GetShortCachedUserPreferences((User as RaptorPrincipal).UserEmail, TimeSpan.FromSeconds(10), CustomHeaders);
      if (userPreferences == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Failed to retrieve preferences for current user"));
      }

      return userPreferences;
    }
  }
}