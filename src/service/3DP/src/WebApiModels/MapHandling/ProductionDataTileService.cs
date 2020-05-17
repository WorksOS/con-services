using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Exceptions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models.Compaction;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Interfaces;
using VSS.Productivity3D.WebApi.Models.ProductionData;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Serilog.Extensions;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  /// <summary>
  /// Provides production data tile functionality for reports
  /// </summary>
  public class ProductionDataTileService : IProductionDataTileService
  {
    private readonly IProductionDataRequestFactory requestFactory;
    private readonly IElevationExtentsProxy elevProxy;
#if RAPTOR
    private readonly IASNodeClient raptorClient;
#endif
    private readonly IConfigurationStore ConfigStore;
    protected readonly ITRexCompactionDataProxy TRexCompactionDataProxy;
    private readonly ILogger log;
    private readonly ILoggerFactory logger;

    public ProductionDataTileService(IProductionDataRequestFactory prodDataFactory, ILoggerFactory logger, IElevationExtentsProxy extentsProxy,
#if RAPTOR
      IASNodeClient raptor, 
#endif
      IConfigurationStore configStore, ITRexCompactionDataProxy trexCompactionDataProxy)
    {
      requestFactory = prodDataFactory;
      log = logger.CreateLogger<ProductionDataTileService>();
      this.logger = logger;
#if RAPTOR
      raptorClient = raptor;
#endif
      elevProxy = extentsProxy;
      ConfigStore = configStore;
      TRexCompactionDataProxy = trexCompactionDataProxy;
    }

    /// <summary>
    /// Gets the requested tile from Raptor
    /// </summary>
    /// <param name="projectSettings">Project settings to use for Raptor</param>
    /// <param name="projectSettingsColors"></param>
    /// <param name="filter">Filter to use for Raptor</param>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Unique project identifier</param>
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
    public async Task<TileResult> GetProductionDataTile(CompactionProjectSettings projectSettings, CompactionProjectSettingsColors projectSettingsColors, FilterResult filter, long projectId, Guid projectUid, DisplayMode mode, ushort width, ushort height, BoundingBox2DLatLon bbox, DesignDescriptor cutFillDesign, FilterResult baseFilter, FilterResult topFilter, DesignDescriptor volumeDesign, VolumeCalcType? volumeCalcType, IHeaderDictionary customHeaders, bool explicitFilters = false)
    {
      var getTile = true;
      ElevationStatisticsResult elevationExtents = null;
      TileRequest tileRequest = null;

      try
      {
        elevationExtents = await GetElevationExtents(projectSettings, filter, projectId, projectUid, mode, customHeaders);
      }
      catch (ServiceException se)
      {
        getTile = mode != DisplayMode.Height;
        if (log.IsTraceEnabled())
          log.LogTrace(
            $"Failed to get elevation extents for height request with error: {se.GetResult.Code}:{se.GetResult.Message} a transparent tile will be generated");
      }

      if (getTile)
      {
        tileRequest = requestFactory.Create<TileRequestHelper>(r => r
           .ProjectUid(projectUid)
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
           .CreateTileRequest(mode, width, height, bbox, elevationExtents, explicitFilters);

        //TileRequest is both v1 and v2 model so cannot change its validation directly.
        //However for v2 we want to return a transparent empty tile for cut-fill if no design specified.
        //So catch the validation exception for this case.
        try
        {
          tileRequest.Validate();
        }
        catch (ServiceException se)
        {
          if (tileRequest.Mode == DisplayMode.CutFill &&
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
          tileResult = await RequestExecutorContainerFactory
            .Build<CompactionTileExecutor>(logger,
#if RAPTOR
              raptorClient, 
#endif
              configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: customHeaders)
            .ProcessAsync(tileRequest) as TileResult;
        }
        catch (Exception ex)
        {
          log.LogWarning($"Exception: {ex.Message} {ex.StackTrace}");
        }
      }

      return tileResult?.TileData != null
        ? tileResult
        : TileResult.EmptyTile(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE);
    }

    /// <summary>
    /// Get the elevation extents for the palette for elevation tile requests
    /// </summary>
    /// <param name="projectSettings">Project settings to use for Raptor</param>
    /// <param name="filter">Filter to use for Raptor</param>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="mode">Display mode; type of data requested</param>
    /// <returns>Elevation extents to use</returns>
    private async Task<ElevationStatisticsResult> GetElevationExtents(CompactionProjectSettings projectSettings, FilterResult filter, long projectId, Guid projectUid, DisplayMode mode, IHeaderDictionary customHeaders)
    {
      var elevExtents = (mode == DisplayMode.Height || mode == DisplayMode.Design3D)
        ? await elevProxy.GetElevationRange(projectId, projectUid, filter, projectSettings, customHeaders)
        : null;
      //Fix bug in Raptor - swap elevations if required
      elevExtents?.SwapElevationsIfRequired();
      return elevExtents;
    }
  }
}
