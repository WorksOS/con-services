using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Cache.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies;
using VSS.Productivity.Push.Models.Attributes;
using VSS.Productivity.Push.Models.Enums;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity.Push.Models.Notifications.Models;

namespace VSS.DataOcean.Client.Models
{
  /// <summary>
  /// Tile Urls are only valid if DO has line-work for that Zxy.
  ///  If it has no line-work the call will return 403. This call takes 300-400 ms by the time Polly retries etc.
  ///  So once we know the Zxy is not retrievable, keep that unique url string in a cache.
  ///
  /// list contains strings like: {fileUID}_Tiles$/tiles/xyz/z/x/y.png e.g. d64bb990-8ae6-4e21-bf62-84395001790b_Tiles$/tiles/xyz/21/822028/378508.png
  ///     these are prefixed with "DataOceanMissingTileCache " and tagged with the fileUID
  /// 
  /// When a file is imported, it will take a period of time for the tiles to be generated.
  ///   We need to be aware of when this process ends ((ProjectFileRasterTilesGeneratedNotification) and the tiles become available.
  ///   At that stage remove any for that fileUid from the list as they may now be available.
  ///
  /// Cleanup cache every 24 hours as imported file (or indeed project) may have been deleted,
  ///      or is no longer viewed by the user.
  /// </summary>
  public class DataOceanMissingTileCache 
  {
    private readonly ILogger<DataOceanMissingTileCache> _log;
    private readonly IDataCache _dataCache;
    private static readonly AsyncDuplicateLock MemCacheLock = new AsyncDuplicateLock();
    private readonly MemoryCacheEntryOptions _options;
    
    public DataOceanMissingTileCache(IDataCache dataDataCache, IConfigurationStore config, ILoggerFactory logger)
    {
      _dataCache = dataDataCache;
      _log = logger.CreateLogger<DataOceanMissingTileCache>();

      const string DATA_OCEAN_TILE_CACHE_HOURS_KEY = "DATA_OCEAN_TILE_CACHE_HOURS";
      var tileCacheTimeHours = config.GetValueInt(DATA_OCEAN_TILE_CACHE_HOURS_KEY, 24);
      _options = new MemoryCacheEntryOptions() {AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(tileCacheTimeHours) };
    }

    public IDataCache GetCache() => _dataCache;

    public async Task<bool> IsTileKnownToBeMissing(string tileDetail)
    {
      var cacheKey = GetCacheKey(tileDetail);
      using (await MemCacheLock.LockAsync(cacheKey))
        return !string.IsNullOrEmpty(_dataCache.Get<string>(cacheKey));
    }

    public async Task CreateMissingTile(string tileDetail)
    {
      var cacheKey = GetCacheKey(tileDetail);
      var fileUid = tileDetail.Split(new[] { DataOceanFileUtil.GENERATED_TILE_FOLDER_SUFFIX }, StringSplitOptions.None)[0];

      using (await MemCacheLock.LockAsync(cacheKey))
      {
        await _dataCache.GetOrCreate(cacheKey, async entry =>
        {
          entry.SetOptions(_options);
          return new CacheItem<string>(cacheKey, new []{fileUid});
        });
      }
    }

    private void RemoveMissingTilesForFileUid(string fileUid)
    {
      if (!string.IsNullOrEmpty(fileUid))
        _dataCache.RemoveByTag(fileUid);
    }

    /// <summary>
    /// Handles the notification for DXF tiles having been generated
    /// </summary>
    [Notification(NotificationUidType.File, ProjectFileRasterTilesGeneratedNotification.PROJECT_FILE_RASTER_TILES_GENERATED_KEY)]
    public async Task RemoveFromMissingTileCache(object parameters)
    {
      RasterTileNotificationParameters result;
      try
      {
        result = JObject.FromObject(parameters).ToObject<RasterTileNotificationParameters>();
      }
      catch (Exception e)
      {
        _log.LogError(e, "Wrong parameters passed to dataOcean missing tile Cache");
        return;
      }

      _log.LogInformation($"{nameof(RemoveFromMissingTileCache)}: Received {ProjectFileRasterTilesGeneratedNotification.PROJECT_FILE_RASTER_TILES_GENERATED_KEY} notification: {JsonConvert.SerializeObject(result)}. Clear tileCache for fileUid.");
      RemoveMissingTilesForFileUid(result.FileUid.ToString());
    }

    /// <summary>
    ///   Gets the cache key. MemoryCache is shared so we need to construct a unique cache key by type of item and tileDetails.
    /// </summary>
    private string GetCacheKey(string tileDetail)
    {
      var keyPrefix = typeof(DataOceanMissingTileCache).Name;
      return $"{keyPrefix} {tileDetail}";
    }

  }
}
