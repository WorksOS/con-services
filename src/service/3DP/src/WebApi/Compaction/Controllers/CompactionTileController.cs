using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.WebApi.Compaction.ActionServices;
using VSS.Productivity3D.WebApi.Compaction.Controllers.Filters;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Interfaces;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting tiles for displaying production data and linework.
  /// </summary>
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  [ProjectVerifier]
  public class CompactionTileController : BaseTileController<CompactionTileController>
  {
    /// <summary>
    /// The tile generator
    /// </summary>
    private readonly IProductionDataTileService tileService;

    /// <summary>
    /// Bounding box helper
    /// </summary>
    private readonly IBoundingBoxHelper boundingBoxHelper;

    /// <summary>
    /// Used to talk to TCC
    /// </summary>
    private readonly IFileRepository fileRepo;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionTileController(
      IConfigurationStore configStore,
      IFileRepository fileRepo, IFileImportProxy fileImportProxy, ICompactionSettingsManager settingsManager, IProductionDataTileService tileService, IBoundingBoxHelper boundingBoxHelper) :
      base(configStore, fileImportProxy, settingsManager)
    {
      this.fileRepo = fileRepo;
      this.tileService = tileService;
      this.boundingBoxHelper = boundingBoxHelper;
    }

    /// <summary>
    /// Supplies tiles of rendered overlays for a number of different thematic sets of data held in a project such as 
    /// elevation, compaction, temperature, cut/fill, volumes etc
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
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
    /// <executor>CompactionTileExecutor</executor> 
    [ValidateTileParameters]
    [Route("api/v2/productiondatatiles")]
    [HttpGet]
    public async Task<TileResult> GetProductionDataTile(
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
      Log.LogDebug($"{nameof(GetProductionDataTile)}: " + Request.QueryString);

      var projectId = ((RaptorPrincipal)User).GetLegacyProjectId(projectUid);
      var projectSettings = GetProjectSettingsTargets(projectUid);
      var projectSettingsColors = GetProjectSettingsColors(projectUid);
      var filter = GetCompactionFilter(projectUid, filterUid);
      var cutFillDesign = GetAndValidateDesignDescriptor(projectUid, cutFillDesignUid);
      var sumVolParameters = GetSummaryVolumesParameters(projectUid, volumeCalcType, volumeBaseUid, volumeTopUid);

      await Task.WhenAll(projectId, projectSettings, projectSettingsColors, filter, cutFillDesign, sumVolParameters);

      return await WithServiceExceptionTryExecuteAsync(() => tileService.GetProductionDataTile(
        projectSettings.Result, 
        projectSettingsColors.Result, 
        filter.Result, 
        projectId.Result, 
        projectUid, 
        mode, 
        width, 
        height,
        boundingBoxHelper.GetBoundingBox(bbox), 
        cutFillDesign.Result, 
        sumVolParameters.Result.Item1, 
        sumVolParameters.Result.Item2, 
        sumVolParameters.Result.Item3,
        volumeCalcType, 
        CustomHeaders, 
        explicitFilters));
    }

    /// <summary>
    /// This requests returns raw array of bytes with PNG without any diagnostic information. If it fails refer to the request with disgnostic info.
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
    /// If the size of a pixel in the rendered tile coveres more than 10.88 meters in width or height, then the pixel will be rendered 
    /// in a 'representational style' where black (currently, but there is a work item to allow this to be configurable) is used to 
    /// indicate the presense of data. Representational style rendering performs no filtering what so ever on the data.10.88 meters is 32 
    /// (number of cells across a subgrid) * 0.34 (default width in meters of a single cell).
    /// </returns>
    /// <executor>CompactionTileExecutor</executor> 
    [ValidateTileParameters]
    [Route("api/v2/productiondatatiles/png")]
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
      [FromQuery] bool explicitFilters=false)
    {
      Log.LogDebug($"{nameof(GetProductionDataTileRaw)}: " + Request.QueryString);

      var projectId = ((RaptorPrincipal)User).GetLegacyProjectId(projectUid);
      var projectSettings = GetProjectSettingsTargets(projectUid);
      var projectSettingsColors = GetProjectSettingsColors(projectUid);
      var filter = GetCompactionFilter(projectUid, filterUid);
      var cutFillDesign = GetAndValidateDesignDescriptor(projectUid, cutFillDesignUid);
      var sumVolParameters = GetSummaryVolumesParameters(projectUid, volumeCalcType, volumeBaseUid, volumeTopUid);

      await Task.WhenAll(projectId, projectSettings, projectSettingsColors, filter, cutFillDesign, sumVolParameters);

      var tileResult = await WithServiceExceptionTryExecuteAsync(() => tileService.GetProductionDataTile(
        projectSettings.Result, 
        projectSettingsColors.Result, 
        filter.Result, 
        projectId.Result, 
        projectUid, 
        mode, 
        width, 
        height,
        boundingBoxHelper.GetBoundingBox(bbox), 
        cutFillDesign.Result, 
        sumVolParameters.Result.Item1, 
        sumVolParameters.Result.Item2, 
        sumVolParameters.Result.Item3,
        volumeCalcType, 
        CustomHeaders, 
        explicitFilters));

      Response.Headers.Add("X-Warning", tileResult.TileOutsideProjectExtents.ToString());

      return new FileStreamResult(new MemoryStream(tileResult.TileData), ContentTypeConstants.ImagePng);
    }

    /// <summary>
    /// Supplies tiles of linework for DXF, Alignment and Design surface files imported into a project.
    /// The tiles for the supplied list of files are overlaid and a single tile returned.
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
    /// <param name="fileType">The imported file type for which to to overlay tiles. Valid values are Linework, Alignment and DesignSurface</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
    /// <executor>DxfTileExecutor</executor> 
    [ValidateTileParameters]
    [ValidateWidthAndHeight]
    [Route("api/v2/lineworktiles")]
    [HttpGet]
    public async Task<TileResult> GetLineworkTile(
      [FromQuery] string service,
      [FromQuery] string version,
      [FromQuery] string request,
      [FromQuery] string format,
      [FromQuery] string transparent,
      [FromQuery] string layers,
      [FromQuery] string crs,
      [FromQuery] string styles,
      [FromQuery] int width,
      [FromQuery] int height,
      [FromQuery] string bbox,
      [FromQuery] Guid projectUid,
      [FromQuery] string fileType)
    {
      Log.LogDebug($"{nameof(GetLineworkTile)}: " + Request.QueryString);

      var requiredFiles = await ValidateFileType(projectUid, fileType);
      var dxfTileRequest = DxfTileRequest.CreateTileRequest(requiredFiles, boundingBoxHelper.GetBoundingBox(bbox));

      dxfTileRequest.Validate();
#if RAPTOR
      var executor = RequestExecutorContainerFactory.Build<DxfTileExecutor>(LoggerFactory, RaptorClient, null, ConfigStore, fileRepo);
      return await executor.ProcessAsync(dxfTileRequest) as TileResult;
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }

    /// <summary>
    /// This requests returns raw array of bytes with PNG without any diagnostic information. If it fails refer to the request with disgnostic info.
    /// Supplies tiles of linework for DXF, Alignment and Design surface files imported into a project.
    /// The tiles for the supplied list of files are overlaid and a single tile returned.
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
    /// <param name="fileType">The imported file type for which to to overlay tiles. Valid values are Linework, Alignment and DesignSurface</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
    /// <executor>DxfTileExecutor</executor> 
    [ValidateTileParameters]
    [Route("api/v2/lineworktiles/png")]
    [HttpGet]
    public async Task<FileResult> GetLineworkTileRaw(
      [FromQuery] string service,
      [FromQuery] string version,
      [FromQuery] string request,
      [FromQuery] string format,
      [FromQuery] string transparent,
      [FromQuery] string layers,
      [FromQuery] string crs,
      [FromQuery] string styles,
      [FromQuery] int width,
      [FromQuery] int height,
      [FromQuery] string bbox,
      [FromQuery] Guid projectUid,
      [FromQuery] string fileType)
    {
      Log.LogDebug($"{nameof(GetLineworkTileRaw)}: " + Request.QueryString);

      var requiredFiles = await ValidateFileType(projectUid, fileType);
      var dxfTileRequest = DxfTileRequest.CreateTileRequest(requiredFiles, boundingBoxHelper.GetBoundingBox(bbox));

      dxfTileRequest.Validate();
#if RAPTOR
      var executor = RequestExecutorContainerFactory.Build<DxfTileExecutor>(LoggerFactory, RaptorClient, null, ConfigStore, fileRepo);
      var result = await executor.ProcessAsync(dxfTileRequest) as TileResult;

      if (result?.TileData == null)
        result = TileResult.EmptyTile(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE);

      return new FileStreamResult(new MemoryStream(result.TileData), ContentTypeConstants.ImagePng);
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }

    /// <summary>
    /// Validates the file type for DXF tile request and gets the imported file data for it
    /// </summary>
    /// <param name="projectUid">The project UID where the files were imported</param>
    /// <param name="fileType">The file type of the imported files</param>
    /// <returns>The imported file data for the requested files</returns>
    private async Task<List<FileData>> ValidateFileType(Guid projectUid, string fileType)
    {
      //Check file type specified
      if (string.IsNullOrEmpty(fileType))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing file type"));
      }

      //Check file type is valid
      if (Enum.TryParse(fileType, true, out ImportedFileType importedFileType))
      {
        if (importedFileType != ImportedFileType.Linework &&
            importedFileType != ImportedFileType.Alignment &&
            importedFileType != ImportedFileType.DesignSurface)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Unsupported file type " + fileType));
        }
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid file type " + fileType));
      }

      //Get all the imported files for the project
      var fileList = await FileImportProxy.GetFiles(projectUid.ToString(), GetUserId(), Request.Headers.GetCustomHeaders()) ?? new List<FileData>();

      //Select the required ones from the list
      var filesOfType = fileList.Where(f => f.ImportedFileType == importedFileType && f.IsActivated).ToList();
      Log.LogInformation("Found {0} files of type {1} from a total of {2}", filesOfType.Count, fileType, fileList.Count);

      return filesOfType;
    }
  }
}
