using ASNodeDecls;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterDataProxies;
using VSS.MasterDataProxies.Interfaces;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Controllers;
using VSS.Productivity3D.Common.Executors;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Compaction.Executors;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Productivity3D.WebApiModels.Compaction.Models;
using VSS.Productivity3D.WebApiModels.Notification.Helpers;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;


namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting tiles for displaying production data and linework.
  /// </summary>
  public class CompactionTileController : Controller
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
    /// For getting list of imported files for a project
    /// </summary>
    private readonly IFileListProxy fileListProxy;
    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    private IConfigurationStore configStore;
    /// <summary>
    /// Used to talk to TCC
    /// </summary>
    private readonly IFileRepository fileRepo;

    /// <summary>
    /// Proxy for getting elevation statistics from Raptor
    /// </summary>
    private readonly IElevationExtentsProxy elevProxy;

    /// <summary>
    /// For getting project settings for a project
    /// </summary>
    private readonly IProjectSettingsProxy projectSettingsProxy;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="fileListProxy">File list proxy</param>
    /// <param name="configStore">Configuration store</param>
    /// <param name="fileRepo">Imported file repository</param>
    /// <param name="elevProxy">Elevation extents proxy</param>
    /// <param name="projectSettingsProxy">Project settings proxy</param>
    public CompactionTileController(IASNodeClient raptorClient, ILoggerFactory logger,
      IFileListProxy fileListProxy, IConfigurationStore configStore, IFileRepository fileRepo, 
      IElevationExtentsProxy elevProxy, IProjectSettingsProxy projectSettingsProxy)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<CompactionTileController>();
      this.fileListProxy = fileListProxy;
      this.configStore = configStore;
      this.fileRepo = fileRepo;
      this.elevProxy = elevProxy;
      this.projectSettingsProxy = projectSettingsProxy;
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
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="mode">The thematic mode to be rendered; elevation, compaction, temperature etc</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
    /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
    /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
    /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
    /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
    ///  to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
    /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction.</param>
    /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
    /// All three parameters must be specified to specify a machine. 
    /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
    /// <param name="machineName">See assetID</param>
    /// <param name="isJohnDoe">See assetIDL</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
    /// <executor>TilesExecutor</executor> 
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/productiondatatiles")]
    [HttpGet]
    [ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
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
      [FromQuery] long? projectId,
      [FromQuery] Guid? projectUid,
      [FromQuery] DisplayMode mode,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] bool? vibeStateOn,
      [FromQuery] ElevationType? elevationType,
      [FromQuery] int? layerNumber,
      [FromQuery] long? onMachineDesignId,
      [FromQuery] long? assetID,
      [FromQuery] string machineName,
      [FromQuery] bool? isJohnDoe)
    {
      log.LogDebug("GetProductionDataTile: " + Request.QueryString);
      ValidateWmsParameters(SERVICE, VERSION, REQUEST, FORMAT, TRANSPARENT, LAYERS, CRS, STYLES);

      if (!projectId.HasValue)
      {
        projectId = (User as RaptorPrincipal).GetProjectId(projectUid);
      }
      var headers = Request.Headers.GetCustomHeaders();
      var projectSettings = await this.GetProjectSettings(projectSettingsProxy, projectUid.Value, headers, log);
      var excludedIds = await this.GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid.Value, headers);
      Filter filter = CompactionSettings.CompactionFilter(
        startUtc, endUtc, onMachineDesignId, vibeStateOn, elevationType, layerNumber,
        this.GetMachines(assetID, machineName, isJohnDoe), excludedIds);
      var tileResult = GetProductionDataTile(projectSettings, filter, projectId.Value, mode, (ushort)WIDTH, (ushort)HEIGHT, GetBoundingBox(BBOX));
      if (mode==DisplayMode.Height)
        Response.GetTypedHeaders().CacheControl=new CacheControlHeaderValue(){NoCache = true, NoStore = true};
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
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="mode">The thematic mode to be rendered; elevation, compaction, temperature etc</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
    /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
    /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
    /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
    /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
    ///  to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
    /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction.</param>
    /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
    /// All three parameters must be specified to specify a machine. 
    /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
    /// <param name="machineName">See assetID</param>
    /// <param name="isJohnDoe">See assetIDL</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request succeeds. 
    /// If the size of a pixel in the rendered tile coveres more than 10.88 meters in width or height, then the pixel will be rendered 
    /// in a 'representational style' where black (currently, but there is a work item to allow this to be configurable) is used to 
    /// indicate the presense of data. Representational style rendering performs no filtering what so ever on the data.10.88 meters is 32 
    /// (number of cells across a subgrid) * 0.34 (default width in meters of a single cell).
    /// </returns>
    /// <executor>TilesExecutor</executor> 
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/productiondatatiles/png")]
    [HttpGet]
    [ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
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
      [FromQuery] long? projectId,
      [FromQuery] Guid? projectUid,
      [FromQuery] DisplayMode mode,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] bool? vibeStateOn,
      [FromQuery] ElevationType? elevationType,
      [FromQuery] int? layerNumber,
      [FromQuery] long? onMachineDesignId,
      [FromQuery] long? assetID,
      [FromQuery] string machineName,
      [FromQuery] bool? isJohnDoe)
    {
      log.LogDebug("GetProductionDataTileRaw: " + Request.QueryString);

      ValidateWmsParameters(SERVICE, VERSION, REQUEST, FORMAT, TRANSPARENT, LAYERS, CRS, STYLES);
      if (!projectId.HasValue)
      {
        projectId = (User as RaptorPrincipal).GetProjectId(projectUid);
      }
      var headers = Request.Headers.GetCustomHeaders();
      var projectSettings = await this.GetProjectSettings(projectSettingsProxy, projectUid.Value, headers, log);
      var excludedIds = await this.GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid.Value, headers);
      Filter filter = CompactionSettings.CompactionFilter(
        startUtc, endUtc, onMachineDesignId, vibeStateOn, elevationType, layerNumber,
        this.GetMachines(assetID, machineName, isJohnDoe), excludedIds);
      var tileResult = GetProductionDataTile(projectSettings, filter, projectId.Value, mode, (ushort)WIDTH, (ushort)HEIGHT, GetBoundingBox(BBOX));
      Response.Headers.Add("X-Warning", tileResult.TileOutsideProjectExtents.ToString());
      if (mode == DisplayMode.Height)
        Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue() { NoCache = true, NoStore = true };
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
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
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
      var executor = RequestExecutorContainer.Build<DxfTileExecutor>(logger, raptorClient, null, configStore, fileRepo);
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
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
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
      var executor = RequestExecutorContainer.Build<DxfTileExecutor>(logger, raptorClient, null, configStore, fileRepo);
      var result = await executor.ProcessAsync(request) as TileResult;
      //AddCacheResponseHeaders();  //done by middleware               
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
      ImportedFileType importedFileType;
      if (Enum.TryParse(fileType, true, out importedFileType))
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
      var fileList = await fileListProxy.GetFiles(projectUid.ToString(), Request.Headers.GetCustomHeaders());
      if (fileList == null)
      {
        fileList = new List<FileData>();
      }
      //Select the required ones from the list
      var filesOfType = fileList.Where(f => f.ImportedFileType == importedFileType && f.IsActivated).ToList();
      log.LogInformation("Found {0} files of type {1} from a total of {2}", filesOfType.Count, fileType, fileList.Count);
      return filesOfType;
    }

    /*
      /// <summary>
      /// Adds caching headers to the http response
      /// </summary>
      private void AddCacheResponseHeaders()
      {
        if (!Response.Headers.ContainsKey("Cache-Control"))
        {
          Response.Headers.Add("Cache-Control", "public");
        }
        Response.Headers.Add("Expires",
          DateTime.Now.AddMinutes(15).ToUniversalTime().ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"));
      }
      */

   

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
        double num;

        if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out num))
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
    /// Gets the requested tile from Raptor
    /// </summary>
    /// <param name="projectSettings"></param>
    /// <param name="filter"></param>
    /// <param name="projectId"></param>
    /// <param name="mode"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="bbox"></param>
    /// <returns>Tile result</returns>
    private TileResult GetProductionDataTile(CompactionProjectSettings projectSettings, Filter filter, long projectId, DisplayMode mode, ushort width, ushort height,
      BoundingBox2DLatLon bbox)
    {
      LiftBuildSettings liftSettings = CompactionSettings.CompactionLiftBuildSettings(projectSettings);
      filter?.Validate();
      ElevationStatisticsResult elevExtents =
        mode == DisplayMode.Height ? elevProxy.GetElevationRange(projectId, filter, projectSettings) : null;
      //Fix bug in Raptor - swap elevations if required
      elevExtents?.SwapElevationsIfRequired();
      var palette = CompactionSettings.CompactionPalette(mode, elevExtents);
      if (mode == DisplayMode.Height)
      {
        log.LogDebug("GetProductionDataTile: surveyedSurfaceExclusionList count={0}, elevExtents={1}-{2}, palette count={4}",
          (filter == null || filter.surveyedSurfaceExclusionList == null)
            ? 0
            : filter.surveyedSurfaceExclusionList.Count,
          elevExtents == null ? 0 : elevExtents.MinElevation, elevExtents == null ? 0 : elevExtents.MaxElevation,
          palette == null ? 0 : palette.Count);
      }
      TileRequest tileRequest = TileRequest.CreateTileRequest(projectId, null, mode,
        palette,
        liftSettings, RaptorConverters.VolumesType.None, 0, null, filter, 0, null, 0,
        filter == null ? FilterLayerMethod.None : filter.layerType.Value,
        bbox, null, width, height, 0, CMV_DETAILS_NUMBER_OF_COLORS, false);
      tileRequest.Validate();
      var tileResult = RequestExecutorContainer.Build<TilesExecutor>(logger, raptorClient, null)
        .Process(tileRequest) as TileResult;
      if (tileResult == null)
      {
        //Return en empty tile
        using (Bitmap bitmap = new Bitmap(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE))
        {
          tileResult = TileResult.CreateTileResult(bitmap.BitmapToByteArray(), TASNodeErrorStatus.asneOK);
        }
      }
      return tileResult;
    }

    private const int CMV_DETAILS_NUMBER_OF_COLORS = 16;
  }
}
