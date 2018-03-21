using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Exceptions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;

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
    /// <param name="volumeCalcType">Type of summary volumes calculation</param>
    /// <param name="customHeaders">Custom request headers</param>
    /// <returns>Tile result</returns>
    public TileResult GetProductionDataTile(CompactionProjectSettings projectSettings, CompactionProjectSettingsColors projectSettingsColors, Filter filter, long projectId, 
      DisplayMode mode, ushort width, ushort height, BoundingBox2DLatLon bbox, DesignDescriptor cutFillDesign, Filter baseFilter,
      Filter topFilter, DesignDescriptor volumeDesign, VolumeCalcType? volumeCalcType, IDictionary<string, string> customHeaders)
    {
      bool getTile = true;
      ElevationStatisticsResult elevationExtents = null;
      TileRequest tileRequest = null;

      try
      {
         elevationExtents = GetElevationExtents(projectSettings, filter, projectId, mode);
      }
      catch (ServiceException se)
      {
        getTile = mode != DisplayMode.Height;
        log.LogTrace(
            $"Failed to get elevation extents for height request with error: {se.GetResult.Code}:{se.GetResult.Message} a transparent tile will be generated");
      }
      
      if (getTile)
      {
         tileRequest = requestFactory.Create<TileRequestHelper>(r => r
            .ProjectId(projectId)
            .Headers(customHeaders)
            .ProjectSettings(projectSettings)
            .ProjectSettingsColors(projectSettingsColors)
            .Filter(filter)
            .DesignDescriptor(cutFillDesign))
            .SetVolumeCalcType(volumeCalcType)
            .SetVolumeDesign(volumeDesign)
            .SetBaseFilter(baseFilter)
            .SetTopFilter(topFilter)
            .CreateTileRequest(mode, width, height, bbox, elevationExtents);

        //TileRequest is both v1 and v2 model so cannot change its validation directly.
        //However for v2 we want to return a transparent empty tile for cut-fill if no design specified.
        //So catch the validation exception for this case.
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
            if (
              se is MissingDesignDescriptorException || 
              se is TwoFiltersRequiredException || 
              se is SingleFilterRequiredException)
            {
              getTile = false;
            }
          }
          //Rethrow any other exception
          if (getTile)
          {
            throw;
          }
        }
      }

      TileResult tileResult = null;
      if (getTile)
      {
        try
        {
          tileResult = RequestExecutorContainerFactory
            .Build<CompactionTileExecutor>(logger, raptorClient)
            .Process(tileRequest) as TileResult;
        }
        catch (Exception ex)
        {
          log.LogWarning($"Exception: {ex.Message} {ex.StackTrace}");
        }
      }

      return tileResult ?? TileResult.EmptyTile(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE);
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
    TileResult GetProductionDataTile(CompactionProjectSettings projectSettings, CompactionProjectSettingsColors projectSettingsColors, Filter filter, long projectId, 
      DisplayMode mode, ushort width, ushort height, BoundingBox2DLatLon bbox, DesignDescriptor cutFillDesign, 
      Filter baseFilter, Filter topFilter, DesignDescriptor volumeDesign, VolumeCalcType? volumeCalcType, IDictionary<string, string> customHeaders);
  }
}
