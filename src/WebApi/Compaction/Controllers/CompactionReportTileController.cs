using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using System.IO;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using VSS.Productivity3D.WebApiModels.MapHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting tiles for reporting requests
  /// </summary>
  [ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
  public class CompactionReportTileController : BaseController
  {
    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

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
    /// Constructor with injection
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <param name="configStore">Configuration store</param>
    /// <param name="geofenceProxy">Configuration store</param>
    /// <param name="fileListProxy">File list proxy</param>
    /// <param name="projectSettingsProxy">Project settings proxy</param>
    /// <param name="settingsManager">Compaction settings manager</param>
    /// <param name="exceptionHandler">Service exception handler</param>
    /// <param name="filterServiceProxy">Filter service proxy</param>
    /// <param name="tileGenerator">Tile generator</param>
    /// <param name="prefProxy">User preferences proxy</param>
    /// <param name="requestFactory">The request factory.</param>
    public CompactionReportTileController(ILoggerFactory logger, IConfigurationStore configStore, IGeofenceProxy geofenceProxy,
      IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy, ICompactionSettingsManager settingsManager,
      IServiceExceptionHandler exceptionHandler, IFilterServiceProxy filterServiceProxy, IMapTileGenerator tileGenerator, 
      IPreferenceProxy prefProxy, IProductionDataRequestFactory requestFactory) 
      : base(logger.CreateLogger<BaseController>(), exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager)
    {
      this.logger = logger;
      this.log = logger.CreateLogger<CompactionDataController>();
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
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
    /// <executor>CompactionTileExecutor</executor> 
    [ProjectUidVerifier]
    [Route("api/v2/compaction/reporttiles")]
    [HttpGet]
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
      [FromQuery] VolumeCalcType? volumeCalcType)
    {
      log.LogDebug("GetReportTile: " + Request.QueryString);

      var tileResult = await GetGeneratedTile(projectUid, filterUid, cutFillDesignUid, volumeBaseUid, volumeTopUid,
        volumeCalcType, overlays, width, height, mapType, mode);

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
    /// <param name="volumeCalcType">Summary volumes calculation type</param>    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request succeeds. 
    /// </returns>
    /// <executor>CompactionTileExecutor</executor> 
    [ProjectUidVerifier]
    [Route("api/v2/compaction/reporttiles/png")]
    [HttpGet]
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
      [FromQuery] VolumeCalcType? volumeCalcType)
    {
      log.LogDebug("GetReportTileRaw: " + Request.QueryString);

      var tileResult = await GetGeneratedTile(projectUid, filterUid, cutFillDesignUid, volumeBaseUid, volumeTopUid,
        volumeCalcType, overlays, width, height, mapType, mode);

      Response.Headers.Add("X-Warning", "false");
      return new FileStreamResult(new MemoryStream(tileResult.TileData), "image/png");
    }

    /// <summary>
    /// Get the generated tile for the request
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="filterUid"></param>
    /// <param name="cutFillDesignUid"></param>
    /// <param name="volumeBaseUid"></param>
    /// <param name="volumeTopUid"></param>
    /// <param name="volumeCalcType"></param>
    /// <param name="overlays"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="mapType"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    private async Task<TileResult> GetGeneratedTile(Guid projectUid, Guid? filterUid, Guid? cutFillDesignUid, Guid? volumeBaseUid, Guid? volumeTopUid, VolumeCalcType? volumeCalcType,
      TileOverlayType[] overlays, int width, int height, MapType? mapType, DisplayMode? mode)
    {
      var project = (User as RaptorPrincipal).GetProject(projectUid);
      var projectSettings = await GetProjectSettings(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      DesignDescriptor cutFillDesign = cutFillDesignUid.HasValue ? await GetAndValidateDesignDescriptor(projectUid, cutFillDesignUid.Value) : null;
      var sumVolParameters = await GetSummaryVolumesParameters(projectUid, volumeCalcType, volumeBaseUid, volumeTopUid);
      var designDescriptor = (!volumeCalcType.HasValue || volumeCalcType.Value == VolumeCalcType.None)
        ? cutFillDesign
        : sumVolParameters.Item3;
      var dxfFiles = await GetFilesOfType(projectUid, ImportedFileType.Linework);
      var alignmentDescriptors = await GetAlignmentDescriptors(projectUid);
      var userPreferences = await GetUserPreferences();
      var geofences = await geofenceProxy.GetGeofences((User as RaptorPrincipal).CustomerUid, CustomHeaders);

      var request = requestFactory.Create<TileGenerationRequestHelper>(r => r
          .ProjectId(project.projectId)
          .Headers(CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter)
          .DesignDescriptor(designDescriptor))
        .SetBaseFilter(sumVolParameters.Item1)
        .SetTopFilter(sumVolParameters.Item2)
        .SetVolumeCalcType(volumeCalcType)
        .SetGeofences(geofences)
        .SetAlignmentDescriptors(alignmentDescriptors)
        .SetDxfFiles(dxfFiles)
        .SetProject(project)
        .CreateTileGenerationRequest(overlays, width, height, mapType, mode, userPreferences.Language);

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
      var fileList = await FileListProxy.GetFiles(projectUid.ToString(), CustomHeaders);
      if (fileList == null || fileList.Count == 0)
      {
        return null;
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
      List<DesignDescriptor> alignmentDescriptors = null;
      if (alignmentFiles != null)
      {
        alignmentDescriptors = new List<DesignDescriptor>();
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
    /// <returns></returns>
    private async Task<UserPreferenceData> GetUserPreferences()
    {
      var userPreferences = await prefProxy.GetUserPreferences(CustomHeaders);
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
