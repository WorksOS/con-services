using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Cache.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.Productivity.Push.Models.Attributes;
using VSS.Productivity.Push.Models.Enums;
using VSS.Productivity.Push.Models.Notifications.Changes;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Tile.Service.Common.Models;

namespace VSS.Tile.Service.Common.Services
{
  /// <summary>
  /// Provides base map tile functionality for reports. Provider is ALK maps.
  /// </summary>
  public class ProductionDataTileService : IProductionDataTileService
  {
    private readonly IConfigurationStore _config;
    private readonly ILogger _log;
    private readonly IProductivity3dV2ProxyCompactionTile _productivity3DProxyCompactionTile;

    private readonly IDataCache _productivityDataCache;
    private readonly DateTimeOffset _expiration;

    public ProductionDataTileService(IConfigurationStore configuration, IDataCache dataCache, ILoggerFactory logger,
      IProductivity3dV2ProxyCompactionTile productivity3DProxyCompactionTile)
    {
      _config = configuration;
      _productivityDataCache = dataCache;
      _log = logger.CreateLogger<ProductionDataTileService>();
      _productivity3DProxyCompactionTile = productivity3DProxyCompactionTile;

      _expiration = DateTimeOffset.UtcNow.AddMinutes(_config.GetValueInt("PROJECT_TILE_CACHE_MINUTES", 60 * 24));
    }

    /// <summary>
    /// Get the base map tile according to the map type.
    /// </summary>
    public async Task<byte[]> GetMapBitmap(Guid projectUid, Guid? filterUid, Guid? cutFillDesignUid,
      MapParameters mapParameters, DisplayMode mode, Guid? baseUid, Guid? topUid, VolumeCalcType? volCalcType,
      IDictionary<string, string> customHeaders = null, bool explicitFilters = false,
      bool projectIsArchived = false, bool generatingForThumbnail = false)
    {
      _log.LogInformation($"{nameof(GetMapBitmap)}: projectUid={projectUid} generatingForThumbnail={generatingForThumbnail}");

      var bbox =
        $"{mapParameters.bbox.minLatDegrees},{mapParameters.bbox.minLngDegrees},{mapParameters.bbox.maxLatDegrees},{mapParameters.bbox.maxLngDegrees}";

      // caching only thumbnails at this stage
      if (generatingForThumbnail)
      {
        var cacheKey = GetCacheKey(projectUid, bbox, mapParameters.mapWidth, mapParameters.mapHeight, mode,
          filterUid, cutFillDesignUid, baseUid, topUid,
          volCalcType, explicitFilters);
        _log.LogDebug($"{nameof(GetMapBitmap)}: cacheKey={cacheKey}");
        
        var bitmapString = await _productivityDataCache.GetOrCreate(cacheKey, async entry =>
        {
          _log.LogDebug($"{nameof(GetMapBitmap)}: Retrieving ProductionData tile from 3dpSvc since it is not in cache");
          var bitmap = await _productivity3DProxyCompactionTile.GetProductionDataTile(projectUid, filterUid,
            cutFillDesignUid, (ushort) mapParameters.mapWidth, (ushort) mapParameters.mapHeight,
            bbox, mode, baseUid, topUid, volCalcType, customHeaders, explicitFilters);

          entry.AbsoluteExpiration = projectIsArchived ? DateTimeOffset.MaxValue : _expiration;
          return new CacheItem<string>(Convert.ToBase64String(bitmap), new[] {projectUid.ToString()});
        });
        _log.LogDebug($"{nameof(GetMapBitmap)}: gotBitmap via cache length={bitmapString.Length}");
        return Convert.FromBase64String(bitmapString);
      }

      return await _productivity3DProxyCompactionTile.GetProductionDataTile(projectUid,
        filterUid, cutFillDesignUid,
        (ushort) mapParameters.mapWidth, (ushort) mapParameters.mapHeight, bbox,
        mode, baseUid, topUid, volCalcType, customHeaders, explicitFilters);
    }

    /// <summary>
    /// Include all variables in key, in case we expand caching to include reportTiles or 3dpMaps.
    ///    thumbnails only include projectUid, bbox, width, height and mode
    /// </summary>
    private string GetCacheKey(Guid projectUid, string bbox, int mapWidth, int mapHeight, DisplayMode mode,
      Guid? filterUid, Guid? cutFillDesignUid, Guid? baseUid, Guid? topUid,
      VolumeCalcType? volCalcType, bool explicitFilters)
    {

      var keyPrefix = typeof(ProductionDataTileService).Name;
      return $"{keyPrefix}-{projectUid}--{bbox}--{mapWidth}--{mapHeight}--{mode}--{filterUid}--{cutFillDesignUid}--{baseUid}--{topUid}--{volCalcType}--{explicitFilters}";
    }

    [Notification( NotificationUidType.Project, ProjectChangedNotification.PROJECT_CHANGED_KEY)]
    // todo [Notification(NotificationUidType.Project, ProjectChangedNotification.PRODUCTION_DATA_CHANGED_KEY)]
    public void InvalidateProjectsFromCache(Guid guid)
    {
      var notificationAttributes =
        (NotificationAttribute) Attribute.GetCustomAttribute(typeof(NotificationAttribute), typeof(NotificationAttribute));
      _log.LogInformation($"{nameof(GetMapBitmap)}: ProjectChanged notification received. TypeOfUid:{notificationAttributes.Type} eventType:{notificationAttributes.Key} projectUid:{guid}");

      _productivityDataCache.RemoveByTag(guid.ToString());
    }

  }

  public interface IProductionDataTileService
  {
    Task<byte[]> GetMapBitmap(Guid projectUid, Guid? filterUid, Guid? cutFillDesignUid,
      MapParameters mapParameters, DisplayMode mode, Guid? baseUid, Guid? topUid, VolumeCalcType? volCalcType,
      IDictionary<string, string> customHeaders, bool explicitFilters,
      bool projectIsArchived, bool generatingForThumbnail );
  }
}
