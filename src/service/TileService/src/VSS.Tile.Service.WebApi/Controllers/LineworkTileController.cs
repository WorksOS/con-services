using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Http;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Tile.Service.Common.Executors;
using VSS.Tile.Service.Common.Filters;
using VSS.Tile.Service.Common.Interfaces;
using VSS.Tile.Service.Common.Models;
using VSS.Tile.Service.Common.Services;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;

namespace VSS.Tile.Service.WebApi.Controllers
{
  public class LineworkTileController : BaseController<LineworkTileController>
  {
    private readonly IDataOceanClient dataOceanClient;
    /// <summary>
    /// Default constructor.
    /// </summary>
    public LineworkTileController(IRaptorProxy raptorProxy, IPreferenceProxy prefProxy, IFileListProxy fileListProxy,
      IMapTileGenerator tileGenerator, IGeofenceProxy geofenceProxy, IMemoryCache cache, IConfigurationStore configStore,
      IBoundingBoxHelper boundingBoxHelper, IDataOceanClient dataOceanClient, ITPaaSApplicationAuthentication authn)
      : base(raptorProxy, prefProxy, fileListProxy, tileGenerator, geofenceProxy, cache, configStore, boundingBoxHelper, authn)
    {
      this.dataOceanClient = dataOceanClient;
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
    [Route("api/v1/lineworktiles")]
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
      Log.LogDebug("GetLineworkTile: " + Request.QueryString);

      var requiredFiles = await ValidateFileType(projectUid, fileType);
      var dxfTileRequest = DxfTileRequest.CreateTileRequest(requiredFiles, boundingBoxHelper.GetBoundingBox(bbox));

      var executor = RequestExecutorContainerFactory.Build<DxfTileExecutor, LineworkTileController>(
        Log, configStore, CustomHeaders, dataOceanClient, authn);
      var result = await executor.ProcessAsync(dxfTileRequest) as TileResult;

      return result;
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
    [Route("api/v1/lineworktiles/png")]
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
      Log.LogDebug("GetLineworkTileRaw: " + Request.QueryString);

      var requiredFiles = await ValidateFileType(projectUid, fileType);
      var dxfTileRequest = DxfTileRequest.CreateTileRequest(requiredFiles, boundingBoxHelper.GetBoundingBox(bbox));

      var executor = RequestExecutorContainerFactory.Build<DxfTileExecutor, LineworkTileController>(
        Log, configStore, CustomHeaders, dataOceanClient, authn);
      var result = await executor.ProcessAsync(dxfTileRequest) as TileResult;

      return new FileStreamResult(new MemoryStream(result.TileData), ContentTypeConstants.ImagePng);
    }

    /// <summary>
    /// This requests returns raw array of bytes with PNG without any diagnostic information. If it fails refer to the request with disgnostic info.
    /// Supplies tiles of linework for DXF, Alignment and Design surface files imported into a project.
    /// The tiles for the supplied list of files are overlaid and a single tile returned.
    /// </summary>
    /// <param name="z">Zoom level</param>
    /// <param name="y">y tile coordinate</param>
    /// <param name="x">x tile coordinate</param>
    /// <param name="width">The width of the tile in pixels</param>
    /// <param name="height">The height of the tile in pixels</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="fileType">The imported file type for which to to overlay tiles. Valid values are Linework, Alignment and DesignSurface</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
    /// <executor>DxfTileExecutor</executor>
    [ValidateWidthAndHeight]
    [Route("api/v1/lineworktiles3d/{z}/{y}/{x}.png")]
    [HttpGet]
    public async Task<FileResult> GetLineworkTile3dRaw(
      [FromRoute] int z,
      [FromRoute] int y,
      [FromRoute] int x,
      [FromQuery] int width,
      [FromQuery] int height,
      [FromQuery] Guid projectUid,
      [FromQuery] string fileType)
    {
      Log.LogDebug($"{nameof(GetLineworkTile3dRaw)}: { Request.QueryString}");

      var requiredFiles = await ValidateFileType(projectUid, fileType);
      var dxfTile3dRequest = DxfTile3dRequest.Create(requiredFiles, z, y, x);

      var executor = RequestExecutorContainerFactory.Build<DxfTileExecutor, LineworkTileController>(
        Log, configStore, CustomHeaders, dataOceanClient, authn);
      var result = await executor.ProcessAsync(dxfTile3dRequest) as TileResult;

      return new FileStreamResult(new MemoryStream(result.TileData), ContentTypeConstants.ImagePng);
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
        if (importedFileType != ImportedFileType.Linework && importedFileType != ImportedFileType.Alignment)
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
      var fileList = await fileListProxy.GetFiles(projectUid.ToString(), GetUserId(), Request.Headers.GetCustomHeaders()) ?? new List<FileData>();

      //Select the required ones from the list
      var filesOfType = fileList.Where(f => f.ImportedFileType == importedFileType && f.IsActivated).ToList();
      Log.LogInformation("Found {0} files of type {1} from a total of {2}", filesOfType.Count, fileType, fileList.Count);

      return filesOfType;
    }
  }
}
