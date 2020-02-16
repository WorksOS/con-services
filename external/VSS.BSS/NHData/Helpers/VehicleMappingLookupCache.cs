using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using log4net;
using MongoDB.Driver.Builders;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.NHDataSvc.DataAccess;
using System.Timers;

namespace VSS.Nighthawk.NHDataSvc.Helpers
{
  public class VehicleMappingLookupCache
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static readonly ObjectCache Cache = MemoryCache.Default;
    private readonly IDocumentStore<VehicleMapping> _vehicleMappingRepository;

    private static long _bookmarkTotalAssetCount;
    private static DateTime _bookmarkUpdateUtc = DateTime.MinValue;
    private static long _bookmarkUpdateUtcAssetCount;

    private Timer _cacheRefreshTimer;
    private TimeSpan _cacheRefreshWaitPeriod = new TimeSpan(0, 0, 0);

    public VehicleMappingLookupCache(IDocumentStore<VehicleMapping> vehicleMappingRepository)
    {
      if (vehicleMappingRepository == null)
        throw new ArgumentNullException("vehicleMappingRepository", @"The dependency cannot be null.");

      _vehicleMappingRepository = vehicleMappingRepository;
    }

    public void Initialize()
    {
      try
      {
        // Set cache to blank to avoid NullReferenceException
        var activeAssetIDs = new List<long>();
        Cache.Set("activeAssetIDs", activeAssetIDs, null);

        CacheAssetIDs();
      }
      catch (Exception ex)
      {
        Log.ErrorFormat("NHDataSvc.VehicleMappingLookupCache Initialize failed: {0}", ex);
      }

      _cacheRefreshWaitPeriod = !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["VehicleMappingCacheRefreshInterval"])
                                ? TimeSpan.Parse(ConfigurationManager.AppSettings["VehicleMappingCacheRefreshInterval"])
                                : new TimeSpan(0, 0, 3, 0);
      InitializeCacheRefreshTimer();
    }

    private void InitializeCacheRefreshTimer()
    {
      if ((ushort)_cacheRefreshWaitPeriod.TotalMilliseconds == 0)
      {
        _cacheRefreshWaitPeriod = new TimeSpan(0, 0, 10);
      }

      _cacheRefreshTimer = new Timer(_cacheRefreshWaitPeriod.TotalMilliseconds);
      _cacheRefreshTimer.Elapsed += HandleCacheRefreshTimerElapsed;
      _cacheRefreshTimer.Start();
      Log.IfInfoFormat("{0} started VehicleMapping cache refresh timer. Cache refresh interval is set to {1}", "NHDataSvc", _cacheRefreshWaitPeriod);
    }

    private void HandleCacheRefreshTimerElapsed(object sender, ElapsedEventArgs eventArgs)
    {
      Log.Debug("VehicleMapping Cache refresh timer elapsed...");

      try
      {
        CacheAssetIDs();
      }
      catch (Exception exception)
      {
        Log.ErrorFormat("NHDataSvc.VehicleMappingLookupCache RefreshTimer failed: {0}", exception);
      }
    }

    public void Release()
    {
      _cacheRefreshTimer.Stop();

      Log.Info("NHDataSvc VehicleMapping cache refresh timer stopped.");
    }

    public void CacheAssetIDs()
    {
      if (!IsCacheRefreshRequired()) return;

      var activeAssetIDs = _vehicleMappingRepository.MongoDocumentCollection.Find(Query.EQ(DocumentFields.IsActive, true)).Select(x => x.AssetId).ToList();
      
      activeAssetIDs.Sort();

      long activeAssetIDsCount = activeAssetIDs.Count;

      Cache.Set("activeAssetIDs", activeAssetIDs, null);

      Log.IfInfoFormat("Refreshed VehicleMappingLookupCache. Cache Size: {0}. Bookmarks - bookmarkTotalAssetCount:{1}, bookmarkUpdateUtc:{2}, bookmarkUpdateUtcAssetCount:{3}", activeAssetIDsCount, _bookmarkTotalAssetCount, _bookmarkUpdateUtc, _bookmarkUpdateUtcAssetCount);
    }

    private bool IsCacheRefreshRequired()
    {
      // Cache needs to be refreshed in following scenarios
      // a. Change in VehicleMapping collection count. Occurs when assets are added/removed from VehicleMapping collection.
      // b. Change in last UpdateUtc. Occurs when an existing asset is updated as active/inactive.
      // c. Change in number of assets in last UpdateUtc(corner case). More than an asset is updated on the same datetime.
      
      var totalAssetCount = _vehicleMappingRepository.MongoDocumentCollection.Count();

      if ((totalAssetCount == 0) && (_bookmarkTotalAssetCount == 0))
        return false; // Nothing to validate

      var refreshCache = false;

      var lastUpdatedAssets = _vehicleMappingRepository.MongoDocumentCollection
                                                       .Find(Query.GTE(DocumentFields.UpdateUtc, _bookmarkUpdateUtc))
                                                       .ToList();

      var lastUpdatedUtc = (lastUpdatedAssets.Count > 0) ? lastUpdatedAssets.Max(x => x.UpdateUtc) : DateTime.MinValue;

      var lastUpdatedAssetCount = lastUpdatedAssets.Count(x => x.UpdateUtc == lastUpdatedUtc);

      if (totalAssetCount != _bookmarkTotalAssetCount)
      {
        _bookmarkTotalAssetCount = totalAssetCount;
        refreshCache = true;
      }

      if (lastUpdatedUtc != _bookmarkUpdateUtc)
      {
        _bookmarkUpdateUtc = lastUpdatedUtc;
        refreshCache = true;
      }

      if (lastUpdatedAssetCount != _bookmarkUpdateUtcAssetCount)
      {
        _bookmarkUpdateUtcAssetCount = lastUpdatedAssetCount;
        refreshCache = true;
      }

      return refreshCache;
    }

    private static List<long> GetActiveAssetIDs()
    {
      return (List<long>)Cache.Get("activeAssetIDs");
    }

    public static bool IsAssetIdActive(long assetId)
    {
      return GetActiveAssetIDs().BinarySearch(assetId) > 0;
    }
  }
}
