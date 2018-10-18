using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Compaction.ActionServices;
using VSS.Productivity3D.WebApi.Compaction.Controllers.Filters;
using VSS.Productivity3D.WebApi.Models.Interfaces;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// A controller for getting 3d map tiles
  /// </summary>
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  [ProjectVerifier]
  public class Compaction3DMapController : BaseController<Compaction3DMapController>
  {
    /// <summary>
    /// Map Display Type for the 3d Maps control
    /// </summary>
    public enum MapDisplayType
    {
      /// <summary>
      /// Height Map image
      /// </summary>
      HeightMap = 0,
      /// <summary>
      /// Height Map representing the design
      /// </summary>
      DesignMap = 1,
      /// <summary>
      /// The texture to be displayed
      /// </summary>
      Texture = 2
    }

    private readonly IProductionDataTileService tileService;
    private readonly IBoundingBoxHelper boundingBoxHelper;

    /// <summary>
    /// Default Constructor
    /// </summary>
    public Compaction3DMapController(ILoggerFactory loggerFactory,
      IServiceExceptionHandler serviceExceptionHandler,
      IConfigurationStore configStore,
      IFileListProxy fileListProxy,
      IProjectSettingsProxy projectSettingsProxy,
      IFilterServiceProxy filterServiceProxy,
      ICompactionSettingsManager settingsManager,
      IProductionDataTileService tileService,
      IASNodeClient raptorClient,
      IBoundingBoxHelper boundingBoxHelper) : base(configStore, fileListProxy, settingsManager)
    {
      this.tileService = tileService;
      this.boundingBoxHelper = boundingBoxHelper;
    }

    /// <summary>
    /// Generates a image for use in the 3d map control
    /// These can be heightmaps / textures / designs
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Optional Filter UID</param>
    /// <param name="designUid">The Design File UID if showing the Design (ignored otherwise)</param>
    /// <param name="cutfillDesignUid">Cut fill design UID for the Texture, ignored for other modes</param>
    /// <param name="type">Map Display Type - Heightmap / Texture / Design</param>
    /// <param name="mode">The thematic mode to be rendered; elevation, compaction, temperature etc (Ignored in Height Map type)</param>
    /// <param name="width">The width, in pixels, of the image tile to be rendered</param>
    /// <param name="height">The height, in pixels, of the image tile to be rendered</param>
    /// <param name="bbox">The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</param>
    /// <returns>An image representing the data requested</returns>
    [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
    [ValidateTileParameters]
    [Route("api/v2/map3d")]
    [HttpGet]
    public async Task<TileResult> GetMapTileData(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? designUid,
      [FromQuery] Guid? cutfillDesignUid,
      [FromQuery] MapDisplayType type,
      [FromQuery] DisplayMode mode,
      [FromQuery] ushort width,
      [FromQuery] ushort height,
      [FromQuery] string bbox)
    {
      var projectId = await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);

      CompactionProjectSettingsColors projectSettingsColors;
      DesignDescriptor design = null;
      DesignDescriptor cutFillDesign = null;
      if (type == MapDisplayType.DesignMap)
      {
        projectSettingsColors = GetGreyScaleHeightColors();
        design = await GetAndValidateDesignDescriptor(projectUid, designUid);
        mode = DisplayMode.Design3D;
      }
      else if (type == MapDisplayType.HeightMap)
      {
        projectSettingsColors = GetGreyScaleHeightColors();
        mode = DisplayMode.Height; // The height map must be of type height....
      }
      else if(type == MapDisplayType.Texture)
      {
        // Only used in texture mode
        cutFillDesign = cutfillDesignUid.HasValue
          ? await GetAndValidateDesignDescriptor(projectUid, cutfillDesignUid)
          : null;

        projectSettingsColors = await GetProjectSettingsColors(projectUid);
      }
      else
      {
        throw new NotImplementedException();
      }

      var filter = await GetCompactionFilter(projectUid, filterUid);

      var tileResult = WithServiceExceptionTryExecute(() =>
        tileService.GetProductionDataTile(projectSettings,
          projectSettingsColors,
          filter,
          projectId,
          projectUid,
          mode,
          width,
          height,
          boundingBoxHelper.GetBoundingBox(bbox),
          design ?? cutFillDesign, // If we have a design, it means we are asking for the design height map - otherwise we may have a cut fill design to determine the texture
          null,
          null,
          null,
          null,
          CustomHeaders));
      Response.Headers.Add("X-Warning", tileResult.TileOutsideProjectExtents.ToString());

      return tileResult;
    }

    /// <summary>
    /// Generates a raw image for use in the 3d map control
    /// These can be heightmaps / textures / designs
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Optional Filter UID</param>
    /// <param name="type">Map Display Type - Heightmap / Texture / Design</param>
    /// <param name="designUid">The Design File UID if showing the Design (ignored otherwise)</param>
    /// <param name="cutfillDesignUid">Cut fill design UID for the Texture, ignored for other modes</param>
    /// <param name="mode">The thematic mode to be rendered; elevation, compaction, temperature etc (Ignored in Height Map type)</param>
    /// <param name="width">The width, in pixels, of the image tile to be rendered</param>
    /// <param name="height">The height, in pixels, of the image tile to be rendered</param>
    /// <param name="bbox">The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</param>
    /// <returns>An image representing the data requested</returns>
    [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
    [ValidateTileParameters]
    [Route("api/v2/map3d/png")]
    [HttpGet]
    public async Task<FileResult> GetMapTileDataRaw(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? designUid,
      [FromQuery] Guid? cutfillDesignUid,
      [FromQuery] MapDisplayType type,
      [FromQuery] DisplayMode mode,
      [FromQuery] ushort width,
      [FromQuery] ushort height,
      [FromQuery] string bbox)
    {
      var result = await GetMapTileData(projectUid, filterUid, designUid, cutfillDesignUid, type, mode, width, height, bbox);
      return new FileStreamResult(new MemoryStream(result.TileData), "image/png");
    }

    private CompactionProjectSettingsColors GetGreyScaleHeightColors()
    {
      var colors = new List<uint>();
      for (var i = 0; i <= 255; i++)
      {
        colors.Add((uint)i << 16 | (uint)i << 8 | (uint)i << 0);
      }

      return CompactionProjectSettingsColors.Create(false, colors);
    }
  }
}
