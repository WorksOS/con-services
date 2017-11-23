using System.Collections.Generic;
using System.Drawing;
using System.Net;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Executors;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Helpers;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  /// <summary>
  /// Provides production data tile functionality for reports
  /// </summary>
  public class ProductionDataTileService : IProductionDataTileService
  {
    private readonly IProductionDataRequestFactory requestFactory;
    private readonly IElevationExtentsProxy elevProxy;
    private readonly IASNodeClient raptorClient;
    private readonly ILogger log;
    private readonly ILoggerFactory logger;

    public ProductionDataTileService(IProductionDataRequestFactory prodDataFactory, ILoggerFactory logger, IElevationExtentsProxy extentsProxy, IASNodeClient raptor)
    {
      requestFactory = prodDataFactory;
      log = logger.CreateLogger<ProductionDataTileService>();
      this.logger = logger;
      raptorClient = raptor;
      elevProxy = extentsProxy;
    }

    /// <summary>
    /// Gets the requested tile from Raptor
    /// </summary>
    /// <param name="projectSettings">Project settings to use for Raptor</param>
    /// <param name="filter">Filter to use for Raptor</param>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="mode">Display mode; type of data requested</param>
    /// <param name="width">Width of the tile</param>
    /// <param name="height">Height of the tile in pixels</param>
    /// <param name="bbox">Bounding box in radians</param>
    /// <param name="cutFillDesign">Design descriptor for cut-fill</param>
    /// <param name="baseFilter">Base filter for  summary volumes</param>
    /// <param name="topFilter">Top filter for  summary volumes</param>
    /// <param name="volumeDesign">Design descriptor for summary volumes design</param>
    /// <returns>Tile result</returns>
    public TileResult GetProductionDataTile(CompactionProjectSettings projectSettings,
      Filter filter, long projectId, DisplayMode mode, ushort width, ushort height,
      BoundingBox2DLatLon bbox, DesignDescriptor cutFillDesign, Filter baseFilter,
      Filter topFilter, DesignDescriptor volumeDesign, IDictionary<string, string> customHeaders)
    {
      var tileRequest = requestFactory.Create<TileRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(customHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter)
          .DesignDescriptor(cutFillDesign))
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
        if (tileRequest.mode == DisplayMode.CutFill && tileRequest.designDescriptor == null)
        {
          if (se.Code == HttpStatusCode.BadRequest &&
              se.GetResult.Code == ContractExecutionStatesEnum.ValidationError &&
              se.GetResult.Message ==
              "Design descriptor required for cut/fill and design to filter or filter to design volumes display")
          {
            getTile = false;
          }
        }
        //Rethrow any other exception
        if (getTile)
          throw se;
      }

      TileResult tileResult = null;
      if (getTile)
      {
        tileResult = RequestExecutorContainerFactory
          .Build<CompactionTilesExecutor>(logger, raptorClient)
          .Process(tileRequest) as TileResult;
      }

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



    /// <summary>
    /// Get the elevation extents for the palette for elevation tile requests
    /// </summary>
    /// <param name="projectSettings">Project settings to use for Raptor</param>
    /// <param name="filter">Filter to use for Raptor</param>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="mode">Display mode; type of data requested</param>
    /// <returns>Elevation extents to use</returns>
    private ElevationStatisticsResult GetElevationExtents(CompactionProjectSettings projectSettings, Filter filter, long projectId, DisplayMode mode)
    {
      var elevExtents = mode == DisplayMode.Height ? elevProxy.GetElevationRange(projectId, filter, projectSettings) : null;
      //Fix bug in Raptor - swap elevations if required
      elevExtents?.SwapElevationsIfRequired();
      return elevExtents;
    }
  }

  public interface IProductionDataTileService
  {
    TileResult GetProductionDataTile(CompactionProjectSettings projectSettings,
      Filter filter, long projectId, DisplayMode mode, ushort width, ushort height,
      BoundingBox2DLatLon bbox, DesignDescriptor cutFillDesign, Filter baseFilter, 
      Filter topFilter, DesignDescriptor volumeDesign, IDictionary<string, string> customHeaders);
  }
}
