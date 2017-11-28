using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Notification.Helpers;
using VSS.Productivity3D.WebApiModels.Compaction.Executors;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Productivity3D.WebApiModels.Compaction.Models;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting tiles for displaying production data and linework.
  /// </summary>
  [ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
  public class CompactionTileController : BaseController
  {
    /// <summary>
    /// Raptor client for use by executor
    /// 
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Used to talk to TCC
    /// </summary>
    private readonly IFileRepository fileRepo;

    /// <summary>
    /// Proxy for getting elevation statistics from Raptor
    /// </summary>
    private readonly IElevationExtentsProxy elevProxy;

    /// <summary>
    /// The request factory
    /// </summary>
    private readonly IProductionDataRequestFactory requestFactory;

    /// <summary>
    /// Constructor with injected raptor client, logger and authenticated projects
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="configStore">Configuration store</param>
    /// <param name="fileRepo">Imported file repository</param>
    /// <param name="elevProxy">Elevation extents proxy</param>
    /// <param name="fileListProxy">File list proxy</param>
    /// <param name="projectSettingsProxy">Project settings proxy</param>
    /// <param name="settingsManager">Compaction settings manager</param>
    /// <param name="requestFactory">The request factory.</param>
    /// <param name="exceptionHandler">Service exception handler</param>
    /// <param name="filterServiceProxy">Filter service proxy</param>
    public CompactionTileController(IASNodeClient raptorClient, ILoggerFactory logger, IConfigurationStore configStore,
      IFileRepository fileRepo, IElevationExtentsProxy elevProxy, IFileListProxy fileListProxy,
      IProjectSettingsProxy projectSettingsProxy, ICompactionSettingsManager settingsManager,
      IProductionDataRequestFactory requestFactory, IServiceExceptionHandler exceptionHandler, IFilterServiceProxy filterServiceProxy) :
      base(logger.CreateLogger<BaseController>(), exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<CompactionTileController>();
      this.fileRepo = fileRepo;
      this.elevProxy = elevProxy;
      this.requestFactory = requestFactory;
    }

    /// <summary>
    /// Supplies tiles of rendered overlays for a number of different thematic sets of data held in a project such as 
    /// elevation, compaction, temperature, cut/fill, volumes etc
    /// </summary>
    /// <param name="SERVICE">WMS parameter - value WMS</param>
    /// <param name="VERSION">WMS parameter - value 1.3.0</param>
    /// <param name="REQUEST">WMS parameter - value GetMap</param>
    /// <param name="FORMAT">WMS parameter - value image/png</param>
    /// <param name="TRANSPARENT">WMS parameter - value true</param>
    /// <param name="LAYERS">WMS parameter - value Layers</param>
    /// <param name="CRS">WMS parameter - value EPSG:4326</param>
    /// <param name="STYLES">WMS parameter - value null</param>
    /// <param name="WIDTH">The width, in pixels, of the image tile to be rendered, usually 256</param>
    /// <param name="HEIGHT">The height, in pixels, of the image tile to be rendered, usually 256</param>
    /// <param name="BBOX">The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Filter UID</param>
    /// <param name="cutFillDesignUid">Design UID for cut-fill</param>
    /// <param name="mode">The thematic mode to be rendered; elevation, compaction, temperature etc</param>
    /// <param name="volumeBaseUid">Base Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeTopUid">Top Design or  filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeCalcType">Summary volumes calculation type</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
    /// <executor>TilesExecutor</executor> 
    [ProjectUidVerifier]
    [Route("api/v2/compaction/productiondatatiles")]
    [HttpGet]
    public async Task<TileResult> GetProductionDataTile(
      [FromQuery] string SERVICE,
      [FromQuery] string VERSION,
      [FromQuery] string REQUEST,
      [FromQuery] string FORMAT,
      [FromQuery] string TRANSPARENT,
      [FromQuery] string LAYERS,
      [FromQuery] string CRS,
      [FromQuery] string STYLES,
      [FromQuery] int WIDTH,
      [FromQuery] int HEIGHT,
      [FromQuery] string BBOX,
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? cutFillDesignUid,
      [FromQuery] DisplayMode mode,
      [FromQuery] Guid? volumeBaseUid,
      [FromQuery] Guid? volumeTopUid,
      [FromQuery] VolumeCalcType? volumeCalcType)
    {
      log.LogDebug("GetProductionDataTile: " + Request.QueryString);

      ValidateWmsParameters(SERVICE, VERSION, REQUEST, FORMAT, TRANSPARENT, LAYERS, CRS, STYLES);
      var projectSettings = await GetProjectSettings(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);

      DesignDescriptor cutFillDesign = cutFillDesignUid.HasValue
        ? await GetAndValidateDesignDescriptor(projectUid, cutFillDesignUid.Value)
        : null;

      var sumVolParameters = await GetSummaryVolumesParameters(projectUid, volumeCalcType, volumeBaseUid, volumeTopUid);

      var tileResult = GetProductionDataTile(projectSettings, filter, projectUid, mode, (ushort)WIDTH, (ushort)HEIGHT,
         GetBoundingBox(BBOX), cutFillDesign, sumVolParameters, volumeCalcType, volumeBaseUid, volumeTopUid);

      return tileResult;
    }

    /// <summary>
    /// This requests returns raw array of bytes with PNG without any diagnostic information. If it fails refer to the request with disgnostic info.
    /// Supplies tiles of rendered overlays for a number of different thematic sets of data held in a project such as elevation, compaction, temperature, cut/fill, volumes etc
    /// </summary>
    /// <param name="SERVICE">WMS parameter - value WMS</param>
    /// <param name="VERSION">WMS parameter - value 1.3.0</param>
    /// <param name="REQUEST">WMS parameter - value GetMap</param>
    /// <param name="FORMAT">WMS parameter - value image/png</param>
    /// <param name="TRANSPARENT">WMS parameter - value true</param>
    /// <param name="LAYERS">WMS parameter - value Layers</param>
    /// <param name="CRS">WMS parameter - value EPSG:4326</param>
    /// <param name="STYLES">WMS parameter - value null</param>
    /// <param name="WIDTH">The width, in pixels, of the image tile to be rendered, usually 256</param>
    /// <param name="HEIGHT">The height, in pixels, of the image tile to be rendered, usually 256</param>
    /// <param name="BBOX">The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Filter UID</param>
    /// <param name="cutFillDesignUid">Design UID for cut-fill</param>
    /// <param name="mode">The thematic mode to be rendered; elevation, compaction, temperature etc</param>
    /// <param name="volumeBaseUid">Base Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeTopUid">Top Design or  filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeCalcType">Summary volumes calculation type</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request succeeds. 
    /// If the size of a pixel in the rendered tile coveres more than 10.88 meters in width or height, then the pixel will be rendered 
    /// in a 'representational style' where black (currently, but there is a work item to allow this to be configurable) is used to 
    /// indicate the presense of data. Representational style rendering performs no filtering what so ever on the data.10.88 meters is 32 
    /// (number of cells across a subgrid) * 0.34 (default width in meters of a single cell).
    /// </returns>
    /// <executor>TilesExecutor</executor> 
    [ProjectUidVerifier]
    [Route("api/v2/compaction/productiondatatiles/png")]
    [HttpGet]
    public async Task<FileResult> GetProductionDataTileRaw(
      [FromQuery] string SERVICE,
      [FromQuery] string VERSION,
      [FromQuery] string REQUEST,
      [FromQuery] string FORMAT,
      [FromQuery] string TRANSPARENT,
      [FromQuery] string LAYERS,
      [FromQuery] string CRS,
      [FromQuery] string STYLES,
      [FromQuery] int WIDTH,
      [FromQuery] int HEIGHT,
      [FromQuery] string BBOX,
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? cutFillDesignUid,
      [FromQuery] DisplayMode mode,
      [FromQuery] Guid? volumeBaseUid,
      [FromQuery] Guid? volumeTopUid,
      [FromQuery] VolumeCalcType? volumeCalcType)
    {
      log.LogDebug("GetProductionDataTileRaw: " + Request.QueryString);

      ValidateWmsParameters(SERVICE, VERSION, REQUEST, FORMAT, TRANSPARENT, LAYERS, CRS, STYLES);
      var projectSettings = await GetProjectSettings(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);

      DesignDescriptor cutFillDesign = cutFillDesignUid.HasValue
        ? await GetAndValidateDesignDescriptor(projectUid, cutFillDesignUid.Value)
        : null;

      var sumVolParameters = await GetSummaryVolumesParameters(projectUid, volumeCalcType, volumeBaseUid, volumeTopUid);
      var tileResult = GetProductionDataTile(projectSettings, filter, projectUid, mode, (ushort)WIDTH, (ushort)HEIGHT,
        GetBoundingBox(BBOX), cutFillDesign, sumVolParameters, volumeCalcType, volumeBaseUid, volumeTopUid);

      Response.Headers.Add("X-Warning", tileResult.TileOutsideProjectExtents.ToString());

      return new FileStreamResult(new MemoryStream(tileResult.TileData), "image/png");
    }

    /// <summary>
    /// Supplies tiles of linework for DXF, Alignment and Design surface files imported into a project.
    /// The tiles for the supplied list of files are overlaid and a single tile returned.
    /// </summary>
    /// <param name="SERVICE">WMS parameter - value WMS</param>
    /// <param name="VERSION">WMS parameter - value 1.3.0</param>
    /// <param name="REQUEST">WMS parameter - value GetMap</param>
    /// <param name="FORMAT">WMS parameter - value image/png</param>
    /// <param name="TRANSPARENT">WMS parameter - value true</param>
    /// <param name="LAYERS">WMS parameter - value Layers</param>
    /// <param name="CRS">WMS parameter - value EPSG:4326</param>
    /// <param name="STYLES">WMS parameter - value null</param>
    /// <param name="WIDTH">The width, in pixels, of the image tile to be rendered, usually 256</param>
    /// <param name="HEIGHT">The height, in pixels, of the image tile to be rendered, usually 256</param>
    /// <param name="BBOX">The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="fileType">The imported file type for which to to overlay tiles. Valid values are Linework, Alignment and DesignSurface</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
    /// <executor>TilesExecutor</executor> 
    [ProjectUidVerifier]
    [Route("api/v2/compaction/lineworktiles")]
    [HttpGet]
    public async Task<TileResult> GetLineworkTile(
      [FromQuery] string SERVICE,
      [FromQuery] string VERSION,
      [FromQuery] string REQUEST,
      [FromQuery] string FORMAT,
      [FromQuery] string TRANSPARENT,
      [FromQuery] string LAYERS,
      [FromQuery] string CRS,
      [FromQuery] string STYLES,
      [FromQuery] int WIDTH,
      [FromQuery] int HEIGHT,
      [FromQuery] string BBOX,
      [FromQuery] Guid projectUid,
      [FromQuery] string fileType)
    {
      log.LogDebug("GetLineworkTile: " + Request.QueryString);

      ValidateWmsParameters(SERVICE, VERSION, REQUEST, FORMAT, TRANSPARENT, LAYERS, CRS, STYLES);
      ValidateTileDimensions(WIDTH, HEIGHT);

      var requiredFiles = await ValidateFileType(projectUid, fileType);
      DxfTileRequest request = DxfTileRequest.CreateTileRequest(requiredFiles, GetBoundingBox(BBOX));
      request.Validate();
      var executor = RequestExecutorContainerFactory.Build<DxfTileExecutor>(logger, raptorClient, null, this.ConfigStore, fileRepo);
      var result = await executor.ProcessAsync(request) as TileResult;
      return result;
    }

    /// <summary>
    /// This requests returns raw array of bytes with PNG without any diagnostic information. If it fails refer to the request with disgnostic info.
    /// Supplies tiles of linework for DXF, Alignment and Design surface files imported into a project.
    /// The tiles for the supplied list of files are overlaid and a single tile returned.
    /// </summary>
    /// <param name="SERVICE">WMS parameter - value WMS</param>
    /// <param name="VERSION">WMS parameter - value 1.3.0</param>
    /// <param name="REQUEST">WMS parameter - value GetMap</param>
    /// <param name="FORMAT">WMS parameter - value image/png</param>
    /// <param name="TRANSPARENT">WMS parameter - value true</param>
    /// <param name="LAYERS">WMS parameter - value Layers</param>
    /// <param name="CRS">WMS parameter - value EPSG:4326</param>
    /// <param name="STYLES">WMS parameter - value null</param>
    /// <param name="WIDTH">The width, in pixels, of the image tile to be rendered, usually 256</param>
    /// <param name="HEIGHT">The height, in pixels, of the image tile to be rendered, usually 256</param>
    /// <param name="BBOX">The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="fileType">The imported file type for which to to overlay tiles. Valid values are Linework, Alignment and DesignSurface</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
    /// <executor>TilesExecutor</executor> 
    [ProjectUidVerifier]
    [Route("api/v2/compaction/lineworktiles/png")]
    [HttpGet]
    public async Task<FileResult> GetLineworkTileRaw(
      [FromQuery] string SERVICE,
      [FromQuery] string VERSION,
      [FromQuery] string REQUEST,
      [FromQuery] string FORMAT,
      [FromQuery] string TRANSPARENT,
      [FromQuery] string LAYERS,
      [FromQuery] string CRS,
      [FromQuery] string STYLES,
      [FromQuery] int WIDTH,
      [FromQuery] int HEIGHT,
      [FromQuery] string BBOX,
      [FromQuery] Guid projectUid,
      [FromQuery] string fileType)
    {
      log.LogDebug("GetLineworkTileRaw: " + Request.QueryString);

      ValidateWmsParameters(SERVICE, VERSION, REQUEST, FORMAT, TRANSPARENT, LAYERS, CRS, STYLES);
      ValidateTileDimensions(WIDTH, HEIGHT);

      var requiredFiles = await ValidateFileType(projectUid, fileType);
      DxfTileRequest request = DxfTileRequest.CreateTileRequest(requiredFiles, GetBoundingBox(BBOX));
      request.Validate();
      var executor = RequestExecutorContainerFactory.Build<DxfTileExecutor>(logger, raptorClient, null, this.ConfigStore, fileRepo);
      var result = await executor.ProcessAsync(request) as TileResult;

      return new FileStreamResult(new MemoryStream(result.TileData), "image/png");
    }

    /// <summary>
    /// Validates the WMS parameters for the tile requests
    /// </summary>
    /// <param name="SERVICE"></param>
    /// <param name="VERSION"></param>
    /// <param name="REQUEST"></param>
    /// <param name="FORMAT"></param>
    /// <param name="TRANSPARENT"></param>
    /// <param name="LAYERS"></param>
    /// <param name="CRS"></param>
    /// <param name="STYLES"></param>
    private void ValidateWmsParameters(
      string SERVICE,
      string VERSION,
      string REQUEST,
      string FORMAT,
      string TRANSPARENT,
      string LAYERS,
      string CRS,
      string STYLES)
    {
      bool invalid = (!string.IsNullOrEmpty(SERVICE) && SERVICE.ToUpper() != "WMS") ||
                     (!string.IsNullOrEmpty(VERSION) && VERSION.ToUpper() != "1.3.0") ||
                     (!string.IsNullOrEmpty(REQUEST) && REQUEST.ToUpper() != "GETMAP") ||
                     (!string.IsNullOrEmpty(FORMAT) && FORMAT.ToUpper() != "IMAGE/PNG") ||
                     (!string.IsNullOrEmpty(TRANSPARENT) && TRANSPARENT.ToUpper() != "TRUE") ||
                     (!string.IsNullOrEmpty(LAYERS) && LAYERS.ToUpper() != "LAYERS") ||
                     (!string.IsNullOrEmpty(CRS) && CRS.ToUpper() != "EPSG:4326") ||
                     (!string.IsNullOrEmpty(STYLES));

      if (invalid)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Service supports only the following: SERVICE=WMS, VERSION=1.3.0, REQUEST=GetMap, FORMAT=image/png, TRANSPARENT=true, LAYERS=Layers, CRS=EPSG:4326, STYLES= (no styles supported)"));
      }
    }

    /// <summary>
    /// Validates the tile width and height
    /// </summary>
    /// <param name="WIDTH"></param>
    /// <param name="HEIGHT"></param>
    private void ValidateTileDimensions(int WIDTH, int HEIGHT)
    {
      if (WIDTH != WebMercatorProjection.TILE_SIZE || HEIGHT != WebMercatorProjection.TILE_SIZE)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Service supports only tile width and height of " + WebMercatorProjection.TILE_SIZE + " pixels"));
      }
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
      var fileList = await this.FileListProxy.GetFiles(projectUid.ToString(), Request.Headers.GetCustomHeaders());
      if (fileList == null)
      {
        fileList = new List<FileData>();
      }
      //Select the required ones from the list
      var filesOfType = fileList.Where(f => f.ImportedFileType == importedFileType && f.IsActivated).ToList();
      log.LogInformation("Found {0} files of type {1} from a total of {2}", filesOfType.Count, fileType, fileList.Count);
      return filesOfType;
    }

    /// <summary>
    /// Get the bounding box values from the query parameter
    /// </summary>
    /// <param name="bbox">The query parameter containing the bounding box in decimal degrees</param>
    /// <returns>Bounding box in radians</returns>
    private BoundingBox2DLatLon GetBoundingBox(string bbox)
    {
      double blLong = 0;
      double blLat = 0;
      double trLong = 0;
      double trLat = 0;

      int count = 0;
      foreach (string s in bbox.Split(','))
      {
        if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Invalid bounding box"));
        }
        num = num * Math.PI / 180.0; //convert decimal degrees to radians
        //Latitude Must be in range -pi/2 to pi/2 and longitude in the range -pi to pi
        if (count == 0 || count == 2)
        {
          if (num < -Math.PI / 2)
          {
            num = num + Math.PI;
          }
          else if (num > Math.PI / 2)
          {
            num = num - Math.PI;
          }
        }
        if (count == 1 || count == 3)
        {
          if (num < -Math.PI)
          {
            num = num + 2 * Math.PI;
          }
          else if (num > Math.PI)
          {
            num = num - 2 * Math.PI;
          }
        }

        switch (count++)
        {
          case 0:
            blLat = num;
            break;
          case 1:
            blLong = num;
            break;
          case 2:
            trLat = num;
            break;
          case 3:
            trLong = num;
            break;
        }
      }
      log.LogDebug("BBOX in radians: blLong=" + blLong + ",blLat=" + blLat + ",trLong=" + trLong + ",trLat=" + trLat);
      return BoundingBox2DLatLon.CreateBoundingBox2DLatLon(blLong, blLat, trLong, trLat);
    }

    /// <summary>
    /// Get the elevation extents for the palette for elevation tile requests
    /// </summary>
    /// <param name="projectSettings">Project settings to use for Raptor</param>
    /// <param name="filter">Filter to use for Raptor</param>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="mode">Display mode; type of data requested</param>
    /// <returns>Elevation extents to use</returns>
    private ElevationStatisticsResult GetElevationExtents(CompactionProjectSettings projectSettings, Common.Models.Filter filter, long projectId, DisplayMode mode)
    {
      var elevExtents = mode == DisplayMode.Height ? elevProxy.GetElevationRange(projectId, filter, projectSettings) : null;
      //Fix bug in Raptor - swap elevations if required
      elevExtents?.SwapElevationsIfRequired();
      return elevExtents;
    }

    /// <summary>
    /// Gets the requested tile from Raptor
    /// </summary>
    /// <param name="projectSettings">Project settings to use for Raptor</param>
    /// <param name="filter">Filter to use for Raptor</param>
    /// <param name="projectUid">Legacy project ID</param>
    /// <param name="mode">Display mode; type of data requested</param>
    /// <param name="width">Width of the tile</param>
    /// <param name="height">Height of the tile in pixels</param>
    /// <param name="bbox">Bounding box in radians</param>
    /// <param name="cutFillDesign">Design descriptor for cut-fill design</param>
    /// <param name="sumVolParameters">Filter(s) and/or design for summary volumes</param>
    /// <param name="volumeCalcType">Volume calculation type</param>
    /// <param name="volumeBaseUid">The base surface Uid</param>
    /// <param name="volumeTopUid">The top surfance Uid</param>
    /// <returns>Tile result</returns>
    private TileResult GetProductionDataTile(CompactionProjectSettings projectSettings, Common.Models.Filter filter, Guid projectUid, DisplayMode mode, ushort width, ushort height, BoundingBox2DLatLon bbox, DesignDescriptor cutFillDesign, Tuple<Common.Models.Filter, Common.Models.Filter, DesignDescriptor> sumVolParameters, VolumeCalcType? volumeCalcType, Guid? volumeBaseUid, Guid? volumeTopUid)
    {
      var projectId = (User as RaptorPrincipal).GetProjectId(projectUid);

      var tileRequest = requestFactory.Create<TileRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(this.CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter)
          .DesignDescriptor(cutFillDesign))
          .SetBaseFilter(sumVolParameters.Item1)
          .SetTopFilter(sumVolParameters.Item2)
          .SetVolumeCalcType(volumeCalcType)
          .SetVolumeDesign(sumVolParameters.Item3)
          .CreateTileRequest(mode, width, height, bbox,
            GetElevationExtents(projectSettings, filter, projectId, mode));

      //TileRequest is both v1 and v2 model so cannot change its validation directly.
      //However for v2 we want to return a transparent empty tile for cut-fill if no design specified.
      //So catch the validation exception for this case.
      bool getTile = true;
      try
      {
        tileRequest.Validate();
      }
      catch (ServiceException se)
      {
        if (tileRequest.mode == DisplayMode.CutFill &&
            se.Code == HttpStatusCode.BadRequest &&
            se.GetResult.Code == ContractExecutionStatesEnum.ValidationError)
        {
          if (se.GetResult.Message ==
              "Design descriptor required for cut/fill and design to filter or filter to design volumes display" ||
              se.GetResult.Message ==
              "Two filters required for filter to filter volumes display" ||
              se.GetResult.Message ==
              "One filter required for design to filter or filter to design volumes display")
          {
            getTile = false;
          }
        }
        //Rethrow any other exception
        if (getTile)
        {
          throw se;
        }
      }

      TileResult tileResult = null;
      if (getTile)
      {
        tileResult = WithServiceExceptionTryExecute(() =>
          RequestExecutorContainerFactory
            .Build<CompactionTilesExecutor>(logger, raptorClient)
            .Process(tileRequest) as TileResult
        );
      }

      return tileResult ?? TileResult.EmptyTile(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE);
    }
  }
}